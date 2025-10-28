using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class kaiAnimation : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    public Transform spriteTransform;

    [Header("Movimiento")]
    public float moveSpeed = 7f;
    private float moveInput;
    private bool isFacingRight = true;

    [Header("Salto")]
    public float jumpForce = 12f;
    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Ajustes de Suelo")]
    public float extraGravity = 2f;
    public float groundCheckOffset = 0.1f;

    [Header("Salud")]
    public int maxHealth = 5;
    private static int currentHealth = -1; // Static health

    [Header("UI de Corazones")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Caída y Reinicio")]
    public float fallThresholdY = -10f;

    [Header("Arma")]
    private bool hasWeapon = false; 
    public Image weaponIconUI;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        if (spriteTransform == null) { Debug.LogError("Sprite Transform no asignado en " + this.name); this.enabled = false; }
        if (anim == null) { Debug.LogError("Animator no encontrado en los hijos de " + this.name); this.enabled = false; }
        if (groundCheck == null) { Debug.LogError("GroundCheck no asignado en " + this.name); this.enabled = false; }
        if (rb == null) { Debug.LogError("Rigidbody2D no encontrado en " + this.name); this.enabled = false; }

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        if (currentHealth == -1)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI();

        // Inicializar UI del arma (oculta al inicio)
        if (weaponIconUI != null)
        {
            weaponIconUI.enabled = false; 
        } else {
             Debug.LogWarning("Weapon Icon UI no asignado en el Inspector!");
        }
    }

    void Update()
    {
        if (groundCheck == null) return;

        Vector2 groundCheckPos = groundCheck.position + Vector3.down * groundCheckOffset;
        isGrounded = Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, groundLayer);

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.down * extraGravity * Time.deltaTime;
        }

        anim.SetFloat("speed", Mathf.Abs(moveInput));
        anim.SetBool("isGrounded", isGrounded);

        if (transform.position.y < fallThresholdY)
        {
            HandleFall();
        }
    }

    void LateUpdate()
    {
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, 0.8f);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<float>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = spriteTransform.localScale;
        scaler.x *= -1;
        spriteTransform.localScale = scaler;
    }

    void HandleFall()
    {
        LoseLife();
    }

    void LoseLife()
    {
        currentHealth--;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Debug.Log("GAME OVER");
            currentHealth = maxHealth; // Reset static health for next game
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void UpdateHealthUI()
    {
        if (hearts == null || fullHeart == null || emptyHeart == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }

            if (i < maxHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }

    public void SnapToGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
        }
    }

     private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 checkPositionOffset = groundCheck.position + Vector3.down * groundCheckOffset;
            Gizmos.DrawWireSphere(checkPositionOffset, groundCheckRadius);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.5f);
        }
    }

    public void PickUpWeapon()
    {
        hasWeapon = true;
        if (weaponIconUI != null)
        {
            weaponIconUI.enabled = true; 
        }
        Debug.Log("¡Arma Recogida!");
    }

    public void OnFire(InputValue value)
    {
        if (hasWeapon && value.isPressed) 
        {
            anim.SetTrigger("fire"); 
            Debug.Log("¡Disparando!");
        }
    }
}