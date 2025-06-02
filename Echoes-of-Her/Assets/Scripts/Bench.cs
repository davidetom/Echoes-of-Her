using System;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bench : MonoBehaviour
{
    public bool interacted;

    private void Awake()
    {
        interacted = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetButtonDown("Interact"))
        {
            //GameManager.Instance.respawnPoint = transform.position;
            interacted = true;
            SaveData.Instance.benchSceneName = SceneManager.GetActiveScene().name;
            SaveData.Instance.benchPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            SaveData.Instance.SaveBench();
            Debug.Log("Respawn set");
        }
    }

    private void OTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetButtonDown("Interact"))
        {
            GameManager.Instance.respawnPoint = transform.position;
            interacted = false;
            Debug.Log("Respawn set");
        }
    }
}

