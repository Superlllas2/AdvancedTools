using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PushBallAgentRayCast : Agent
{
    public Transform ball;
    public Transform target;

    private Rigidbody agentRb;
    private Rigidbody ballRb;

    private Vector3 startAgentPos;
    private Vector3 startBallPos;

    private float previousBallToTargetDist;
    private bool ballInTargetZone = false;
    private BoxCollider spawnArea;
    private BoxCornerDetector cornerDetector;

    public float torqueMultiplier = 1f;
    public float forceMultiplier = 10f;

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        cornerDetector = ball.GetComponent<BoxCornerDetector>();
        
        var parent = transform.parent;
        if (parent)
        {
            var areaTransform = parent.Find("SpawnArea");
            if (areaTransform) spawnArea = areaTransform.GetComponent<BoxCollider>();
        }

        if (!spawnArea)
        {
            Debug.LogError("SpawnArea not found as sibling in environment: " + gameObject.name);
        }

        startAgentPos = transform.position;
        startBallPos = ball.position;

        ballRb.WakeUp();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("[EP START]");
        ballInTargetZone = false;

        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        transform.position = RandomSpawnPoint();
        ball.position = RandomSpawnPoint();

        // transform.position = startAgentPos;
        // ball.position = startBallPos;

        previousBallToTargetDist = Vector3.Distance(ball.position, target.position);
        SuccessTracker.totalEpisodes++;
        cornerDetector.wallsTouching = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentRb.linearVelocity);
        sensor.AddObservation(agentRb.angularVelocity);
        
        // Vector3 agentToBall = ball.position - transform.position;
        // sensor.AddObservation(agentToBall.normalized);
        // sensor.AddObservation(agentToBall.magnitude);
        //
        // Vector3 ballToTarget = target.position - ball.position;
        // sensor.AddObservation(ballToTarget.normalized);
        //
        // sensor.AddObservation(agentRb.linearVelocity);
        // sensor.AddObservation(agentRb.angularVelocity);
        //
        // sensor.AddObservation(target.position - transform.position); // optional
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var currentBallToTargetDist = Vector3.Distance(ball.position, target.position);
        var delta = previousBallToTargetDist - currentBallToTargetDist;
        previousBallToTargetDist = currentBallToTargetDist;

        if (delta > 0f)
            AddReward(0.05f * delta); // reward for progress

        // Vector3 ballDir = (target.position - ball.position).normalized;
        // float alignment = Vector3.Dot(ballRb.linearVelocity.normalized, ballDir);
        //
        // if (alignment > 0.8f && ballRb.linearVelocity.magnitude > 0.2f)
        //     AddReward(0.01f); // reward for good direction
        
        var agentToBall = (ball.position - transform.position).normalized;
        var ballToTarget = (target.position - ball.position).normalized;

        // Is the agent moving *toward* the ball in the right direction to push it to the goal?
        var agentApproach = Vector3.Dot(agentRb.linearVelocity.normalized, agentToBall);
        var pushAlignment = Vector3.Dot(agentToBall, ballToTarget);
        
        if (Physics.Raycast(transform.position, agentRb.linearVelocity.normalized, out RaycastHit hit, 0.6f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log(hit.collider.name);
                AddReward(-0.02f); // nudging into wall? stop.
            }
        }

        // Only reward if approach AND alignment are both good
        if (agentApproach > 0.8f && pushAlignment > 0.6f)
        {
            AddReward(0.1f);
        }

        if (agentRb.angularVelocity.magnitude > 3f)
            AddReward(-0.1f); // penalize spinning
        
        // Penalty for making box stuck in the corner
        if (cornerDetector)
        {
            if (cornerDetector.wallsTouching >= 2)
            {
                Debug.Log("Box stucked in the corner");
                NotifyBallFailed(-3);
            }
        }
        
        // Check if the agent is looking at the wall
        if (ballRb.linearVelocity.magnitude > 0.01f && ballRb.linearVelocity.magnitude < 0.1f)
        {
            if (Physics.Raycast(ball.position, ballRb.linearVelocity.normalized, out var wallHit, 0.3f) &&
                wallHit.collider.CompareTag("Wall"))
            {
                Debug.Log("Hits the wall - stuck");
                AddReward(-0.01f);
            }
        }
        

        var move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        var rotate = actions.ContinuousActions[2];

        agentRb.AddForce(move * forceMultiplier);

        if (agentRb.angularVelocity.magnitude < 1f)
            agentRb.AddTorque(Vector3.up * rotate * torqueMultiplier);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            AddReward(0.3f);

            var ballToTarget = (target.position - ball.position).normalized;
            var ballVelocity = ballRb.linearVelocity.normalized;
            var alignment = Vector3.Dot(ballVelocity, ballToTarget);

            if (alignment > 0.7f) AddReward(0.5f * alignment);
        }
    }

    public void NotifyBallEnteredGoal()
    {
        ballInTargetZone = true;
        SuccessTracker.successfulEpisodes++;
        AddReward(2f);
        EndEpisode();
        Debug.Log($"Success Rate: {(float)SuccessTracker.successfulEpisodes / SuccessTracker.totalEpisodes:P}");
    }

    private void NotifyBallFailed(float penalty)
    {
        AddReward(penalty);
        EndEpisode();
    }

    public void EndManuallyWithPenalty()
    {
        if (!ballInTargetZone)
        {
            var dist = Vector3.Distance(ball.position, target.position);
            AddReward(-Mathf.Clamp01(dist / 1.5f));
            AddReward(-1f);
        }
        EndEpisode();
    }
    
    Vector3 RandomSpawnPoint()
    {
        var bounds = spawnArea.bounds;
        var x = Random.Range(bounds.min.x, bounds.max.x);
        var z = Random.Range(bounds.min.z, bounds.max.z);
        var y = bounds.center.y + 0.1f;

        return new Vector3(x, y, z);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetAxis("Horizontal");
        c[1] = Input.GetAxis("Vertical");
        c[2] = Input.GetKey(KeyCode.Q) ? -1f : Input.GetKey(KeyCode.E) ? 1f : 0f;
    }
}