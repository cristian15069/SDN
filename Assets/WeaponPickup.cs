using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Configuración del Arma")]
    [Tooltip("El índice del arma en la lista 'All Weapons' del jugador (0, 1, 2, etc.)")]
    public int weaponIndex = 0; 
    public bool isSingleUse = true;
    private bool hasBeenUsed = false;

    [Header("UI de Interacción")]
    public GameObject pickupIndicator; 
    public GameObject mobilePickupButton; 

    private bool isPlayerNear = false;
    private kaiAnimation playerScript; 

    void Start()
    {
        if (pickupIndicator != null)
            pickupIndicator.SetActive(false);
        
        if (mobilePickupButton != null)
            mobilePickupButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();

            if (pickupIndicator != null)
            {
                pickupIndicator.SetActive(true);
            }
            
            if (mobilePickupButton != null)
            {
                mobilePickupButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerScript = null;

            if (pickupIndicator != null)
                pickupIndicator.SetActive(false);
            
            if (mobilePickupButton != null)
                mobilePickupButton.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && !hasBeenUsed && Input.GetKeyDown(KeyCode.X))
        {
            DoPickup();
        }
    }

    public void DoPickup()
    {
        if (isPlayerNear && !hasBeenUsed && playerScript != null)
        {
            playerScript.AcquireWeapon(weaponIndex);
            hasBeenUsed = true;

            if (pickupIndicator != null)
                pickupIndicator.SetActive(false);
            
            if (mobilePickupButton != null)
                mobilePickupButton.SetActive(false);

            if (isSingleUse)
            {
                Destroy(gameObject); 
            }
        }
    }
}

