using UnityEngine;

public class BallGoalTrigger : MonoBehaviour
{
    public PushBallAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            agent.NotifyBallEnteredGoal();
        }
    }
}
