using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    public float launchPadForce = 10f;
    public float forwardBoost = 5f;

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(velocity.x + forwardBoost, launchPadForce, velocity.z);
    }
}