using UnityEngine;
using UnityEngine.UI; // Necesario para el botón de móvil

public class VendingMachine : MonoBehaviour
{
    [Header("Estado")]
    private bool isUsed = false; // Para que solo se pueda usar una vez
    private bool isPlayerNear = false;
    private kaiAnimation playerScript; 

    [Header("Sprites (Opcional)")]
    public Sprite spriteOn;  // Sprite de la máquina "llena"
    public Sprite spriteOff; // Sprite de la máquina "vacía"
    private SpriteRenderer mySpriteRenderer;

    [Header("UI de Interacción")]
    public GameObject interactIndicator; // El "Press X" que flota
    public Button mobileInteractButton;  // El botón de la UI para móvil

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        if(mySpriteRenderer != null)
            mySpriteRenderer.sprite = spriteOn; // Empezar con el sprite "llena"
        
        if (interactIndicator != null) interactIndicator.SetActive(false);
        if (mobileInteractButton != null) mobileInteractButton.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo mostrar si el jugador está cerca Y la máquina no se ha usado
        if (other.CompareTag("Player") && !isUsed)
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();
            
            if (interactIndicator != null) interactIndicator.SetActive(true);
            if (mobileInteractButton != null) mobileInteractButton.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerScript = null;
            
            if (interactIndicator != null) interactIndicator.SetActive(false);
            if (mobileInteractButton != null) mobileInteractButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Revisar si el jugador presiona la tecla de interacción (ej. 'X')
        // (¡Asegúrate de que esta tecla no sea la misma que "disparar"!)
        if (isPlayerNear && !isUsed && Input.GetKeyDown(KeyCode.C)) 
        {
            DoInteract();
        }
    }

    // Esta función la llamará el botón de móvil
    public void DoInteract()
    {
        if (isPlayerNear && !isUsed && playerScript != null)
        {
            // ¡Llamamos a la nueva función del jugador!
            playerScript.GainLife(); 

            // Marcar como usada
            isUsed = true;
            
            // Cambiar al sprite "apagado"
            if(mySpriteRenderer != null)
                mySpriteRenderer.sprite = spriteOff; 
            
            // Ocultar los indicadores
            if (interactIndicator != null) interactIndicator.SetActive(false);
            if (mobileInteractButton != null) mobileInteractButton.gameObject.SetActive(false);
        }
    }
}