using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    [Header("Estado de la Estación")]
    private bool isCharged = true; // Empieza cargada por defecto
    private bool isPlayerNear = false;
    private kaiAnimation playerScript; 

    [Header("Sprites de la Estación")]
    public Sprite spriteOn;  // Arrastra aquí tu sprite de "encendido"
    public Sprite spriteOff; // Arrastra aquí tu sprite de "apagado"
    private SpriteRenderer mySpriteRenderer; // Referencia a su propio sprite

    [Header("UI de Interacción")]
    public GameObject rechargeIndicator; // Arrastra aquí el objeto "IndicadorRecarga"

    void Start()
    {
        // Obtener el componente SpriteRenderer de ESTE objeto
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (mySpriteRenderer == null) {
            Debug.LogError("RechargeStation no tiene SpriteRenderer!");
        }

        // 1. Asegurarse de que empieza "encendido"
        isCharged = true;
        mySpriteRenderer.sprite = spriteOn;

        // 2. Asegurarse de que el indicador "Recarga Aquí" empieza oculto
        if (rechargeIndicator != null)
        {
            rechargeIndicator.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<kaiAnimation>();

            // ¡NUEVO! Solo mostrar el indicador si la estación AÚN tiene carga
            if (isCharged && rechargeIndicator != null)
            {
                rechargeIndicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerScript = null;

            // ¡NUEVO! Siempre ocultar el indicador cuando el jugador se va
            if (rechargeIndicator != null)
            {
                rechargeIndicator.SetActive(false);
            }
        }
    }

    void Update()
    {
        // ¡CONDICIÓN MODIFICADA!
        // ¿Está el jugador cerca? ¿Está la estación CARGADA? ¿Presionó [E]?
        if (isPlayerNear && isCharged && Input.GetKeyDown(KeyCode.E))
        {
            if (playerScript != null)
            {
                // 1. Llama al jugador para recargar
                playerScript.RechargeBattery();

                // 2. Gasta la estación (ya no se puede volver a usar)
                isCharged = false;

                // 3. Cambia el sprite de la estación a "apagado"
                mySpriteRenderer.sprite = spriteOff;

                // 4. Oculta el indicador "Recarga Aquí" permanentemente
                if (rechargeIndicator != null)
                {
                    rechargeIndicator.SetActive(false);
                }
            }
        }
    }
}