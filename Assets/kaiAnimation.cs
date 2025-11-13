using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    
    [Header("Sonidos")]
    public AudioSource shootAudioSource; // Tu AudioSource original para disparos
    public AudioSource walkAudioSource;  // Un NUEVO AudioSource para caminar
    public AudioClip walkSound;          // El archivo .wav o .mp3 de tus pasos

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

        // --- LÓGICA DE AUDIO AJUSTADA ---
        // Buscamos los 2 AudioSource
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 0)
        {
            shootAudioSource = sources[0]; // El primero es para disparos
        }
        if (sources.Length > 1)
        {
            walkAudioSource = sources[1]; // El segundo es para caminar
        }

        // Si te faltan, los creamos por seguridad y te avisamos
        if (shootAudioSource == null) 
        {
            Debug.LogWarning("Faltaba 1er AudioSource (disparos), creando uno.");
            shootAudioSource = gameObject.AddComponent<AudioSource>();
        }
        if (walkAudioSource == null)
        {
            Debug.LogWarning("Faltaba 2do AudioSource (caminar), creando uno.");
            walkAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configura el altavoz de caminar
        if (walkSound != null)
        {
            walkAudioSource.clip = walkSound;
            walkAudioSource.loop = true;
            walkAudioSource.playOnAwake = false;
        }
        else
        {
            Debug.LogWarning("No se ha asignado un 'Walk Sound' en el Inspector.");
        }
        
        shootAudioSource.playOnAwake = false;
        // --- FIN LÓGICA DE AUDIO ---


        if (spriteTransform == null) { Debug.LogError("Sprite Transform no asignado en " + this.name); this.enabled = false; }
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
        HandleWalkSound(); // <-- ¡LÓGICA DE SONIDO AÑADIDA!

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

    // --- ¡NUEVA FUNCIÓN DE SONIDO! ---
    void HandleWalkSound()
    {
        if (walkAudioSource == null) return;

        bool isWalkingOnGround = (Mathf.Abs(moveInput) > 0.1f && isGrounded);

        if (isWalkingOnGround)
        {
            if (!walkAudioSource.isPlaying)
            {
                walkAudioSource.Play();
            }
        }
        else
        {
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }
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
        
        if (pressingPickup)
        {
            if (currentPickupScript != null) currentPickupScript.DoPickup();
        }
        if (pressingRecharge)
        {
            if (currentRechargeScript != null) currentRechargeScript.DoRecharge();
        }
        if (pressingVending)
        {
            if (currentVendingScript != null) currentVendingScript.DoInteract();
        }
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
            
            if (currentWeapon.projectilePrefab == null) 
            {
                Debug.LogError("Prefab de proyectil no asignado para " + currentWeapon.weaponName);
                return;
            }
            if (firePoint == null) 
            {
                Debug.LogError("Fire Point no asignado en el Inspector.", this.gameObject);
                return;
            }

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
            Debug.LogError("Error en la UI de Armas: Faltan referencias para el arma: " + currentWeapon.weaponName);
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

    public void SetCurrentInteractable(WeaponPickup item)
    {
        currentPickupScript = item;
    }
    public void SetCurrentInteractable(RechargeStation item)
    {
        currentRechargeScript = item;
    }
    public void SetCurrentInteractable(VendingMachine item)
    {
        currentVendingScript = item;
    }

    public void ClearCurrentInteractable(WeaponPickup item)
    {
        if(currentPickupScript == item) currentPickupScript = null;
    }
    public void ClearCurrentInteractable(RechargeStation item)
    {
        if(currentRechargeScript == item) currentRechargeScript = null;
    }
    public void ClearCurrentInteractable(VendingMachine item)
    {
        if(currentVendingScript == item) currentVendingScript = null;
    }
}