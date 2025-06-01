using Unity.VisualScripting;
using UnityEngine;

public class DeathStone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController.Instance.halfMana = false;
            UIManager.Instance.SwitchMana(UIManager.ManaState.FullMana);
            Destroy(gameObject);
        }
    }
}
