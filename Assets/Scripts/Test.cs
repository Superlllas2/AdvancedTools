using UnityEngine;

public class Test : MonoBehaviour
{
    void Start() {
        GetComponent<Rigidbody>().AddForce(Vector3.forward * 20f, ForceMode.Impulse);
    }
}
