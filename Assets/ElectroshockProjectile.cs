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
            sr.color = Color.white; 
            sr.sortingOrder = 20; 
        }
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
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
        
        Destroy(gameObject);
    }
}