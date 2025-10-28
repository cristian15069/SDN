using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public string playerTag = "Player"; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            kaiAnimation playerScript = other.GetComponent<kaiAnimation>();
            if (playerScript != null)
            {
                playerScript.PickUpWeapon(); 
            }

            Destroy(gameObject); 
        }
    }
}