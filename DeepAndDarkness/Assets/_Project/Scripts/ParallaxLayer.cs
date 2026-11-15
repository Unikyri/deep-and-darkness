using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("SpriteRenderer del mundo superior. El script repetirá su sprite hacia la derecha.")]
    [SerializeField] private SpriteRenderer upperWorldRenderer;

    [Tooltip("SpriteRenderer del mundo inferior. El script repetirá su sprite hacia la derecha.")]
    [SerializeField] private SpriteRenderer lowerWorldRenderer;

    [Tooltip("Cámara usada para detectar cuándo una franja salió completamente de la pantalla.")]
    [SerializeField] private Camera mainCamera;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento de esta capa hacia la izquierda, en unidades por segundo.")]
    [SerializeField, Min(0f)] private float scrollSpeed = 1f;

    private SpriteRenderer upperWorldCopy;
    private SpriteRenderer lowerWorldCopy;

    private void Start()
    {
        if (mainCamera == null) { mainCamera = Camera.main; }

        if (mainCamera == null)
        {
            Debug.LogError("ParallaxLayer necesita una cámara asignada o una cámara con el tag MainCamera.", this);
            enabled = false;
            return;
        }

        upperWorldCopy = CreateLoopCopy(upperWorldRenderer);
        lowerWorldCopy = CreateLoopCopy(lowerWorldRenderer);
    }

    private void Update()
    {
        MoveAndLoop(upperWorldRenderer, upperWorldCopy);
        MoveAndLoop(lowerWorldRenderer, lowerWorldCopy);
    }

    private SpriteRenderer CreateLoopCopy(SpriteRenderer sourceRenderer)
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null) { return null; }

        float tileWidth = sourceRenderer.bounds.size.x;
        if (tileWidth <= 0f) { return null; }

        GameObject copyObject = Instantiate(sourceRenderer.gameObject, sourceRenderer.transform.parent);
        copyObject.name = sourceRenderer.gameObject.name + "_Loop";
        copyObject.transform.position = sourceRenderer.transform.position + Vector3.right * tileWidth;

        return copyObject.GetComponent<SpriteRenderer>();
    }

    private void MoveAndLoop(SpriteRenderer original, SpriteRenderer copy)
    {
        if (original == null || copy == null) { return; }

        Vector3 movement = Vector3.left * scrollSpeed * Time.deltaTime;
        original.transform.position += movement;
        copy.transform.position += movement;

        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - original.transform.position.z);
        Vector3 cameraLeftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDepth));

        if (original.bounds.max.x <= cameraLeftEdge.x)
        {
            original.transform.position = copy.transform.position + Vector3.right * original.bounds.size.x;
        }

        if (copy.bounds.max.x <= cameraLeftEdge.x)
        {
            copy.transform.position = original.transform.position + Vector3.right * copy.bounds.size.x;
        }
    }
}