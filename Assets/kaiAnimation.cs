using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // Necesario para Corrutinas

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public GameObject projectilePrefab;
    public int maxAmmo;
    public Sprite weaponIcon;
    public Sprite[] batterySprites;
    public bool isOwned = false;
    public AudioClip shootSound;
}

public class kaiAnimation : MonoBehaviour
{
    private Rigidbody2D rb;
    public Animator anim;
    public Transform spriteTransform;
    
    private SpriteRenderer mySpriteRenderer; 

    [Header("Sonidos")]
    public AudioSource shootAudioSource;
    public AudioSource walkAudioSource;
    public AudioClip walkSound;
    public AudioClip hurtSound;

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

    [Header("Salud e Inmunidad")] // Cabecera actualizada
    public int maxHealth = 5;
    private int currentHealth = -1;
    public float immunityDuration = 2.0f; // Tiempo de inmunidad en segundos
    private bool isInvulnerable = false;  // Bandera interna

    [Header("UI de Corazones")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Caída y Reinicio")]
    public float fallThresholdY = -10f;

    [Header("Reaparición")]
    public Transform respawnPoint;

    [Header("Controles Táctiles Manuales")]
    public Canvas myCanvas;
    public RectTransform moveLeftButtonRect;
    public RectTransform moveRightButtonRect;
    public RectTransform jumpButtonRect;
    public RectTransform fireButtonRect;
    public RectTransform pickupWeaponButtonRect;
    public RectTransform rechargeButtonRect;
    public RectTransform vendingButtonRect;

    private Camera canvasCamera;
    private bool isMoving = false;
    
    private WeaponPickup currentPickupScript;
    private RechargeStation currentRechargeScript;
    private VendingMachine currentVendingScript;

    [Header("Sistema de Armas")]
    public Transform firePoint;
    public Image weaponIconUI;
    public Image batteryMeterUI;

    public Weapon[] allWeapons;
    private int currentWeaponIndex = -1;
    private int[] currentAmmoCounts;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 0) shootAudioSource = sources[0];
        if (sources.Length > 1) walkAudioSource = sources[1];

        if (shootAudioSource == null) shootAudioSource = gameObject.AddComponent<AudioSource>();
        if (walkAudioSource == null) walkAudioSource = gameObject.AddComponent<AudioSource>();

        if (walkSound != null)
        {
            walkAudioSource.clip = walkSound;
            walkAudioSource.loop = true;
            walkAudioSource.playOnAwake = false;
        }
        
        shootAudioSource.playOnAwake = false;

