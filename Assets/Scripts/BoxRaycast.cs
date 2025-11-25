using System;
using UnityEngine;

public class BoxRaycast : MonoBehaviour
{
    public PushBallAgentRayCast agent;
    public static event Action OnBoxReachedGoal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TargetZone"))
        {
            Debug.Log("Ball entered TargetZone");
            OnBoxReachedGoal?.Invoke();
            agent.NotifyBallEnteredGoal();
        }
    }
}