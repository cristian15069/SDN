using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class VendingMachine : MonoBehaviour
{
    [Header("Estado")]
    private bool isUsed = false;
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
        if (other.CompareTag("Player") && !isUsed)
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();
            
            if(playerScript != null)
            {
                // 1. Registramos este script en el jugador
                playerScript.SetCurrentInteractable(this);

                // 2. DECISIÓN VISUAL:
                if (Application.isMobilePlatform)
                {
                    // En Móvil: Muestra botón táctil
                    if (playerScript.vendingButtonRect != null)
                        playerScript.vendingButtonRect.gameObject.SetActive(true);
                }
                else 
                {
                    // En PC o Editor: Muestra cartel "Presiona X"
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
                // Avisamos al jugador que nos fuimos
                playerScript.ClearCurrentInteractable(this);

                // Ocultamos el botón móvil
                if (playerScript.vendingButtonRect != null)
                    playerScript.vendingButtonRect.gameObject.SetActive(false);
            }
            playerScript = null;

            if (interactIndicator != null) 
                interactIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && !isUsed && !Application.isMobilePlatform && Input.GetKeyDown(KeyCode.C))
        {
            DoInteract();
        }
    }

    public void DoInteract()
    {
        if (!isPlayerNear || isUsed || playerScript == null) return;

        playerScript.GainLife(); 
        isUsed = true;

        if (audioSource != null && interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }
        
        if(mySpriteRenderer != null) 
            mySpriteRenderer.sprite = spriteOff; 
        
        if (playerScript.vendingButtonRect != null)
            playerScript.vendingButtonRect.gameObject.SetActive(false);
            
        if (interactIndicator != null) 
            interactIndicator.SetActive(false);
    }
}