        if (spriteTransform == null) { Debug.LogError("Sprite Transform no asignado en " + this.name); this.enabled = false; }
        else 
        {
            mySpriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            if(mySpriteRenderer == null) mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (anim == null) { Debug.LogError("Animator no encontrado en los hijos de " + this.name); this.enabled = false; }
        if (groundCheck == null) { Debug.LogError("GroundCheck no asignado en " + this.name); this.enabled = false; }
        if (rb == null) { Debug.LogError("Rigidbody2D no encontrado en " + this.name); this.enabled = false; }

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (currentHealth == -1)
        {
            currentHealth = maxHealth;
        }
        UpdateHealthUI();

        if (myCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = null;
        }
        else
        {
            canvasCamera = myCanvas.worldCamera;
        }

        currentAmmoCounts = new int[allWeapons.Length];
        weaponIconUI.enabled = false;
        batteryMeterUI.enabled = false;
        currentWeaponIndex = -1;

        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (allWeapons[i].isOwned)
            {
                currentAmmoCounts[i] = allWeapons[i].maxAmmo;
                if (currentWeaponIndex == -1)
                {
                    currentWeaponIndex = i;
                    UpdateWeaponUI();
                }
            }
        }
    }

    void Update()
    {
        HandleTouchInput();
        HandleWalkSound();

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

    void HandleWalkSound()
    {
        if (walkAudioSource == null) return;
        bool isWalkingOnGround = (Mathf.Abs(moveInput) > 0.1f && isGrounded);

        if (isWalkingOnGround)
        {
            if (!walkAudioSource.isPlaying) walkAudioSource.Play();
        }
        else
        {
            if (walkAudioSource.isPlaying) walkAudioSource.Stop();
        }
    }

    void HandleTouchInput()
    {
        bool pressingJump = false;
        bool pressingFire = false;
        bool pressingLeft = false;
        bool pressingRight = false;
        bool pressingPickup = false;
        bool pressingRecharge = false;
        bool pressingVending = false;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Vector2 touchPos = Input.GetTouch(i).position;
                UnityEngine.TouchPhase phase = UnityEngine.Input.GetTouch(i).phase;

                if (CheckTouchOnRect(touchPos, moveLeftButtonRect)) pressingLeft = true;
                if (CheckTouchOnRect(touchPos, moveRightButtonRect)) pressingRight = true;
                if (CheckTouchOnRect(touchPos, jumpButtonRect) && phase == UnityEngine.TouchPhase.Began) pressingJump = true;
                if (CheckTouchOnRect(touchPos, fireButtonRect) && phase == UnityEngine.TouchPhase.Began) pressingFire = true;
                
                if (CheckTouchOnRect(touchPos, pickupWeaponButtonRect) && phase == UnityEngine.TouchPhase.Began) pressingPickup = true;
                if (CheckTouchOnRect(touchPos, rechargeButtonRect) && phase == UnityEngine.TouchPhase.Began) pressingRecharge = true;
                if (CheckTouchOnRect(touchPos, vendingButtonRect) && phase == UnityEngine.TouchPhase.Began) pressingVending = true;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (CheckTouchOnRect(mousePos, moveLeftButtonRect)) pressingLeft = true;
            if (CheckTouchOnRect(mousePos, moveRightButtonRect)) pressingRight = true;
            if (CheckTouchOnRect(mousePos, jumpButtonRect) && Input.GetMouseButtonDown(0)) pressingJump = true;
            if (CheckTouchOnRect(mousePos, fireButtonRect) && Input.GetMouseButtonDown(0)) pressingFire = true;

            if (CheckTouchOnRect(mousePos, pickupWeaponButtonRect) && Input.GetMouseButtonDown(0)) pressingPickup = true;
            if (CheckTouchOnRect(mousePos, rechargeButtonRect) && Input.GetMouseButtonDown(0)) pressingRecharge = true;
            if (CheckTouchOnRect(mousePos, vendingButtonRect) && Input.GetMouseButtonDown(0)) pressingVending = true;
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

        if (pressingJump) OnTouchJump();
        if (pressingFire) OnTouchFire();
        
        if (pressingPickup && currentPickupScript != null) currentPickupScript.DoPickup();
        if (pressingRecharge && currentRechargeScript != null) currentRechargeScript.DoRecharge();
        if (pressingVending && currentVendingScript != null) currentVendingScript.DoInteract();
    }

    bool CheckTouchOnRect(Vector2 touchPosition, RectTransform rect)
    {
        if (rect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition, canvasCamera);
    }

    void LateUpdate()
    {
        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
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

    // -------------------------------------------------------------------------
    // SECCIÓN DE GESTIÓN DE DAÑO (MODIFICADA)
    // -------------------------------------------------------------------------

    void HandleFall()
    {
        // Caer al vacío cuenta como trampa: quita vida Y reinicia posición
        TakeDamageFromTrap();
    }

    /// <summary>
    /// Llama a esto desde el script de tu Zombie.
    /// Quita vida pero NO te mueve, y te da inmunidad temporal.
    /// </summary>
    public void TakeDamageFromEnemy()
    {
        if (isInvulnerable) return; // Si ya tienes inmunidad, ignorar daño.

        ProcessDamage(false); // false = NO RESPAWNEAR
        
        if (currentHealth > 0) // Si sigues vivo, activar inmunidad
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    /// <summary>
    /// Llama a esto desde la Barrera Láser o Trampas.
    /// Quita vida Y te regresa al punto de resurrección.
    /// </summary>
    public void TakeDamageFromTrap()
    {
        // Las trampas y caídas suelen ignorar la inmunidad y forzar respawn
        ProcessDamage(true); // true = SÍ RESPAWNEAR
    }

    // (Mantenemos este método público por compatibilidad, actuará como daño de enemigo por defecto)
    public void LoseLife()
    {
        TakeDamageFromEnemy();
    }

    // Lógica interna unificada para quitar vida
    private void ProcessDamage(bool forceRespawn)
    {
        currentHealth--;
        UpdateHealthUI();

        if (hurtSound != null && shootAudioSource != null)
        {
            shootAudioSource.PlayOneShot(hurtSound); 
        }
        else 
        {
            Debug.LogWarning("Falta asignar el Audio de Daño (HurtSound)");
        }

        if (currentHealth <= 0)
        {
            Debug.Log("GAME OVER");
            currentHealth = maxHealth;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("Vida perdida. Vidas restantes: " + currentHealth);
            
            // Aquí está la diferencia: Solo respawneamos si es trampa/caída (forceRespawn)
            if (forceRespawn)
            {
                RespawnPlayer();
            }
            else
            {
                // Si es daño normal (zombie), nos quedamos donde estamos.
                // Opcional: Podrías añadir un pequeño empujón (knockback) aquí si quisieras.
            }
        }
    }

    // Corrutina para la inmunidad y el parpadeo
    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        
        float timer = 0;
        float blinkInterval = 0.15f; // Qué tan rápido parpadea

        while (timer < immunityDuration)
        {
            // Alternar visibilidad o color
            if(mySpriteRenderer != null)
            {
                Color c = mySpriteRenderer.color;
                c.a = (c.a == 1f) ? 0.5f : 1f; // Alternar entre opaco y semi-transparente
                mySpriteRenderer.color = c;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Restaurar estado normal
        if(mySpriteRenderer != null)
        {
            Color c = mySpriteRenderer.color;
            c.a = 1f;
            mySpriteRenderer.color = c;
        }
        
        isInvulnerable = false;
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
        }
    }

    void UpdateHealthUI()
    {
        if (hearts == null || fullHeart == null || emptyHeart == null) return;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].sprite = (i < currentHealth) ? fullHeart : emptyHeart;
            hearts[i].enabled = (i < maxHealth);
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
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void AcquireWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length) return;
        allWeapons[weaponIndex].isOwned = true;
        currentAmmoCounts[weaponIndex] = allWeapons[weaponIndex].maxAmmo;
        if (currentWeaponIndex == -1) currentWeaponIndex = weaponIndex;
        SwitchWeaponToIndex(weaponIndex);
    }

    private void DoFire()
    {
        if (currentWeaponIndex == -1) return;
        Weapon currentWeapon = allWeapons[currentWeaponIndex];
        if (!currentWeapon.isOwned) return;

        if (currentAmmoCounts[currentWeaponIndex] > 0)
        {
            currentAmmoCounts[currentWeaponIndex]--;
            UpdateWeaponUI();
            
            anim.SetInteger("WeaponIndex", currentWeaponIndex);
            anim.SetTrigger("fire");
            
            if (currentWeapon.shootSound != null && shootAudioSource != null)
            {
                shootAudioSource.PlayOneShot(currentWeapon.shootSound, 1.0f); 
            }
            
            if (currentWeapon.projectilePrefab == null) return;
            if (firePoint == null) return;

            GameObject projectileGO = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);
            ElectroshockProjectile projectile = projectileGO.GetComponent<ElectroshockProjectile>();
            if (projectile != null)
            {
                projectile.SetDirection(isFacingRight ? Vector2.right : Vector2.left);
            }
        }
        else
        {
            Debug.LogWarning("¡Batería vacía para " + currentWeapon.weaponName + "!");
        }
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed) DoFire();
    }

    public void RechargeCurrentWeapon()
    {
        if (currentWeaponIndex == -1) return;
        int weaponIndex = currentWeaponIndex;
        if (currentAmmoCounts[weaponIndex] == allWeapons[weaponIndex].maxAmmo) return;
        currentAmmoCounts[weaponIndex] = allWeapons[weaponIndex].maxAmmo;
        UpdateWeaponUI();
    }

    private void SwitchWeapon()
    {
        if (allWeapons.Length <= 1) return;
        int nextIndex = currentWeaponIndex;
        for (int i = 0; i < allWeapons.Length; i++)
        {
            nextIndex = (nextIndex + 1) % allWeapons.Length;
            if (allWeapons[nextIndex].isOwned)
            {
                SwitchWeaponToIndex(nextIndex);
                return;
            }
        }
    }

    private void SwitchWeaponToIndex(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length || !allWeapons[weaponIndex].isOwned) return;
        currentWeaponIndex = weaponIndex;
        anim.SetInteger("WeaponIndex", currentWeaponIndex);
        UpdateWeaponUI();
    }

    private void UpdateWeaponUI()
    {
        if (currentWeaponIndex == -1)
        {
            weaponIconUI.enabled = false;
            batteryMeterUI.enabled = false;
            return;
        }

        Weapon currentWeapon = allWeapons[currentWeaponIndex];
        if (weaponIconUI == null || batteryMeterUI == null || currentWeapon.weaponIcon == null || currentWeapon.batterySprites == null || currentWeapon.batterySprites.Length < 4)
        {
            return;
        }

        weaponIconUI.enabled = true;
        weaponIconUI.sprite = currentWeapon.weaponIcon;

        batteryMeterUI.enabled = true;
        int currentAmmo = currentAmmoCounts[currentWeaponIndex];
        int maxAmmo = currentWeapon.maxAmmo;
        float ammoPercentage = (maxAmmo > 0) ? (float)currentAmmo / maxAmmo : 0;

        if (currentAmmo == 0) batteryMeterUI.sprite = currentWeapon.batterySprites[3];
        else if (ammoPercentage <= 0.33f) batteryMeterUI.sprite = currentWeapon.batterySprites[2];
        else if (ammoPercentage <= 0.75f) batteryMeterUI.sprite = currentWeapon.batterySprites[1];
        else batteryMeterUI.sprite = currentWeapon.batterySprites[0];
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
        DoFire();
    }

    public void OnSwitchWeapon(InputValue value)
    {
        if (value.isPressed) SwitchWeapon();
    }

    public void OnTouchSwitchWeapon()
    {
        SwitchWeapon();
    }
    
    public void GainLife()
    {
        if (currentHealth >= maxHealth) return;
        currentHealth++;
        UpdateHealthUI();
    }

    public void SetCurrentInteractable(WeaponPickup item) { currentPickupScript = item; }
    public void SetCurrentInteractable(RechargeStation item) { currentRechargeScript = item; }
    public void SetCurrentInteractable(VendingMachine item) { currentVendingScript = item; }

    public void ClearCurrentInteractable(WeaponPickup item) { if(currentPickupScript == item) currentPickupScript = null; }
    public void ClearCurrentInteractable(RechargeStation item) { if(currentRechargeScript == item) currentRechargeScript = null; }
    public void ClearCurrentInteractable(VendingMachine item) { if(currentVendingScript == item) currentVendingScript = null; }
}