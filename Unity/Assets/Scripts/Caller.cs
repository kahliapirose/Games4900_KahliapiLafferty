using UnityEngine;

public class Caller : MonoBehaviour
{
    public Receiver receiver;

    void Start()
    {
        Debug.Log("Hello Friend");
        receiver.OnCalled();
    }

}
