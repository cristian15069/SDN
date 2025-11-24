using UnityEngine;

public class followCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Suavizado")]
    public float smoothSpeed = 5f; 
    
    [Header("Look Ahead (Mirar Adelante)")]
    public float lookAheadDistance = 3f; // Qué tan lejos se adelanta la cámara
    public float lookAheadSpeed = 2f;    // Qué tan rápido cambia de lado
    private float currentLookOffset;

    [Header("Límites del Mapa")]
    public bool useLimits = false;
    public float minX;
    public float maxX;

    // Variables internas
    private float fixedY;
    private float lastXPos;

    void Start()
    {
        if (target == null) return;

        // Guardamos la altura inicial de la cámara para no moverla en Y
        fixedY = transform.position.y;
        lastXPos = target.position.x;
    }

    void Update()
    {
        if (target == null) return;

        // 1. DETECTAR DIRECCIÓN
        // Comparamos la posición actual con la del frame anterior
        float xDifference = target.position.x - lastXPos;
        
        float targetOffset = 0;

        // Si se mueve a la derecha (diferencia positiva)
        if (xDifference > 0.01f)
        {
            targetOffset = lookAheadDistance;
        }
        // Si se mueve a la izquierda (diferencia negativa)
        else if (xDifference < -0.01f)
        {
            targetOffset = -lookAheadDistance;
        }
        else
        {
            // Si está quieto, mantenemos el último offset para que la cámara no se centre de golpe
            targetOffset = currentLookOffset;
        }

        // Actualizamos la posición anterior para el siguiente frame
        lastXPos = target.position.x;

        // 2. SUAVIZAR EL CAMBIO DE LADO
        // Esto hace que la cámara se deslice suavemente de izquierda a derecha
        currentLookOffset = Mathf.Lerp(currentLookOffset, targetOffset, lookAheadSpeed * Time.deltaTime);

        // 3. CALCULAR POSICIÓN DESEADA
        // Posición del jugador + el adelanto calculado
        Vector3 desiredPosition = new Vector3(target.position.x + currentLookOffset, fixedY, transform.position.z);

        // 4. APLICAR LÍMITES (Si están activados)
        if (useLimits)
        {
            // El Clamp evita que la cámara pase de los bordes minX y maxX
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        // 5. MOVER LA CÁMARA
        // Usamos Lerp para mover la cámara suavemente hacia la posición deseada
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}