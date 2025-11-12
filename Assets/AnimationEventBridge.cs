using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    public ZombieController zombieController;

    public void DealDamageFromAnimation()
    {

        if (zombieController != null)
        {
            zombieController.DealDamageFromAnimation();
        }
        else
        {
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