using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    public ZombieController zombieController;

    public void DealDamageFromAnimation()
    {
        // TRAMPA 1: ¿Se está llamando esta función?
        Debug.Log("--- 1. Evento 'DealDamage' RECIBIDO por el 'Bridge' (Hijo) ---");

        if (zombieController != null)
        {
            // TRAMPA 2: ¿Está conectado al Padre?
            Debug.Log("--- 2. 'Bridge' está conectado. Pasando mensaje al 'Padre' (ZombieController)... ---");
            zombieController.DealDamageFromAnimation();
        }
        else
        {
            // TRAP 2 (FALLO): ¡No está conectado!
            Debug.LogError("¡ERROR DE PUENTE! El 'Animator' (Hijo) disparó el evento, pero la casilla 'Zombie Controller' en 'AnimationEventBridge' (Hijo) está VACÍA.");
        }
    }

    public void OnAttackAnimationComplete()
    {
        if (zombieController != null)
        {
            zombieController.OnAttackAnimationComplete();
        }
        else
        {
            Debug.LogError("¡ERROR DE PUENTE! El 'Animator' (Hijo) disparó el evento 'OnAttackAnimationComplete', pero la casilla 'Zombie Controller' en 'AnimationEventBridge' (Hijo) está VACÍA.");
        }
    }


}