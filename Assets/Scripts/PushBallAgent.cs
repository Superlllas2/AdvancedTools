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

    public float forceMultiplier = 10f;

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        startAgentPos = transform.localPosition;
        startBallPos = ball.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("EPISODE START");
        agentRb.linearVelocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        previousDistanceToBall = Vector3.Distance(transform.position, ball.position);
        
        agentRb.MovePosition(startAgentPos);
        ballRb.MovePosition(startBallPos);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(ball.position);
        sensor.AddObservation(target.position);
        sensor.AddObservation(ballRb.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 force = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        agentRb.AddForce(force * forceMultiplier);

        float currentDistance = Vector3.Distance(transform.position, ball.position);
        float distanceDelta = previousDistanceToBall - currentDistance;
        AddReward(distanceDelta * 0.01f); // reward for getting closer
        previousDistanceToBall = currentDistance;

        AddReward(-0.001f); // time penalty
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            AddReward(0.5f); // Reward for touching the ball
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TargetZone") && other.attachedRigidbody == ballRb)
        {
            AddReward(1f);
            EndEpisode();
        }

        if (other.CompareTag("ResetZone"))
        {
            AddReward(-2f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Horizontal");
        continuous[1] = Input.GetAxis("Vertical");
    }
}
