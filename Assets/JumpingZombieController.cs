using UnityEngine;

public class JumpingZombieController : ZombieController
{
    [Header("Configuracion de Salto")]
    public float jumpForce = 10f;
    public float forwardJumpForce = 4f; 
    public float jumpCooldown = 2f;
    public float jumpDetectionRange = 8f; 
    public float heightDetectionThreshold = 1.5f;

    [Header("Deteccion de Entorno")]
    public Transform ceilingCheck;
    public float ceilingCheckRadius = 0.2f;
    public Transform wallCheck; 
    public float wallCheckDistance = 1f;
    
    protected float lastJumpTime;

    protected override void ChasePlayer()
    {
        base.ChasePlayer(); 

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float heightDifference = player.position.y - transform.position.y;
            
            bool playerIsHigh = heightDifference > heightDetectionThreshold;
            bool playerIsClose = distanceToPlayer <= jumpDetectionRange;
            bool obstacleInFront = CheckForObstacle();

            if (playerIsClose && (playerIsHigh || obstacleInFront))
            {
                TryJump();
            }
        }
    }

    protected bool CheckForObstacle()
    {
        if (wallCheck == null) return false;

        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);

        return hit.collider != null;
    }

    protected void TryJump()
    {
        if (groundDetection == null || ceilingCheck == null) return;
        
        bool isGrounded = Physics2D.OverlapCircle(groundDetection.position, groundCheckDistance, groundLayer);
        bool cantJump = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
        
        if (isGrounded && !cantJump && Time.time >= lastJumpTime + jumpCooldown)
        {
            float direction = movingRight ? 1f : -1f;
            
            rb.linearVelocity = new Vector2(direction * forwardJumpForce, jumpForce);
            
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

        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 direction = movingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(direction * wallCheckDistance));
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jumpDetectionRange);
    }

    protected override void AttackPlayer()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        if (player != null)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);

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