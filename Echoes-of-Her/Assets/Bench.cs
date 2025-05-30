using System;
using UnityEngine;

public class Bench : MonoBehaviour
{

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetButtonDown("Interact"))
        {
            GameManager.Instance.respawnPoint = transform.position;
            Debug.Log("Respawn set");
        }
    }
}
