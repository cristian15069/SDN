using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 3;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " ha recibido " + damageAmount + " de daño. Salud restante: " + currentHealth);

        // Aquí puedes añadir un efecto visual de daño (parpadeo, etc.)
        // anim.SetTrigger("Hit"); 

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha sido derrotado.");
        Destroy(gameObject);
    }
}