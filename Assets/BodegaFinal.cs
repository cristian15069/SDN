using UnityEngine;
using UnityEngine.SceneManagement;

public class BodegaFinal : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public string finalMenuSceneName = "FinalMenuScene"; // Nombre de tu escena de victoria
    public float delayBeforeLoading = 0.5f; // Tiempo que tarda la animación en terminar

    [Header("Referencias al Jugador")]
    public kaiAnimation playerScript; 
    public Rigidbody2D playerRb;
    public Animator playerAnim;
    public GameObject playerUI; // La UI de corazones/armas para ocultarla

    [Header("Animación de la Puerta")]
    public Animator doorAnimator; // El Animator de la bodega/puerta
    public string openTriggerName = "OpenDoor"; // El nombre del Trigger en el Animator de la puerta

    [Header("UI de Interacción")]
    public GameObject pcIndicator; // El texto "Presiona V"
    public GameObject mobileButton; // El botón para abrir en móvil

    private bool isPlayerNear = false;
    private bool levelFinished = false;

    void Start()
    {
        // Asegurarnos de que la UI empiece oculta
        if (pcIndicator != null) pcIndicator.SetActive(false);
        if (mobileButton != null) mobileButton.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !levelFinished)
        {
            isPlayerNear = true;
            
            // Mostrar la UI correspondiente
            if (Application.isMobilePlatform)
            {
                if (mobileButton != null) mobileButton.SetActive(true);
            }
            else
            {
                if (pcIndicator != null) pcIndicator.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            
            // Ocultar toda la UI si se aleja
            if (pcIndicator != null) pcIndicator.SetActive(false);
            if (mobileButton != null) mobileButton.SetActive(false);
        }
    }

    void Update()
    {
        // Lógica para PC (Tecla V)
        // Usamos !Application.isMobilePlatform para que en el Editor funcione la tecla V
        if (isPlayerNear && !levelFinished && !Application.isMobilePlatform && Input.GetKeyDown(KeyCode.V))
        {
            ActivateFinalSequence();
        }
    }

    // Esta función la llamará el botón móvil (o la tecla V)
    public void ActivateFinalSequence()
    {
        if (levelFinished) return; // Evitar activarlo dos veces
        levelFinished = true;

        // 1. Ocultar indicadores de interacción
        if (pcIndicator != null) pcIndicator.SetActive(false);
        if (mobileButton != null) mobileButton.SetActive(false);

        // 2. Detener al jugador
        if (playerScript != null) playerScript.enabled = false;
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.simulated = false; 
        }
        if (playerAnim != null) playerAnim.SetFloat("speed", 0);

        // 3. Ocultar la UI del juego
        if (playerUI != null) playerUI.SetActive(false);

        // 4. Reproducir animación de la puerta
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTriggerName);
        }

        // 5. Cargar el menú final después del retraso
        Invoke("LoadMenu", delayBeforeLoading);
    }

    void LoadMenu()
    {
        SceneManager.LoadScene(finalMenuSceneName);
    }
}