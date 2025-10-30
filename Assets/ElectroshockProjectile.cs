using UnityEngine;

public class ElectroshockProjectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    public float lifetime = 2f;

    private Vector2 moveDirection;

    void Start()
    {
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
        if (other.CompareTag("Enemy"))
        {
            
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
               enemy.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning(gameObject.name + " [PROYECTIL] Hit ENEMY, but no EnemyHealth script was found on " + other.name);
            }
        }

        Destroy(gameObject);
    }
}