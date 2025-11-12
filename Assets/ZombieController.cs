using UnityEngine;
using System.Collections;

public class ZombieController : MonoBehaviour
{
    protected Rigidbody2D rb;
    public Animator anim;
    public Transform spriteTransform;
    
    [Header("Configuración General")]
    public float moveSpeed = 2f;
    public int maxHealth = 3;
    protected int currentHealth;
    public LayerMask playerLayer;
    public GameObject deathEffectPrefab;
    
    [Header("Detección")]
    public Vector2 detectionBoxSize = new Vector2(5f, 2f);
    public Vector2 attackBoxSize = new Vector2(1f, 1f);
    public Transform playerCheck;
    public Transform groundDetection;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    
    [Header("Ataque")]
    public float attackCooldown = 1.5f;
    protected float lastAttackTime;
    
    protected bool movingRight = true; 
    protected Transform player;
    protected bool isAttacking = false;
    protected bool isDying = false;
    protected bool isTakingHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        rb.freezeRotation = true; // Evita que se caiga de lado
    }

    void Update()
    {
        if (isDying || isTakingHit || isAttacking) return;

        FindPlayer(); 

        // --- LÓGICA DE ESTADO MEJORADA ---
        bool canSeePlayer = (player != null);
        bool canAttackPlayer = false;

        if (canSeePlayer && playerCheck != null)
        {
            // Revisa si el jugador está EN RANGO DE ATAQUE
            Collider2D playerInAttackRange = Physics2D.OverlapBox(playerCheck.position, attackBoxSize, 0, playerLayer);
            if (playerInAttackRange != null)
            {
                canAttackPlayer = true;
            }
        }
        
        // Ahora decidimos qué hacer:
        if (canAttackPlayer)
        {
            AttackPlayer();
        }
        else if (canSeePlayer) // Si te veo, pero no te puedo atacar...
        {
            ChasePlayer();
        }
        else // Si no te veo...
        {
            Patrol();
        }
        // --- FIN DE LA LÓGICA ---

        if(anim != null)
        {
            anim.SetBool("isAttacking", isAttacking);
            anim.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    protected virtual void FindPlayer()
    {
        if (playerCheck == null) return;
        
        if (player == null)
        {
            Collider2D playerCollider = Physics2D.OverlapBox(playerCheck.position, detectionBoxSize, 0, playerLayer);
            if (playerCollider != null)
            {
                player = playerCollider.transform;
            }
        }
        else
        {
            if (!player.gameObject.activeSelf || Vector2.Distance(playerCheck.position, player.position) > (detectionBoxSize.x * 1.5f))
            {
                player = null;
            }
        }
    }

    protected virtual void Patrol()
    {
        if (groundDetection == null) return;

        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundCheckDistance, groundLayer);

        if (groundInfo.collider == null) 
        {
            if (isGrounded()) 
            {
                movingRight = !movingRight;
                Flip();
            }
        }

        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
    }
    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying || isTakingHit || isAttacking || collision.gameObject.CompareTag("Player")) return;

        if (collision.contacts.Length > 0)
        {
            float normalX = collision.contacts[0].normal.x;
            if (Mathf.Abs(normalX) > 0.3f)
            {
                if (normalX < 0 && movingRight)
                {
                    movingRight = false;
                    Flip();
                }
                else if (normalX > 0 && !movingRight)
                {
                    movingRight = true;
                    Flip();
                }
            }
        }
    }

    protected virtual void ChasePlayer()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(Mathf.Sign(direction) * moveSpeed * 1.2f, rb.linearVelocity.y); 

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
    }

    protected virtual void AttackPlayer()
    {
        if (player == null) return;
        
        rb.linearVelocity = Vector2.zero; 
        float direction = player.position.x - transform.position.x;
        if (direction > 0 && !movingRight) { movingRight = true; Flip(); }
        else if (direction < 0 && movingRight) { movingRight = false; Flip(); }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            if(anim != null) anim.SetTrigger("attack"); 
            
            StartCoroutine(ResetAttackState(1.0f));
        }
    }

    public void DealDamageFromAnimation()
    {
        if (playerCheck == null) return;
        Collider2D playerHit = Physics2D.OverlapBox(playerCheck.position, attackBoxSize, 0, playerLayer);
        if (playerHit != null)
        {
            kaiAnimation playerHealth = playerHit.GetComponent<kaiAnimation>();
            if (playerHealth != null)
            {
                playerHealth.LoseLife();
            }
        }
    }

    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
    }

    private IEnumerator ResetAttackState(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isAttacking) isAttacking = false; 
    }
    
    protected void Flip()
    {
        if (spriteTransform == null) return;
        Vector3 scaler = spriteTransform.localScale;
        scaler.x *= -1;
        spriteTransform.localScale = scaler;
    }
    
    protected bool isGrounded()
    {
        if (groundDetection == null) return false;
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundCheckDistance, groundLayer);
        return groundInfo.collider != null;
    }

    public void TakeDamage(int damage)
    {
        if (isDying || isTakingHit) return;

        currentHealth -= damage;
        isTakingHit = true; 
        rb.linearVelocity = Vector2.zero;
        
        if(anim != null) anim.SetTrigger("hit"); 
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(ResetHitState(0.3f)); 
        }
    }
    
    private IEnumerator ResetHitState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTakingHit = false;
    }

    protected void Die()
    {
        if (isDying) return;
        isDying = true;
        
        if(anim != null) anim.SetTrigger("die"); 
        
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; 
        
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach(Collider2D col in colliders) col.enabled = false;

        this.enabled = false; 
        
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject, 3f); 
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (playerCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(playerCheck.position, detectionBoxSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(playerCheck.position, attackBoxSize);
        }

        if (groundDetection != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundDetection.position, groundDetection.position + Vector3.down * groundCheckDistance);
        }
    }
}