using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float hitForce;
    [SerializeField] private int speed;
    [SerializeField] private float lifetime = 1; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        transform.position += speed * transform.right;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            other.GetComponent<Enemy>().EnemyHit(damage, (other.transform.position -transform.position).normalized, hitForce);
        }
    }
}
