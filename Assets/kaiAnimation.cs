using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class kaiAnimation : MonoBehaviour
{
    private Rigidbody2D rb;
    public Animator anim;
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
    private bool isFallingToDeath = false;

    [Header("Ajustes de Suelo")]
    public float extraGravity = 2f;
    public float groundCheckOffset = 0.1f;

    [Header("Salud")]
    public int maxHealth = 5;
    private static int currentHealth = -1;

    [Header("UI de Corazones")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Caída y Reinicio")]
    public float fallThresholdY = -10f;

    [Header("Reaparición")]
    public Transform respawnPoint;
    
    [Header("Arma")]
    private bool hasWeapon = false;
    public Image weaponIconUI; 
    public Image batteryMeterUI;

    [Header("Disparo")]
    public GameObject electroshockPrefab; 
    public Transform firePoint;          
    
    [Header("Batería del Arma")]
    public int maxAmmo = 10; 
    private int currentAmmo;
    public Sprite[] batterySprites; 

    [Header("Controles Táctiles Manuales")]
    public Canvas myCanvas; 
    public RectTransform moveLeftButtonRect;  
    public RectTransform moveRightButtonRect; 
    public RectTransform jumpButtonRect;      
    public RectTransform fireButtonRect;      

    private Camera canvasCamera; 
    private bool isMoving = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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

        if (weaponIconUI != null)
        {
            weaponIconUI.enabled = false;
        }
        
        if(batteryMeterUI != null){
            batteryMeterUI.enabled = false;
        }

        if (myCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = null;
        }
        else
        {
            canvasCamera = myCanvas.worldCamera;
        }
    }

    void Update()
    {
        HandleTouchInput(); 

        if (groundCheck == null) return;

        Vector2 groundCheckPos = groundCheck.position + Vector3.down * groundCheckOffset;
        isGrounded = Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            isFallingToDeath = false;
        }

        anim.SetFloat("speed", Mathf.Abs(moveInput));
        anim.SetBool("isGrounded", isGrounded); 

        if (transform.position.y < fallThresholdY && !isFallingToDeath)
        {
            isFallingToDeath = true; 
            HandleFall();
        }
    }

    void HandleTouchInput()
    {
        bool pressingJump = false;
        bool pressingFire = false;
        bool pressingLeft = false;
        bool pressingRight = false;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Vector2 touchPos = Input.GetTouch(i).position;
                if (CheckTouchOnRect(touchPos, moveLeftButtonRect)) pressingLeft = true;
                if (CheckTouchOnRect(touchPos, moveRightButtonRect)) pressingRight = true;
                if (CheckTouchOnRect(touchPos, jumpButtonRect) && Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began) pressingJump = true;
                if (CheckTouchOnRect(touchPos, fireButtonRect) && Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began) pressingFire = true;
            }
        }
        else if (Input.GetMouseButton(0)) 
        {
            Vector2 mousePos = Input.mousePosition;
            if (CheckTouchOnRect(mousePos, moveLeftButtonRect)) pressingLeft = true;
            if (CheckTouchOnRect(mousePos, moveRightButtonRect)) pressingRight = true;
            if (CheckTouchOnRect(mousePos, jumpButtonRect) && Input.GetMouseButtonDown(0)) pressingJump = true;
            if (CheckTouchOnRect(mousePos, fireButtonRect) && Input.GetMouseButtonDown(0)) pressingFire = true;
        }

        if (pressingLeft)
        {
            OnPointerDownMove(-1); 
            isMoving = true;
        }
        else if (pressingRight)
        {
            OnPointerDownMove(1);
            isMoving = true;
        }
        else if (isMoving) 
        {
            isMoving = false;
            OnPointerUpMove();
        }

        if (pressingJump)
        {
            OnTouchJump();
        }
        if (pressingFire)
        {
            OnTouchFire();
        }
    }

    bool CheckTouchOnRect(Vector2 touchPosition, RectTransform rect)
    {
        if (rect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition, canvasCamera);
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
        rb.linearVelocity = targetVelocity;

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.down * extraGravity * Time.fixedDeltaTime;
        }
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

    public void LoseLife()
    {
        currentHealth--;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Debug.Log("GAME OVER");
            currentHealth = maxHealth; 
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("Vida perdida. Vidas restantes: " + currentHealth);
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            rb.linearVelocity = Vector2.zero; 
            isFallingToDeath = false;
        }
        else
        {
            Debug.LogError("¡No se ha asignado un Respawn Point en el Inspector!");
        }
    }

    public void SetRespawnPoint(Transform newPoint)
    {
        if (respawnPoint != newPoint)
        {
            respawnPoint = newPoint;
            Debug.Log("¡Nuevo punto de reaparición guardado en: " + newPoint.name);
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
        currentAmmo = maxAmmo;
        if (weaponIconUI != null)
        {
            weaponIconUI.enabled = true;
        }

        if (batteryMeterUI != null)
            batteryMeterUI.enabled = true;

        UpdateBatteryUI();
        Debug.Log("¡Arma Recogida!");
    }

    public void OnFire(InputValue value)
    {
        if (!value.isPressed) return; 

        if (hasWeapon && currentAmmo > 0)
        {
            currentAmmo--;
            UpdateBatteryUI(); 
            
            anim.SetTrigger("fire"); 
            Debug.Log("OnFire: Tiene arma, animación 'fire' disparada.");

            if (electroshockPrefab != null && firePoint != null)
            {
                GameObject projectileGO = Instantiate(electroshockPrefab, firePoint.position, firePoint.rotation);
                ElectroshockProjectile projectile = projectileGO.GetComponent<ElectroshockProjectile>();
                
                if (projectile != null)
                {
                    Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
                    projectile.SetDirection(direction);
                }
            }
            else
            {
                Debug.LogWarning("Electroshock Prefab o Fire Point no asignados!");
            }
        }
        else if (hasWeapon && currentAmmo <= 0)
        {
            Debug.Log("¡Batería vacía! *click* *click*");
        }
    }

    public void RechargeBattery()
    {
        if (currentAmmo == maxAmmo) 
        {
            Debug.Log("La batería ya está llena.");
            return; 
        }

        Debug.Log("¡Recargando batería!");
        currentAmmo = maxAmmo;
        
        UpdateBatteryUI();
    }
    
    void UpdateBatteryUI()
    {
        if (batteryMeterUI == null || batterySprites.Length < 4)
        {
            Debug.LogWarning("Battery Meter UI o Battery Sprites no están configurados.");
            return;
        }

        float ammoPercentage = (float)currentAmmo / maxAmmo;

        if (currentAmmo == 0)
        {
            batteryMeterUI.sprite = batterySprites[3];
        }
        else if (ammoPercentage <= 0.33f)
        {
            batteryMeterUI.sprite = batterySprites[2];
        }
        else if (ammoPercentage <= 0.75f)
        {
            batteryMeterUI.sprite = batterySprites[1];
        }
        else
        {
            batteryMeterUI.sprite = batterySprites[0];
        }
    }

    public void OnPointerDownMove(float direction)
    {
        moveInput = direction;
    }

    public void OnPointerUpMove()
    {
        moveInput = 0f;
    }

    public void OnTouchJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnTouchFire()
    {
        if (hasWeapon && currentAmmo > 0) 
        {
            currentAmmo--; 
            UpdateBatteryUI(); 
            anim.SetTrigger("fire"); 
            
            if (electroshockPrefab != null && firePoint != null)
            {
                GameObject projectileGO = Instantiate(electroshockPrefab, firePoint.position, firePoint.rotation);
                ElectroshockProjectile projectile = projectileGO.GetComponent<ElectroshockProjectile>();
                if (projectile != null)
                {
                    Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
                    projectile.SetDirection(direction);
                }
            }
        }
        else if (hasWeapon && currentAmmo <= 0)
        {
            Debug.Log("¡Batería vacía! *click* *click*");
        }
    }
}