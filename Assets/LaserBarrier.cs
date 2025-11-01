using UnityEngine;

public class LaserBarrier : MonoBehaviour
{
    [Header("Sprites de la Barrera")]
    public Sprite spriteOn; 
    public Sprite spriteOff;
    private SpriteRenderer spriteRenderer;

    [Header("Configuración del Láser")]
    public float activeTime = 2f;    
    public float inactiveTime = 1.5f; 
    public int damage = 1;           
    public float damageInterval = 0.5f; 

    private BoxCollider2D laserCollider;
    private bool isOn = false; 
    private float timer;
    private float damageTimer; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
     
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        if (colliders.Length > 1) {
            foreach(BoxCollider2D col in colliders) {
                if (col.isTrigger) {
                    laserCollider = col;
                    break;
                }
            }
        }

        if (laserCollider == null) {
            Debug.LogError("LaserBarrier no tiene un BoxCollider2D marcado como Is Trigger para el láser!");
            this.enabled = false;
            return;
        }

        SetLaserState(false);
        timer = inactiveTime; 
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isOn = !isOn; // Cambia el estado (encendido <-> apagado)
            SetLaserState(isOn); // Actualiza la visual y el collider

            timer = isOn ? activeTime : inactiveTime; // Resetea el temporizador
        }
    }

    void SetLaserState(bool state)
    {
        isOn = state;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOn ? spriteOn : spriteOff; // Cambia el sprite
        }
        if (laserCollider != null)
        {
            laserCollider.enabled = isOn; // Activa o desactiva el collider de daño
        }

        Debug.Log("Láser está " + (isOn ? "ENCENDIDO" : "APAGADO"));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Esta función se llama MIENTRAS el jugador está dentro del Trigger del láser
        if (isOn && other.CompareTag("Player"))
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0)
            {
                // Busca el script de vida del jugador
                kaiAnimation player = other.GetComponent<kaiAnimation>();
                if (player != null)
                {
                    player.LoseLife(); // Le quitamos vida al jugador
                    Debug.Log("Jugador recibió " + damage + " de daño por láser.");
                }
                damageTimer = damageInterval; // Resetea el temporizador de daño
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cuando el jugador sale del láser, reseteamos el temporizador de daño
        if (other.CompareTag("Player"))
        {
            damageTimer = 0;
        }
    }
}