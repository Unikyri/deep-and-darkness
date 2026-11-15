using UnityEngine;

public class CharacterIdleBob : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Límites del escenario usados para detectar la zona de reposo de este personaje.")]
    [SerializeField] private GameplayBounds gameplayBounds;
    [Tooltip("Mundo cuyos límites se usan: Upper para el vampiro y Lower para la sirena.")]
    [SerializeField] private GameplayBounds.WorldSide worldSide;

    [Header("Detección del modo idle")]
    [Tooltip("Distancia vertical máxima al límite de reposo para considerar que el personaje llegó a esa zona.")]
    [SerializeField, Min(0f)] private float positionTolerance = 0.06f;
    [Tooltip("Tiempo en segundos que debe permanecer en la zona de reposo antes de iniciar el balanceo.")]
    [SerializeField, Min(0f)] private float idleDelay = 1f;

    [Header("Animación Bob")]
    [Tooltip("Desplazamiento vertical máximo del visual durante el balanceo, en unidades locales.")]
    [SerializeField, Min(0f)] private float amplitude = 0.1f;
    [Tooltip("Cantidad de ciclos completos de balanceo por segundo.")]
    [SerializeField, Min(0.01f)] private float cyclesPerSecond = 0.5f;
    [Tooltip("Rapidez con la que el balanceo aparece al activarse y se desvanece al desactivarse.")]
    [SerializeField, Min(0f)] private float fadeSpeed = 4f;
    [Tooltip("Multiplicador que determina la dirección del balanceo; usa valores opuestos para que ambos personajes oscilen en espejo.")]
    [SerializeField] private float bobDirection = 1f;

    private Rigidbody2D rb;
    private Vector3 initialLocalPosition;
    private float idleTimer;
    private float bobWeight;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        initialLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float restingY = gameplayBounds.GetInnerLimitY(worldSide);

        bool nearRestingY = Mathf.Abs(rb.position.y - restingY) <= positionTolerance;

        if (nearRestingY)
        {
            idleTimer = Mathf.Min(idleTimer + Time.deltaTime, idleDelay);
        }
        else { idleTimer = 0f; }

        bool shouldBob = nearRestingY && idleTimer >= idleDelay;
        float targetWeight = shouldBob ? 1f : 0f;

        bobWeight = Mathf.MoveTowards(bobWeight, targetWeight, fadeSpeed * Time.deltaTime);

        float wave = Mathf.Sin(Time.time * Mathf.PI * 2f * cyclesPerSecond);

        float yOffset = wave * amplitude * bobWeight * bobDirection;

        Vector3 visualPosition = initialLocalPosition;
        visualPosition.y += yOffset;
        transform.localPosition = visualPosition;
    }
}
