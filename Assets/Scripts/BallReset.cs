using UnityEngine;

public class BallReset : MonoBehaviour
{
    public PushBallAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResetZone"))
        {
            Debug.Log("Ball touched ResetZone. Ending episode.");
            agent.AddReward(-0.5f);
            agent.EndEpisode();
        }
    }
}

