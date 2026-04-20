using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float height = 6f;
    public float speed = 3f;

    private Vector3 startPos;
    
    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.PingPong(Time.time * speed, height);
        transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
    }
}

