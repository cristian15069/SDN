using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PausarJuego : MonoBehaviour
{
    [Header("Referencias OBLIGATORIAS")]
    public GameObject menuPausa;
    public RectTransform botonPausaRect; // Arrastra tu botón aquí
    public Canvas myCanvas;              // Arrastra tu Canvas aquí

    [Header("Teclas")]
    public KeyCode teclaPausa = KeyCode.Escape;
    public KeyCode teclaReanudar = KeyCode.R;
    public KeyCode teclaReiniciar = KeyCode.T;
    public KeyCode teclaMenuPrincipal = KeyCode.M;
    public KeyCode teclaSalir = KeyCode.Q;

    public string nombreMenuPrincipal = "MenuInicial";
    
    private bool juegoPausado = false;
    private Camera canvasCamera;

    private void Start()
    {
        // 1. FUERZA BRUTA: El mouse debe verse SIEMPRE al iniciar
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true; 

        if (menuPausa != null)
            menuPausa.SetActive(false);

        // Configuración de cámara para detectar clicks
        if (myCanvas != null)
        {
            if (myCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                canvasCamera = null;
            else
                canvasCamera = myCanvas.worldCamera;
        }
    }

    private void Update()
    {
        // 2. SEGURIDAD: Si por alguna razón el mouse se esconde, lo mostramos de nuevo
        if (Cursor.visible == false) 
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // --- DETECCIÓN MANUAL DEL CLIC ---
        if (Input.GetMouseButtonDown(0))
        {
            if (!juegoPausado)
            {
                if (CheckTouchOnRect(Input.mousePosition, botonPausaRect))
                {
                    Pausar();
                }
            }
        }
        // ---------------------------------

        if (Input.GetKeyDown(teclaPausa)) TogglePausa();

        if (juegoPausado)
        {
            if (Input.GetKeyDown(teclaReanudar)) Reanudar();
            if (Input.GetKeyDown(teclaReiniciar)) ReiniciarNivel();
            if (Input.GetKeyDown(teclaMenuPrincipal)) SalirAlMenuPrincipal();
            if (Input.GetKeyDown(teclaSalir)) SalirDelJuego();
        }
    }

    bool CheckTouchOnRect(Vector2 touchPosition, RectTransform rect)
    {
        if (rect == null || !rect.gameObject.activeInHierarchy) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition, canvasCamera);
    }

    public void TogglePausa()
    {
        if (juegoPausado) Reanudar();
        else Pausar();
    }

    public void Pausar()
    {
        if (menuPausa != null) menuPausa.SetActive(true);

        Time.timeScale = 0f; 
        juegoPausado = true;
        
        // Aseguramos mouse visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        if (menuPausa != null) menuPausa.SetActive(false);

        Time.timeScale = 1f; 
        juegoPausado = false;

        // IMPORTANTE: Ya NO escondemos el mouse aquí. 
        // Se quedará visible para que no tengas problemas.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreMenuPrincipal);
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
} 