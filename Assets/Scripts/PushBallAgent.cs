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

    private float previousBallToTargetDist;
    private bool ballInTargetZone = false;

    public float torqueMultiplier = 1f;
    public float forceMultiplier = 10f;

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        startAgentPos = transform.position;
        startBallPos = ball.position;

        ballRb.WakeUp();
    }

    public override void OnEpisodeBegin()
    {
        ballInTargetZone = false;

        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        transform.position = startAgentPos;
        ball.position = startBallPos;

        previousBallToTargetDist = Vector3.Distance(ball.position, target.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 agentToBall = ball.position - transform.position;
        sensor.AddObservation(agentToBall.normalized);
        sensor.AddObservation(agentToBall.magnitude);

        Vector3 ballToTarget = target.position - ball.position;
        sensor.AddObservation(ballToTarget.normalized);

        sensor.AddObservation(agentRb.linearVelocity);
        sensor.AddObservation(agentRb.angularVelocity);

        sensor.AddObservation(target.position - transform.position); // optional
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float currentBallToTargetDist = Vector3.Distance(ball.position, target.position);
        float delta = previousBallToTargetDist - currentBallToTargetDist;
        previousBallToTargetDist = currentBallToTargetDist;

        if (delta > 0f)
            AddReward(0.05f * delta); // reward for progress

        Vector3 ballDir = (target.position - ball.position).normalized;
        float alignment = Vector3.Dot(ballRb.linearVelocity.normalized, ballDir);

        if (alignment > 0.8f && ballRb.linearVelocity.magnitude > 0.2f)
            AddReward(0.01f); // reward for good direction

        if (agentRb.angularVelocity.magnitude > 3f)
            AddReward(-0.1f); // penalize spinning

        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        float rotate = actions.ContinuousActions[2];

        agentRb.AddForce(move * forceMultiplier);

        if (agentRb.angularVelocity.magnitude < 1f)
            agentRb.AddTorque(Vector3.up * rotate * torqueMultiplier);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            AddReward(0.3f);

            Vector3 ballToTarget = (target.position - ball.position).normalized;
            Vector3 ballVelocity = ballRb.linearVelocity.normalized;
            float alignment = Vector3.Dot(ballVelocity, ballToTarget);

            if (alignment > 0.7f)
                AddReward(0.5f * alignment);
        }
    }

    public void NotifyBallEnteredGoal()
    {
        ballInTargetZone = true;
        AddReward(2f);
        EndEpisode();
    }

    public void NotifyBallFailed()
    {
        AddReward(-3f);
        EndEpisode();
    }

    public void EndManuallyWithPenalty()
    {
        if (!ballInTargetZone)
        {
            float dist = Vector3.Distance(ball.position, target.position);
            AddReward(-Mathf.Clamp01(dist / 1.5f));
            AddReward(-1f);
        }
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetAxis("Horizontal");
        c[1] = Input.GetAxis("Vertical");
        c[2] = Input.GetKey(KeyCode.Q) ? -1f : Input.GetKey(KeyCode.E) ? 1f : 0f;
    }
}