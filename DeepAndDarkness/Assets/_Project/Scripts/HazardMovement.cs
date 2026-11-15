using UnityEngine;

public class HazardMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad horizontal del ataque al desplazarse hacia la izquierda, en unidades por segundo.")]
    [SerializeField, Min(0f)] private float horizontalSpeed = 5f;

    [Header("Salida de pantalla")]
    [Tooltip("Distancia adicional más allá del borde izquierdo de la cámara que el ataque debe recorrer antes de destruirse.")]
    [SerializeField, Min(0f)] private float despawnMargin = 0.5f;

    private Rigidbody2D hazardRb;
    private Collider2D hazardCollider;
    private Camera mainCamera;

    private void Awake()
    {
        hazardRb = GetComponent<Rigidbody2D>();
        hazardCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
    }

    public void SetHorizontalSpeed(float newSpeed)
    {
        horizontalSpeed = Mathf.Max(0f, newSpeed);
    }

    private void FixedUpdate()
    {
        // Calculo para mover el ataque/obstaculo
        Vector2 nextPosition = hazardRb.position + Vector2.left * horizontalSpeed * Time.fixedDeltaTime;

        // Establece dónde está el final del escenario
        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 cameraLeftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDepth));

        // Calculo para asegurarnos de que el obstaculo o ataque salga por completo desde la derecha
        float hazardRightEdgeX = nextPosition.x + hazardCollider.bounds.extents.x;

        // Destruye el ataque/obstaculo cuando sale de la pantalla
        if (hazardRightEdgeX < cameraLeftEdge.x - despawnMargin)
        {
            Destroy(gameObject);
            return;
        }

        hazardRb.MovePosition(nextPosition);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
