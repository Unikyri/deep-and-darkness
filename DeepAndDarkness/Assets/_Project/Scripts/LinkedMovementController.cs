using System;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LinkedMovementController : MonoBehaviour
{

    [SerializeField] private Rigidbody2D batRb;
    [SerializeField] private Rigidbody2D mermaiRb;
    [SerializeField] private InputActionReference liftReference;


    [Header("Movimiento")]
    [SerializeField] private float liftForce = 15f;
    [SerializeField] private float maxVerticalSpeed = 6f;

    [Header("Movimiento vertical del Vampiro")]
    [SerializeField] private float batMinimumY = 0.5f;
    [SerializeField] private float batMaximumY = 4.5f;

    [Header("Movimiento vertical de la Sirena")]
    [SerializeField] private float mermaidMinimumY = -4.5f;
    [SerializeField] private float mermaidMaximumY = -0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        mermaiRb.gravityScale = -Mathf.Abs(batRb.gravityScale); //Le asigna el valor negativo de la gravedad  del vampiro a la sirena
    }

    void OnEnable()
    {
        liftReference.action.Enable();
    }

    void OnDisable()
    {
        liftReference.action.Disable();
    }

    void FixedUpdate()
    {
        if (liftReference.action.IsPressed())
        {
            batRb.AddForce(Vector2.up * liftForce, ForceMode2D.Force);
            mermaiRb.AddForce(Vector2.down * liftForce, ForceMode2D.Force);
        }

        LimitVerticalSpeed(batRb);
        LimitVerticalSpeed(mermaiRb);

        ClampVerticalPosition(batRb, batMinimumY, batMaximumY);

        ClampVerticalPosition(mermaiRb, mermaidMinimumY, mermaidMaximumY);

    }

    //Función que limita la velocidad vertical de ambos personajes
    private void LimitVerticalSpeed(Rigidbody2D rb)
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalSpeed, maxVerticalSpeed);
        rb.linearVelocity = velocity;
    }

    //Establece los limites de cada personaje
    private void ClampVerticalPosition(Rigidbody2D rb, float minY, float maxY)
    {
        Vector2 position = rb.position;
        float clampedY = Mathf.Clamp(position.y, minY, maxY);

        if (Mathf.Approximately(position.y, clampedY))
        {
            return;
        }

        rb.position = new Vector2(position.x, clampedY);

        Vector2 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;
    }

    void Start()
    {

    }



    // Update is called once per frame
    void Update()
    {



    }
}
