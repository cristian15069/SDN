using UnityEngine;
using UnityEngine.UI;

public class WeaponPickup : MonoBehaviour
{
    [Header("Configuración del Arma")]
    public int weaponIndex = 0; 
    public bool isSingleUse = true;
    private bool hasBeenUsed = false;

    [Header("UI de Interacción (PC/Editor)")]
    public GameObject interactIndicator; 

    private bool isPlayerNear = false;
    private kaiAnimation playerScript; 

    private SpriteRenderer mySpriteRenderer;
    private Collider2D myCollider;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();

        if (interactIndicator != null) 
            interactIndicator.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();
            
            if(playerScript != null)
            {
                // Le decimos al jugador que somos el objeto interactivo
                playerScript.SetCurrentInteractable(this);

                // --- LÓGICA CORREGIDA ---
                // Si es un móvil REAL -> Botón Táctil
                // Si es Editor o PC -> Cartel "Presiona X"
                if (Application.isMobilePlatform)
                {
                    if (playerScript.pickupWeaponButtonRect != null)
                        playerScript.pickupWeaponButtonRect.gameObject.SetActive(true);
                }
                else
                {
                    if (interactIndicator != null) 
                        interactIndicator.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            
            if(playerScript != null)
            {
                playerScript.ClearCurrentInteractable(this);

                if (playerScript.pickupWeaponButtonRect != null)
                    playerScript.pickupWeaponButtonRect.gameObject.SetActive(false);
            }
            playerScript = null;

            if (interactIndicator != null) 
                interactIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // En el Editor, isMobilePlatform es falso, así que esto funcionará
        if (isPlayerNear && !hasBeenUsed && !Application.isMobilePlatform && Input.GetKeyDown(KeyCode.X))
        {
            DoPickup();
        }
    }

    public void DoPickup()
    {
        if (!isPlayerNear || hasBeenUsed || playerScript == null) return;

        playerScript.AcquireWeapon(weaponIndex);
        hasBeenUsed = true;

        // Ocultar botón móvil (si estuviera activo)
        if (playerScript.pickupWeaponButtonRect != null)
            playerScript.pickupWeaponButtonRect.gameObject.SetActive(false);
            
        // Ocultar cartel PC (si estuviera activo)
        if (interactIndicator != null) 
            interactIndicator.SetActive(false);

        if (isSingleUse)
        {
            Destroy(gameObject);
        }
    }
}