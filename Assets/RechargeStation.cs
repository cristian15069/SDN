using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class RechargeStation : MonoBehaviour
{
    [Header("Estado")]
    private bool isCharged = true;
    private bool isPlayerNear = false;
    private kaiAnimation playerScript; 

    [Header("Sprites (Opcional)")]
    public Sprite spriteOn; 
    public Sprite spriteOff;
    private SpriteRenderer mySpriteRenderer;

    [Header("UI de Interacción (PC/Editor)")]
    public GameObject interactIndicator; 

    [Header("Sonido")]
    public AudioClip interactSound; 
    private AudioSource audioSource;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        if(mySpriteRenderer != null) 
            mySpriteRenderer.sprite = spriteOn; 
        
        if (interactIndicator != null) 
            interactIndicator.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isCharged)
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();
            if(playerScript != null)
            {
                playerScript.SetCurrentInteractable(this);
                
                if (Application.isMobilePlatform)
                {
                    if(playerScript.rechargeButtonRect != null)
                        playerScript.rechargeButtonRect.gameObject.SetActive(true);
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
                
                if(playerScript.rechargeButtonRect != null)
                    playerScript.rechargeButtonRect.gameObject.SetActive(false);
            }
            playerScript = null;
            
            if (interactIndicator != null) 
                interactIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && isCharged && !Application.isMobilePlatform && Input.GetKeyDown(KeyCode.E))
        {
            DoRecharge();
        }
    }

    public void DoRecharge()
    {
        if (!isPlayerNear || !isCharged || playerScript == null) return;

        playerScript.RechargeCurrentWeapon(); 
        isCharged = false;

        if (audioSource != null && interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }
        
        if(mySpriteRenderer != null) mySpriteRenderer.sprite = spriteOff; 
        
        if(playerScript.rechargeButtonRect != null)
            playerScript.rechargeButtonRect.gameObject.SetActive(false);
          
        if (interactIndicator != null) 
            interactIndicator.SetActive(false);
    }
}