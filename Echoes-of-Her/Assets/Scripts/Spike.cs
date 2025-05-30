using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!PlayerController.Instance.pState.cutscene)
            {
                if (PlayerController.Instance.pState.alive)
                {
                    PlayerController.Instance.rb.linearVelocity = Vector2.zero;
                    PlayerController.Instance.TakeDamage(1, Vector2Int.zero);
                }
            }
            StartCoroutine(GameManager.Instance.Respawn());
        }
    }
}
