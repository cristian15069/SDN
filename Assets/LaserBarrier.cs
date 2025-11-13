using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Sonido")]
    public AudioClip laserSound;
    private AudioSource audioSource;

    private BoxCollider2D laserCollider;
    private bool isOn = false; 
    private float timer;
    private float damageTimer; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
     
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
            isOn = !isOn;
            SetLaserState(isOn);
            timer = isOn ? activeTime : inactiveTime; 
        }
    }

    void SetLaserState(bool state)
    {
        isOn = state;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOn ? spriteOn : spriteOff; 
        }
        
        if (laserCollider != null)
        {
            laserCollider.enabled = isOn; 
        }

        if (isOn && audioSource != null && laserSound != null)
        {
            audioSource.PlayOneShot(laserSound);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isOn && other.CompareTag("Player"))
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0)
            {
                kaiAnimation player = other.GetComponent<kaiAnimation>();
                if (player != null)
                {
                    player.LoseLife(); 
                }
                damageTimer = damageInterval; 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            damageTimer = 0;
        }
    }
}