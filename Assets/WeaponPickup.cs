using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ¿Estás seguro de que el jugador tiene el tag "Player"?
        if (other.CompareTag("Player"))
        {
            // 2. ¿Estás seguro de que estás obteniendo el script "kaiAnimation"?
            kaiAnimation playerScript = other.GetComponent<kaiAnimation>();

            if (playerScript != null)
            {
                // 3. ¡Llama a la función!
                playerScript.PickUpWeapon();
                
                // 4. Destruye el arma del suelo
                Destroy(gameObject); 
            }
        }
    }
}