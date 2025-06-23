using UnityEngine;

public class Test : MonoBehaviour
{
    void OnTriggerEnter(Collider other )
    {
        if (other.CompareTag("TargetZone"))
        {
            Debug.Log("Ball entered the zone");
            GetComponent<Rigidbody>().AddForce(Vector3.up * 50f, ForceMode.Impulse);
        }
    }
}
