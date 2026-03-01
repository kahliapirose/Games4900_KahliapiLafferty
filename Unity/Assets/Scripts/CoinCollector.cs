using UnityEngine;

public class CoinCollector : MonoBehaviour
{
   void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Coin Collected!");
        }
    }

}
