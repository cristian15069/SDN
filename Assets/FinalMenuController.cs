using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalMenuController : MonoBehaviour
{
    [Header("Nombres de Escenas")]
    public string gameSceneName = "SampleScene"; // Tu nivel actual
    // public string nextLevelSceneName = "Nivel2"; // El siguiente nivel (si existe)
    public string mainMenuSceneName = "MenuInicial"; // Tu menú de inicio

    public void OnPlayAgain()
    {
        // Vuelve a cargar el nivel actual
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnNextLevel()
    {
        // Carga el siguiente nivel
        // SceneManager.LoadScene(nextLevelSceneName);
    }

    public void OnExitToMenu()
    {
        // Vuelve al menú principal o cierra el juego
        // SceneManager.LoadScene(mainMenuSceneName); 
        
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}