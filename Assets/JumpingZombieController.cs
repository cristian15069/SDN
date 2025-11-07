using UnityEngine;

public class JumpingZombieController : ZombieController
{
    [Header("Saltar Zombie")]
    public float jumpForce = 8f;
    public float jumpCooldown = 3f;
    protected float lastJumpTime;
    public float jumpDetectionRange = 3f; // Rango para decidir si saltar para alcanzar al jugador
    public Transform ceilingCheck; // Para evitar saltar y golpearse la cabeza
    public float ceilingCheckRadius = 0.2f;

    protected override void ChasePlayer()
    {
        base.ChasePlayer(); 

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= jumpDetectionRange && player.position.y > transform.position.y + 0.5f)
            {
                TryJump();
            }
        }
    }

    protected void TryJump()
    {
        if (groundDetection == null || ceilingCheck == null) return;
        
        bool isGrounded = Physics2D.OverlapCircle(groundDetection.position, groundCheckDistance, groundLayer);
        bool canJump = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer) == null;
        
        if (isGrounded && canJump && Time.time >= lastJumpTime + jumpCooldown)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if(anim != null) anim.SetTrigger("jump"); 
            lastJumpTime = Time.time;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }

    protected override void AttackPlayer()
{
    isAttacking = true;
    rb.linearVelocity = Vector2.zero;

    if (player != null)
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // Girar antes de atacar
        if (direction > 0 && !movingRight)
        {
            movingRight = true;
            Flip();
        }
        else if (direction < 0 && movingRight)
        {
            movingRight = false;
            Flip();
        }

        if (anim != null)
        {
            anim.SetTrigger("attack");
        }

        float attackDirection = movingRight ? 1f : -1f;
        Vector2 attackPos = (Vector2)playerCheck.position + Vector2.right * attackDirection * 0.5f;

        Collider2D playerHit = Physics2D.OverlapBox(attackPos, attackBoxSize, 0, playerLayer);
        if (playerHit != null)
        {
            kaiAnimation playerHealth = playerHit.GetComponent<kaiAnimation>();
            if (playerHealth != null)
            {
                playerHealth.LoseLife();
            }
        }

        lastAttackTime = Time.time;
    }
}

}