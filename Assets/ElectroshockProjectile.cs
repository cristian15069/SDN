using UnityEngine;

public class ElectroshockProjectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    public float lifetime = 2f;

    private Vector2 moveDirection;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Corrección visual (Color y Capa)
            sr.color = Color.white; 
            sr.sortingOrder = 20; 
        }
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // --- ¡NUEVA LÓGICA DE ROTACIÓN! ---
        // Si la bala va hacia la izquierda (X es negativo)
        if (moveDirection.x < 0)
        {
            // Rotamos el objeto 180 grados en el eje Y (efecto espejo)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // Si va a la derecha, mantenemos la rotación normal (0)
            transform.rotation = Quaternion.identity;
        }
    }

    void Update()
    {
        // Movemos la bala (importante usar Space.World para que la rotación no afecte la dirección)
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            ZombieController enemy = other.GetComponent<ZombieController>();
            if (enemy != null)
            {
               enemy.TakeDamage(damage);
            }
        }
        
        if (other.isTrigger) 
        {
            return; 
        }

        Destroy(gameObject);
    }
}