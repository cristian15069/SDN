using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; 

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

    [Header("Salud e Inmunidad")]
    public int maxHealth = 5;
    private int currentHealth; 
    public float immunityDuration = 2.0f;
    private bool isInvulnerable = false;

    [Header("UI de Corazones")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Habilidad Especial")]
    public int killsToReady = 10; 
    public float abilityDuration = 5f;     
    public Color abilityColor = Color.cyan; 
    public Text killCounterUI;              
    public GameObject mobileSkillButtonObj; 
    
    private int currentKills = 0;
    private bool isAbilityReady = false;    
    private bool isAbilityActive = false;   

    [Header("Caida y Reinicio")]
    public float fallThresholdY = -10f;

    [Header("Reaparicion")]
    public Transform respawnPoint;

    [Header("Controles Tactiles")]
    public Canvas myCanvas;
    public RectTransform moveLeftButtonRect;
    public RectTransform moveRightButtonRect;
    public RectTransform jumpButtonRect;
    public RectTransform fireButtonRect;
    public RectTransform pickupWeaponButtonRect;
    public RectTransform rechargeButtonRect;
    public RectTransform vendingButtonRect;
    public RectTransform skillButtonRect; 

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

        currentHealth = maxHealth;
        UpdateHealthUI();

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

        if (spriteTransform == null) { this.enabled = false; }
        else 
        {
            mySpriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            if(mySpriteRenderer == null) mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (anim == null) { this.enabled = false; }
        if (groundCheck == null) { this.enabled = false; }
        if (rb == null) { this.enabled = false; }

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (myCanvas.renderMode == RenderMode.ScreenSpaceOverlay) canvasCamera = null;
        else canvasCamera = myCanvas.worldCamera;

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
        
        if (mobileSkillButtonObj != null) mobileSkillButtonObj.SetActive(false);
        UpdateKillUI();
    }

    void Update()
    {
        HandleTouchInput();
        HandleWalkSound();

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryActivateAbility();
        }

        if (groundCheck == null) return;

        Vector2 groundCheckPos = groundCheck.position + Vector3.down * groundCheckOffset;
        isGrounded = Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, groundLayer);

        if (isGrounded) isFallingToDeath = false;

        anim.SetFloat("speed", Mathf.Abs(moveInput));
        anim.SetBool("isGrounded", isGrounded);

        if (transform.position.y < fallThresholdY && !isFallingToDeath)
        {
            isFallingToDeath = true;
            HandleFall();
        }
    }

    public void AddKill()
    {
        if (isAbilityActive || isAbilityReady) return;

        currentKills++;
        
        if (currentKills >= killsToReady)
        {
            currentKills = killsToReady;
            isAbilityReady = true;
            
            if (mobileSkillButtonObj != null) mobileSkillButtonObj.SetActive(true);
        }
        
        UpdateKillUI();
    }

    public void TryActivateAbility()
    {
        if (isAbilityReady && !isAbilityActive)
        {
            StartCoroutine(AbilityRoutine());
        }
    }

    void UpdateKillUI()
    {
        if (killCounterUI != null)
        {
            if (isAbilityActive) killCounterUI.text = "¡ACTIVO!";
            else if (isAbilityReady) killCounterUI.text = "CARGA ILIMITADA! (PRECIONA R)";
            else killCounterUI.text = currentKills + "/" + killsToReady;
        }
    }

    IEnumerator AbilityRoutine()
    {
        isAbilityActive = true;
        isAbilityReady = false; 
        currentKills = 0;       

        if (mobileSkillButtonObj != null) mobileSkillButtonObj.SetActive(false);
        
        UpdateKillUI();

        float timer = 0f;
        float flashSpeed = 0.1f; 

        while (timer < abilityDuration)
        {
            if (mySpriteRenderer != null)
            {
                mySpriteRenderer.color = (mySpriteRenderer.color == Color.white) ? abilityColor : Color.white;
            }
            yield return new WaitForSeconds(flashSpeed);
            timer += flashSpeed;
        }

        if (mySpriteRenderer != null) mySpriteRenderer.color = Color.white;
        
        isAbilityActive = false;
        UpdateKillUI();
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
        bool pressingSkill = false;

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
                
                if (CheckTouchOnRect(touchPos, skillButtonRect) && phase == UnityEngine.TouchPhase.Began) pressingSkill = true;
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
            
            if (CheckTouchOnRect(mousePos, skillButtonRect) && Input.GetMouseButtonDown(0)) pressingSkill = true;
        }

        if (pressingLeft) { OnPointerDownMove(-1); isMoving = true; }
        else if (pressingRight) { OnPointerDownMove(1); isMoving = true; }
        else if (isMoving) { isMoving = false; OnPointerUpMove(); }

        if (pressingJump) DoJump();
        if (pressingFire) OnTouchFire();
        if (pressingSkill) TryActivateAbility();
        
        if (pressingPickup && currentPickupScript != null) currentPickupScript.DoPickup();
        if (pressingRecharge && currentRechargeScript != null) currentRechargeScript.DoRecharge();
        if (pressingVending && currentVendingScript != null) currentVendingScript.DoInteract();
    }

    bool CheckTouchOnRect(Vector2 touchPosition, RectTransform rect)
    {
        if (rect == null || !rect.gameObject.activeInHierarchy) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition, canvasCamera);
    }

    void LateUpdate()
    {
        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();
    }

    void FixedUpdate()
    {
        #if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if (!isGrounded && rb.linearVelocity.y < 0) rb.linearVelocity += Vector2.down * extraGravity * Time.fixedDeltaTime;
        #else
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        if (!isGrounded && rb.velocity.y < 0) rb.velocity += Vector2.down * extraGravity * Time.fixedDeltaTime;
        #endif
    }

    public void OnMove(InputValue value) { moveInput = value.Get<float>(); }
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            DoJump();
        }
    }

    private void DoJump()
    {
        if (isGrounded)
        {
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            #else
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            #endif
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = spriteTransform.localScale;
        scaler.x *= -1;
        spriteTransform.localScale = scaler;
    }

    void HandleFall() { TakeDamageFromTrap(); }
    public void LoseLife() { TakeDamageFromEnemy(); }

    public void TakeDamageFromEnemy()
    {
        if (isInvulnerable) return; 
        ProcessDamage(false); 
        if (currentHealth > 0) StartCoroutine(InvulnerabilityRoutine());
    }

    public void TakeDamageFromTrap() { ProcessDamage(true); }

    private void ProcessDamage(bool forceRespawn)
    {
        currentHealth--;
        UpdateHealthUI();

        if (hurtSound != null && shootAudioSource != null) shootAudioSource.PlayOneShot(hurtSound); 

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            if (forceRespawn) RespawnPlayer();
        }
    }

    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float timer = 0;
        float blinkInterval = 0.15f; 
        while (timer < immunityDuration)
        {
            if(mySpriteRenderer != null)
            {
                Color c = mySpriteRenderer.color;
                c.a = (c.a == 1f) ? 0.5f : 1f; 
                mySpriteRenderer.color = c;
            }
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
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
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
            #else
            rb.velocity = Vector2.zero;
            #endif
            isFallingToDeath = false;
        }
    }

    public void SetRespawnPoint(Transform newPoint) { if (respawnPoint != newPoint) respawnPoint = newPoint; }

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

        if (currentAmmoCounts[currentWeaponIndex] > 0 || isAbilityActive)
        {
            if (!isAbilityActive)
            {
                currentAmmoCounts[currentWeaponIndex]--;
                UpdateWeaponUI();
            }
            
            anim.SetInteger("WeaponIndex", currentWeaponIndex);
            anim.SetTrigger("fire");
            
            if (currentWeapon.shootSound != null && shootAudioSource != null)
                shootAudioSource.PlayOneShot(currentWeapon.shootSound, 1.0f); 
            
            if (currentWeapon.projectilePrefab != null && firePoint != null)
            {
                GameObject projectileGO = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);
                ElectroshockProjectile projectile = projectileGO.GetComponent<ElectroshockProjectile>();
                if (projectile != null) projectile.SetDirection(isFacingRight ? Vector2.right : Vector2.left);
            }
        }
        else
        {
            Debug.LogWarning("Bateria vacia");
        }
    }

    public void OnFire(InputValue value) { if (value.isPressed) DoFire(); }
    public void OnTouchFire() { DoFire(); }

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
            if (allWeapons[nextIndex].isOwned) { SwitchWeaponToIndex(nextIndex); return; }
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
        if (currentWeaponIndex == -1) { weaponIconUI.enabled = false; batteryMeterUI.enabled = false; return; }

        Weapon currentWeapon = allWeapons[currentWeaponIndex];
        if (weaponIconUI == null || batteryMeterUI == null || currentWeapon.weaponIcon == null) return;

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

    public void OnPointerDownMove(float direction) { moveInput = direction; }
    public void OnPointerUpMove() { moveInput = 0f; }
    public void OnSwitchWeapon(InputValue value) { if (value.isPressed) SwitchWeapon(); }
    public void OnTouchSwitchWeapon() { SwitchWeapon(); }
    public void GainLife() { if (currentHealth >= maxHealth) return; currentHealth++; UpdateHealthUI(); }
    public void SetCurrentInteractable(WeaponPickup item) { currentPickupScript = item; }
    public void SetCurrentInteractable(RechargeStation item) { currentRechargeScript = item; }
    public void SetCurrentInteractable(VendingMachine item) { currentVendingScript = item; }
    public void ClearCurrentInteractable(WeaponPickup item) { if(currentPickupScript == item) currentPickupScript = null; }
    public void ClearCurrentInteractable(RechargeStation item) { if(currentRechargeScript == item) currentRechargeScript = null; }
    public void ClearCurrentInteractable(VendingMachine item) { if(currentVendingScript == item) currentVendingScript = null; }
}