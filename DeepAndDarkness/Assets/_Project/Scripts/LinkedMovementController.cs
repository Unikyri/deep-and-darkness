using UnityEngine;
using UnityEngine.InputSystem;

public class LinkedMovementController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Rigidbody2D del vampiro, usado para aplicar y limitar su movimiento vertical.")]
    [SerializeField] private Rigidbody2D batRb;
    [Tooltip("Rigidbody2D de la sirena, usado para aplicar el movimiento vertical opuesto al del vampiro.")]
    [SerializeField] private Rigidbody2D mermaiRb;
    [Tooltip("Acción de entrada que, mientras se mantiene pulsada, impulsa a los personajes en direcciones opuestas.")]
    [SerializeField] private InputActionReference liftReference;
    [Tooltip("Límites verticales de movimiento para ambos personajes.")]
    [SerializeField] private GameplayBounds gameplayBounds;

    [Header("Movimiento")]
    [Tooltip("Fuerza vertical aplicada mientras se mantiene pulsada la acción: hacia arriba para el vampiro y hacia abajo para la sirena.")]
    [SerializeField] private float liftForce = 15f;
    [Tooltip("Velocidad vertical máxima permitida para cada personaje, en unidades por segundo.")]
    [SerializeField] private float maxVerticalSpeed = 6f;

    private void Awake()
    {
        //Invertimos la gravedad de la sirena con respecto a la del Vampiro
        mermaiRb.gravityScale = -Mathf.Abs(batRb.gravityScale);
    }

    private void OnEnable()
    {
        liftReference.action.Enable();
    }

    private void OnDisable()
    {

        liftReference.action.Disable();
    }

    private void FixedUpdate()
    {
        if (liftReference.action.IsPressed())
        {
            batRb.AddForce(Vector2.up * liftForce, ForceMode2D.Force);
            mermaiRb.AddForce(Vector2.down * liftForce, ForceMode2D.Force);
        }

        LimitVerticalSpeed(batRb);
        LimitVerticalSpeed(mermaiRb);

        ClampVerticalPosition(batRb, gameplayBounds.BatMinimumY, gameplayBounds.BatMaximumY);

        ClampVerticalPosition(mermaiRb, gameplayBounds.MermaidMinimumY, gameplayBounds.MermaidMaximumY);
    }

    private void LimitVerticalSpeed(Rigidbody2D rb)
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalSpeed, maxVerticalSpeed);

        rb.linearVelocity = velocity;
    }

    private void ClampVerticalPosition(Rigidbody2D rb, float minimumY, float maximumY)
    {
        Vector2 position = rb.position;
        float clampedY = Mathf.Clamp(position.y, minimumY, maximumY);

        if (Mathf.Approximately(position.y, clampedY))
        {
            return;
        }

        rb.position = new Vector2(position.x, clampedY);

        Vector2 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;
    }
}
