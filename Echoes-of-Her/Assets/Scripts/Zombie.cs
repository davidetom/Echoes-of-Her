using UnityEngine;

public class Zombie : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        rb.gravityScale = 12f;
    }

    protected override void Awake()
   {
        base.Awake();
   }

   protected override bool Grounded()
   {
        return base.Grounded();
   }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if(!isRecoiling && Grounded())
        {
            // Calculate direction to player (only on X axis)
            float direction = Mathf.Sign(PlayerController.Instance.transform.position.x - transform.position.x);
            
            // Apply velocity instead of directly moving transform
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
            
            // Optional: Face the player
            transform.localScale = new Vector3(direction * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce) 
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);
    }
}
