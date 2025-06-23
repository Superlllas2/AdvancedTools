using UnityEngine;

public class BallReset : MonoBehaviour
{
    public PushBallAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TargetZone"))
        {
            Debug.Log("Ball entered TargetZone");
            agent.NotifyBallEnteredGoal();
        }
        else if (other.CompareTag("ResetZone"))
        {
            Debug.Log("Ball touched ResetZone");
            agent.NotifyBallFailed();
        }
    }
}

