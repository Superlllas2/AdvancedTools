using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PushBallAgent : Agent
{
    public Transform ball;
    public Transform target;

    private Rigidbody agentRb;
    private Rigidbody ballRb;

    private Vector3 startAgentPos;
    private Vector3 startBallPos;
    private Quaternion startBallRot;
    private Quaternion startAgentRot;

    private float previousBallToTargetDist;
    private bool ballInTargetZone = false;
    private BoxCollider spawnArea;
    private BoxCornerDetector cornerDetector;

    public float torqueMultiplier = 1f;
    public float forceMultiplier = 10f;

    // private static SuccessWindow Recent = new (100);

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        cornerDetector = ball.GetComponent<BoxCornerDetector>();

        // RANDOM SPAWN
        // var parent = transform.parent;
        // if (parent)
        // {
        //     var areaTransform = parent.Find("SpawnArea");
        //     if (areaTransform) spawnArea = areaTransform.GetComponent<BoxCollider>();
        // }
        //
        // if (!spawnArea)
        // {
        //     Debug.LogError("SpawnArea not found as sibling in environment: " + gameObject.name);
        // }

        startAgentPos = transform.position;
        startBallPos = ball.position;

        ballRb.WakeUp();
    }

    public override void OnEpisodeBegin()
    {
        ballInTargetZone = false;

        agentRb.velocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // RANDOM SPAWN 2
        // transform.position = RandomSpawnPoint();
        // ball.position = RandomSpawnPoint();

        // DEFAULT SPAWN
        transform.position = startAgentPos;
        transform.rotation = startAgentRot;

        ball.position = startBallPos;
        ball.rotation = startBallRot;

        previousBallToTargetDist = Vector3.Distance(ball.position, target.position);
        SuccessTracker.Add(0);
        if (cornerDetector) cornerDetector.wallsTouching = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 agentToBall = ball.position - transform.position;
        sensor.AddObservation(agentToBall.normalized);
        sensor.AddObservation(agentToBall.magnitude);

        Vector3 ballToTarget = target.position - ball.position;
        sensor.AddObservation(ballToTarget.normalized);
        sensor.AddObservation(ballToTarget.magnitude);

        sensor.AddObservation(agentRb.velocity);
        sensor.AddObservation(agentRb.angularVelocity);

        sensor.AddObservation(ballRb.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // OLD REWARDING SYSTEM
        // var currentBallToTargetDist = Vector3.Distance(ball.position, target.position);
        // var delta = previousBallToTargetDist - currentBallToTargetDist;
        // previousBallToTargetDist = currentBallToTargetDist;
        //
        // if (delta > 0f)
        //     AddReward(0.05f * delta); // reward for progress
        //
        // // Vector3 ballDir = (target.position - ball.position).normalized;
        // // float alignment = Vector3.Dot(ballRb.linearVelocity.normalized, ballDir);
        // //
        // // if (alignment > 0.8f && ballRb.linearVelocity.magnitude > 0.2f)
        // //     AddReward(0.01f); // reward for good direction
        //
        // var agentToBall = (ball.position - transform.position).normalized;
        // var ballToTarget = (target.position - ball.position).normalized;
        //
        // // Is the agent moving *toward* the ball in the right direction to push it to the goal?
        // var agentApproach = Vector3.Dot(agentRb.velocity.normalized, agentToBall);
        // var pushAlignment = Vector3.Dot(agentToBall, ballToTarget);
        //
        // if (Physics.Raycast(transform.position, agentRb.velocity.normalized, out RaycastHit hit, 0.6f))
        // {
        //     if (hit.collider.CompareTag("Wall"))
        //     {
        //         //Debug.Log(hit.collider.name);
        //         AddReward(-0.02f);
        //     }
        // }
        //
        // // Reward if approach AND alignment are both good
        // if (agentApproach > 0.8f && pushAlignment > 0.6f)
        // {
        //     AddReward(0.1f);
        // }
        //
        // if (agentRb.angularVelocity.magnitude > 3f)
        //     AddReward(-0.1f); // penalize spinning
        //
        // // Penalty for making box stuck in the corner
        // if (cornerDetector)
        // {
        //     if (cornerDetector.wallsTouching >= 2)
        //     {
        //         //Debug.Log("Box stucked in the corner");
        //         NotifyBallFailed(-3);
        //     }
        // }
        //
        // // Check if the agent is looking at the wall
        // if (ballRb.velocity.magnitude > 0.01f && ballRb.velocity.magnitude < 0.1f)
        // {
        //     if (Physics.Raycast(ball.position, ballRb.velocity.normalized, out var wallHit, 0.3f) &&
        //         wallHit.collider.CompareTag("Wall"))
        //     {
        //         //Debug.Log("Hits the wall - stuck");
        //         AddReward(-0.01f);
        //     }
        // }


        var move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        var rotate = actions.ContinuousActions[2];

        agentRb.AddForce(move * forceMultiplier);

        if (agentRb.angularVelocity.magnitude < 1f)
            agentRb.AddTorque(Vector3.up * rotate * torqueMultiplier);

        // Reward ball progress toward the goal
        var currentBallToTargetDist = Vector3.Distance(ball.position, target.position);
        var delta = previousBallToTargetDist - currentBallToTargetDist;
        previousBallToTargetDist = currentBallToTargetDist;
        if (delta > 0f) AddReward(0.1f * delta);

        // Step penalty
        AddReward(-0.001f);

        // If the box is stuck in a corner, end early with a light penalty.
        if (cornerDetector && cornerDetector.wallsTouching >= 2)
        {
            NotifyBallFailed(-1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            // NOISY REWARDS
            // AddReward(0.3f);
            //
            // var ballToTarget = (target.position - ball.position).normalized;
            // var ballVelocity = ballRb.velocity.normalized;
            // var alignment = Vector3.Dot(ballVelocity, ballToTarget);
            //
            // if (alignment > 0.7f) AddReward(0.5f * alignment);

            AddReward(0.2f);
        }
    }

    public void NotifyBallEnteredGoal()
    {
        ballInTargetZone = true;
        // SuccessTracker.successfulEpisodes++;
        SuccessTracker.Add(1);
        AddReward(3f);
        EndEpisode();
        // Debug.Log($"Success Rate: {(float)SuccessTracker.successfulEpisodes / SuccessTracker.totalEpisodes:P}");
        var rate = SuccessTracker.GetRate();
        Academy.Instance.StatsRecorder.Add("PushBall/SuccessRate", rate);
        Debug.Log($"Success Rate: {rate:P2}");
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