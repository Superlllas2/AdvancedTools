using System;
using UnityEngine;

public class Box : MonoBehaviour
{
    public PushBallAgent agent;
    public static event Action<Collider> OnBoxReachedGoal;
    
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TargetZone"))
        {
            //Debug.Log("Ball entered TargetZone");
            OnBoxReachedGoal?.Invoke(other);
            agent.NotifyBallEnteredGoal();
        }
    }
}