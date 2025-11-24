using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PausarJuego : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject menuPausa;
    public Button botonPausa;
    
    [Header("Controles Teclado")]
    public KeyCode teclaReanudar = KeyCode.R;
    public KeyCode teclaSalir = KeyCode.Q;
    public KeyCode teclaReiniciar = KeyCode.T;
    public KeyCode teclaMenuPrincipal = KeyCode.M;
    public KeyCode teclaPausa = KeyCode.Escape;
    
    [Header("Nombres de Escenas")]
    public string nombreMenuPrincipal = "Menu";
    
    [SerializeField] private bool juegoPausado = false;

    private void Start()
    {
        if (botonPausa != null)
            botonPausa.onClick.AddListener(Pausar);

        if (menuPausa != null)
            menuPausa.SetActive(false);
    }

    private void Update()
    {
        ProcesarInputTeclado();
    }

    private void ProcesarInputTeclado()
    {
        if (Input.GetKeyDown(teclaPausa))
        {
            TogglePausa();
        }

        if (!juegoPausado) return;

        if (Input.GetKeyDown(teclaReanudar))
        {
            Reanudar();
        }
        
        if (Input.GetKeyDown(teclaReiniciar))
        {
            ReiniciarNivel();
        }
        
        if (Input.GetKeyDown(teclaMenuPrincipal))
        {
            SalirAlMenuPrincipal();
        }
        
        if (Input.GetKeyDown(teclaSalir))
        {
            SalirDelJuego();
        }
    }

    public void TogglePausa()
    {
        if (juegoPausado)
        {
            Reanudar();
        }
        else
        {
            Pausar();
        }
    }

    public void Reanudar()
    {
        if (menuPausa != null)
            menuPausa.SetActive(false);
            
        Time.timeScale = 1f;
        juegoPausado = false;
        Debug.Log("Juego reanudado!");
    }

    public void Pausar()
    {
        if (menuPausa != null)
            menuPausa.SetActive(true);
            
        Time.timeScale = 0f;
        juegoPausado = true;
        Debug.Log("Juego pausado!");
    }

    public void ReiniciarNivel()
    {
        Debug.Log("Reiniciando nivel...");
        
        Time.timeScale = 1f;
        juegoPausado = false;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenuPrincipal()
    {
        Debug.Log("Saliendo al menú principal...");
        
        Time.timeScale = 1f;
        juegoPausado = false;
        
        if (!string.IsNullOrEmpty("MenuInicial"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        else
        {
            Debug.LogError("Nombre del menú principal no asignado!");
        }
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        
        Time.timeScale = 1f;
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}