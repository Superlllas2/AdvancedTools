using UnityEngine;

public class Box : MonoBehaviour
{
    public PushBallAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TargetZone"))
        {
            Debug.Log("Ball entered TargetZone");
            agent.NotifyBallEnteredGoal();
        }
    }
}