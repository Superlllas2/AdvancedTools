using UnityEngine;

public class BoxCornerDetector : MonoBehaviour
{
    public int wallsTouching = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            wallsTouching++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            wallsTouching--;
        }
    }
}