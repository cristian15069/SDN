using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    [Header("Estado de la Estación")]
    private bool isCharged = true;
    private bool isPlayerNear = false;
    private kaiAnimation playerScript; 

    [Header("Sprites de la Estación")]
    public Sprite spriteOn; 
    public Sprite spriteOff;
    private SpriteRenderer mySpriteRenderer;

    [Header("UI de Interacción")]
    public GameObject rechargeIndicator; 

    [Header("Botón Móvil")]
    public GameObject mobileRechargeButton;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (mySpriteRenderer == null) {
            Debug.LogError("RechargeStation no tiene SpriteRenderer!");
        }

        isCharged = true;
        mySpriteRenderer.sprite = spriteOn;

        if (rechargeIndicator != null)
        {
            rechargeIndicator.SetActive(false);
        }

        if(mobileRechargeButton != null)
            mobileRechargeButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();

            if (isCharged)
            {
                if (rechargeIndicator != null)
                    rechargeIndicator.SetActive(true);
                
                if (mobileRechargeButton != null)
                    mobileRechargeButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerScript = null;

            if (rechargeIndicator != null)
                rechargeIndicator.SetActive(false);
            
            if (mobileRechargeButton != null)
                mobileRechargeButton.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && isCharged && Input.GetKeyDown(KeyCode.E))
        {
            DoRecharge();
        }
    }

    public void DoRecharge()
    {
        if (isPlayerNear && isCharged && playerScript != null)
        {
            playerScript.RechargeCurrentWeapon(); 
            
            isCharged = false;
            mySpriteRenderer.sprite = spriteOff;

            if (rechargeIndicator != null)
                rechargeIndicator.SetActive(false);
            
            if (mobileRechargeButton != null)
                mobileRechargeButton.SetActive(false);
        }
    }
}
