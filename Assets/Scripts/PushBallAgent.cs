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
    private float previousDistanceToBall;
    private float previousBallToTargetDist;
    private bool episodeShouldEnd = false;
    private float episodeEndTimer = 0f;
    private bool ballInTargetZone = false;

    public float torqueMultiplier = 1f;
    public float forceMultiplier = 10f;
    

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        startAgentPos = transform.position;
        startBallPos = ball.position;
    }
    
    
    private void FixedUpdate()
    {
        if (episodeShouldEnd)
        {
            episodeEndTimer += Time.fixedDeltaTime;
        
            // Check if ball reached the zone in time
            if (ballInTargetZone)
            {
                AddReward(2f); // delayed success
                Debug.Log("Ball entered TargetZone");
                episodeShouldEnd = false;
                EndEpisode();
            }
            else if (episodeEndTimer > 1.0f) // wait max 1 second
            {
                AddReward(-1f); // fail
                EndEpisode();
            }
        }
    }
    
    public void NotifyBallEnteredGoal()
    {
        Debug.Log("Notification Entered Goal");
        ballInTargetZone = true;
        episodeShouldEnd = true;
        episodeEndTimer = 0f;
    }

    public void NotifyBallFailed()
    {
        AddReward(-3f);
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log("EPISODE START");
        episodeShouldEnd = false;
        episodeEndTimer = 0f;
        ballInTargetZone = false;
        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        previousDistanceToBall = Vector3.Distance(transform.position, ball.position);
        previousBallToTargetDist = Vector3.Distance(ball.position, target.position);
        
        agentRb.MovePosition(startAgentPos);
        ballRb.MovePosition(startBallPos);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((ball.position - transform.position).normalized); // direction to ball
        sensor.AddObservation((target.position - ball.position).normalized); // target direction
        sensor.AddObservation(agentRb.linearVelocity);
        sensor.AddObservation(agentRb.angularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Dynamic reward for distance
        var currentDistance = Vector3.Distance(transform.position, ball.position);
        var distanceDelta = previousDistanceToBall - currentDistance;
        previousDistanceToBall = currentDistance;
        
        // The reward for getting ball closer to the target
        var currentDist = Vector3.Distance(ball.position, target.position);
        var delta = previousBallToTargetDist - currentDist;
        if (distanceDelta > 0f)
        {
            AddReward(distanceDelta * 0.05f); // reward for getting closer
        }
        previousBallToTargetDist = currentDist;
        
        // Penalize spinning
        var angularVelocityPenalty = Vector3.Magnitude(agentRb.angularVelocity);
        AddReward(-0.005f * angularVelocityPenalty);
        
        // Facing the ball and/or facing the goal when pushing.
        var toBall = (ball.position - transform.position).normalized;
        var alignment = Vector3.Dot(transform.forward, toBall);
        AddReward(0.01f * alignment);  // Positive if agent looks toward ball
        
        // Telling to move
        var move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        var rotate = actions.ContinuousActions[2];

        agentRb.AddForce(move * forceMultiplier);
        
        var maxAngularVelocity = 1f;
        if (agentRb.angularVelocity.magnitude < maxAngularVelocity)
        {
            agentRb.AddTorque(Vector3.up * rotate * torqueMultiplier);
        }
        
        // Punishment for being curious... :(
        if (Vector3.Distance(transform.position, ball.position) > previousDistanceToBall + 0.1f)
        {
            AddReward(-0.01f); // discourage wandering
        }
        
        // For staying close to the ball
        var distanceToBall = Vector3.Distance(transform.position, ball.position);
        AddReward(1f / (distanceToBall + 1f) * 0.001f);
        
        //Speed penalty
        var speedPenalty = agentRb.linearVelocity.magnitude > 5f ? -0.05f : 0f;
        AddReward(speedPenalty);

        // Time penalty
        AddReward(-0.001f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("Agent touched the ball");
            var toTarget = (target.position - ball.position).normalized;
            var ballVelocity = ballRb.linearVelocity.normalized;
            var alignment = Vector3.Dot(toTarget, ballVelocity);
            AddReward(0.5f * Mathf.Clamp01(alignment)); // only reward good pushes
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("TargetZone") && other.attachedRigidbody == ballRb)
        // {
        //     Debug.Log("Ball hits the zone");
        //     AddReward(2f);
        //     EndEpisode();
        // }

        if (other.CompareTag("ResetZone"))
        {
            Debug.Log("The agent entered the reset zone");
            episodeShouldEnd = true;
            episodeEndTimer = 0f;
            AddReward(-2f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Horizontal");
        continuous[1] = Input.GetAxis("Vertical");
        continuous[2] = Input.GetKey(KeyCode.Q) ? -1f : Input.GetKey(KeyCode.E) ? 1f : 0f;
    }
}
