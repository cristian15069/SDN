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
}

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

    [Header("Controles Táctiles Manuales")]
    public Canvas myCanvas;
    public RectTransform moveLeftButtonRect;
    public RectTransform moveRightButtonRect;
    public RectTransform jumpButtonRect;
    public RectTransform fireButtonRect;

    private Camera canvasCamera;
    private bool isMoving = false;

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

    public void AcquireWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length)
        {
            Debug.LogError("Índice de arma inválido: " + weaponIndex);
            return;
        }

        allWeapons[weaponIndex].isOwned = true;
        currentAmmoCounts[weaponIndex] = allWeapons[weaponIndex].maxAmmo;

        if (currentWeaponIndex == -1)
        {
            currentWeaponIndex = weaponIndex;
        }

        SwitchWeaponToIndex(weaponIndex);
        Debug.Log("¡Arma adquirida: " + allWeapons[weaponIndex].weaponName + "!");
    }

    private void DoFire()
    {
        if (currentWeaponIndex == -1)
        {
            return;
        }

        Weapon currentWeapon = allWeapons[currentWeaponIndex];

        if (!currentWeapon.isOwned)
        {
            return;
        }

        if (currentAmmoCounts[currentWeaponIndex] > 0)
        {
            currentAmmoCounts[currentWeaponIndex]--;
            UpdateWeaponUI();
            
            anim.SetInteger("WeaponIndex", currentWeaponIndex);
            anim.SetTrigger("fire");
            
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
                Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
                projectile.SetDirection(direction);
            }
        }
        else
        {
            Debug.LogWarning("¡Batería vacía para " + currentWeapon.weaponName + "!");
        }
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            DoFire();
        }
    }

    public void RechargeCurrentWeapon()
    {
        if (currentWeaponIndex == -1)
        {
            Debug.Log("No hay arma equipada para recargar.");
            return;
        }

        int weaponIndex = currentWeaponIndex;
        if (currentAmmoCounts[weaponIndex] == allWeapons[weaponIndex].maxAmmo)
        {
            Debug.Log("La batería de " + allWeapons[weaponIndex].weaponName + " ya está llena.");
            return;
        }

        Debug.Log("¡Recargando " + allWeapons[weaponIndex].weaponName + "!");
        currentAmmoCounts[weaponIndex] = allWeapons[weaponIndex].maxAmmo;

        UpdateWeaponUI();
    }

    private void SwitchWeapon()
    {
        if (allWeapons.Length <= 1) return;

        Debug.Log("--- 1. SWITCHWEAPON LLAMADO. Arma actual: " + currentWeaponIndex);

        int nextIndex = currentWeaponIndex;
        for (int i = 0; i < allWeapons.Length; i++)
        {
            nextIndex = (nextIndex + 1) % allWeapons.Length;
            
            Debug.Log("--- 2. Probando índice: " + nextIndex);

            if (allWeapons[nextIndex].isOwned)
            {
                Debug.Log("--- 3. ¡Arma encontrada! Cambiando a " + allWeapons[nextIndex].weaponName);
                currentWeaponIndex = nextIndex;
                UpdateWeaponUI();
                return;
            }
            else
            {
                Debug.Log("--- 3b. Arma en índice " + nextIndex + " no es poseída ('isOwned' = false).");
            }
        }
        
        Debug.LogWarning("--- 4. No se encontró otra arma poseída. Quedándose en el arma actual.");
    }

    private void SwitchWeaponToIndex(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length || !allWeapons[weaponIndex].isOwned)
        {
            return;
        }

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
        
        if (weaponIconUI == null || batteryMeterUI == null)
        {
            Debug.LogError("¡ERROR DE UI! Las casillas 'Weapon Icon UI' o 'Battery Meter UI' no están asignadas en el Inspector del Jugador.");
            return;
        }

        // --- ¡DEBUG CHISMOSO! ---
        if (currentWeapon.weaponIcon != null)
        {
            // Esto nos dirá el nombre del archivo del sprite que está cargando
            Debug.Log("--- 4. UPDATE UI: Poniendo el icono: " + currentWeapon.weaponIcon.name + " (del arma " + currentWeapon.weaponName + ")");
            weaponIconUI.enabled = true;
            weaponIconUI.sprite = currentWeapon.weaponIcon;
        }
        else
        {
            Debug.LogError("¡ERROR DE ARMA! El 'Weapon Icon' para '" + currentWeapon.weaponName + "' (Element " + currentWeaponIndex + ") está vacío (null).");
            weaponIconUI.enabled = false;
        }
        // --- FIN DEL DEBUG ---

        if (currentWeapon.batterySprites == null || currentWeapon.batterySprites.Length < 4)
        {
            Debug.LogError("¡ERROR DE ARMA! Los 'Battery Sprites' para '" + currentWeapon.weaponName + "' (Element " + currentWeaponIndex + ") no están asignados o son menos de 4.");
            batteryMeterUI.enabled = false;
            return;
        }
        
        batteryMeterUI.enabled = true;
        int currentAmmo = currentAmmoCounts[currentWeaponIndex];
        int maxAmmo = currentWeapon.maxAmmo;
        
        float ammoPercentage = (maxAmmo > 0) ? (float)currentAmmo / maxAmmo : 0;

        if (currentAmmo == 0)
        {
            batteryMeterUI.sprite = currentWeapon.batterySprites[3];
        }
        else if (ammoPercentage <= 0.33f)
        {
            batteryMeterUI.sprite = currentWeapon.batterySprites[2];
        }
        else if (ammoPercentage <= 0.75f)
        {
            batteryMeterUI.sprite = currentWeapon.batterySprites[1];
        }
        else
        {
            batteryMeterUI.sprite = currentWeapon.batterySprites[0];
        }
    }

    void UpdateBatteryUI()
    {
        UpdateWeaponUI();
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
        if (value.isPressed)
        {
            SwitchWeapon();
        }
    }

    public void OnTouchSwitchWeapon()
    {
        SwitchWeapon();
    }

    public void GainLife()
    {
        // Primero, comprobamos si la salud ya está al máximo.
        if (currentHealth >= maxHealth)
        {
            Debug.Log("Salud ya está al máximo. No se puede curar.");
            return; // Salimos de la función si ya está lleno
        }

        // Si no está lleno, añadimos una vida
        currentHealth++;
        Debug.Log("¡Vida ganada! Vidas actuales: " + currentHealth);

        // ¡Importante! Actualizamos la UI para que muestre el nuevo corazón
        UpdateHealthUI();
    }
}