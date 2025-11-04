using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            kaiAnimation player = other.GetComponent<kaiAnimation>();

            if (player != null)
            {
                player.SetRespawnPoint(this.transform);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f); 
        Gizmos.DrawCube(transform.position, GetComponent<BoxCollider2D>().size);
    }
}