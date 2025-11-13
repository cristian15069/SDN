using UnityEngine;
using System.Collections.Generic; // ¡Importante para usar Listas!

public class ZoneSpawner : MonoBehaviour
{
    [Header("Configuración de Zombis")]
    [Tooltip("La lista de TODOS los prefabs de zombis que PUEDEN aparecer aquí (ej. Zombie_Normal, JumpingZombie)")]
    public GameObject[] zombiePrefabs; // Arrastra tus PREFABS de zombis aquí

    [Header("Configuración de Aparición")]
    [Tooltip("Arrastra aquí TODOS los 'Spawn Points' (hijos) que este generador puede usar")]
    public Transform[] spawnPoints; // Arrastra los PUNTOS de aparición (hijos) aquí
    
    [Tooltip("Cuántos zombis generar cuando se active")]
    public int amountToSpawn = 3;

    private bool hasBeenTriggered = false;

    // --- Paso A: Asegurarse de que el Trigger esté bien ---
    void Start()
    {
        // Forzamos a que el collider de este objeto sea un Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("¡ZoneSpawner '" + name + "' no tiene Collider 2D! No puede detectar al jugador.", this.gameObject);
        }
    }

    // --- Paso B: Detectar al Jugador ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ¿Es el jugador? ¿Y es la primera vez que entra?
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true; // Marcar como usado
            Debug.Log("¡Jugador entró en la zona! Generando " + amountToSpawn + " zombis.");
            SpawnZombies();
        }
    }

    // --- Paso C: La Lógica de Generación ---
    void SpawnZombies()
    {
        // Fallo de seguridad: ¿Olvidaste conectar los prefabs o los puntos?
        if (zombiePrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogError("¡ZoneSpawner '" + name + "' no tiene Prefabs de Zombi o Puntos de Aparición asignados en el Inspector!", this.gameObject);
            return;
        }

        // Creamos una "copia" de la lista de puntos para poder elegir sin repetir
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        // Asegurarse de no intentar generar más zombis que puntos disponibles
        int spawnCount = Mathf.Min(amountToSpawn, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            // 1. Elegir un PREFAB de zombi al azar de la lista
            GameObject prefabToSpawn = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];

            // 2. Elegir un PUNTO de aparición al azar de la lista
            int index = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[index];
            
            // 3. Quitar ese punto de la lista para no volver a usarlo
            availablePoints.RemoveAt(index); 

            // 4. ¡Crear el zombi!
            Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        }
    }
}