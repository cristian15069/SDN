using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; 

    [Header("Configuración de Seguimiento")]
    public float smoothSpeed = 10f; 
    public Vector3 offset = new Vector3(0, 1, -10); 

    // --- ¡NUEVAS VARIABLES! ---
    [Header("Límites de la Cámara")]
    public float minX; // El borde izquierdo
    public float maxX; // El borde derecho
    
    private float fixedY;

    void Start()
    {
        fixedY = transform.position.y + offset.y;
    }

    void LateUpdate ()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // --- ¡NUEVA LÍNEA! ---
        // Sujeta la posición X entre los límites minX y maxX
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);

        // Aplicamos la X sujetada y la Y fija
        transform.position = new Vector3(smoothedPosition.x, fixedY, transform.position.z);
    }
}