using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        kaiAnimation player = FindFirstObjectByType<kaiAnimation>();

        if (player != null)
        {
            player.AddKill();
        }

        Destroy(gameObject);
    }
}