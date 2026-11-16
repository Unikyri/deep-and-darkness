using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CollectibleTrajectory : MonoBehaviour
{
    private enum PathShape { Sine, Cosine, Spiral }

    private Rigidbody2D body;
    private Camera mainCamera;
    private PathShape pathShape;
    private float startX;
    private float centerY;
    private float verticalRadius;
    private float horizontalSpeed;
    private float cycleDuration;
    private float spiralHorizontalRadius;
    private float spiralExpansionDuration;
    private float rightExtent;
    private float elapsedTime;
    private bool isConfigured;

    public string PathName
    {
        get
        {
            if (pathShape == PathShape.Sine) { return "seno"; }
            if (pathShape == PathShape.Cosine) { return "coseno"; }
            return "espiral";
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public bool Initialize(GameplayBounds bounds, Camera camera, int collectibleNumber, float speed, float period, float spiralRadius, float expansionDuration)
    {
        if (bounds == null || !bounds.IsConfigured || camera == null || collectibleNumber < 1) { return false; }

        mainCamera = camera;
        horizontalSpeed = Mathf.Max(0.1f, speed);
        cycleDuration = Mathf.Max(0.5f, period);
        spiralHorizontalRadius = Mathf.Max(0f, spiralRadius);
        spiralExpansionDuration = Mathf.Max(0.1f, expansionDuration);
        pathShape = (PathShape)((collectibleNumber - 1) % 3);
        startX = body.position.x;

        float topExtent = 0f;
        float bottomExtent = 0f;

        if (VisibleBounds2D.TryGetBounds(gameObject, out Bounds visibleBounds))
        {
            topExtent = Mathf.Max(0f, visibleBounds.max.y - transform.position.y);
            bottomExtent = Mathf.Max(0f, transform.position.y - visibleBounds.min.y);
            rightExtent = Mathf.Max(0f, visibleBounds.max.x - transform.position.x);
        }

        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        float cameraBottom = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, cameraDepth)).y;
        float cameraTop = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, cameraDepth)).y;
        float minY = Mathf.Max(bounds.MermaidMinimumY, cameraBottom + bottomExtent);
        float maxY = Mathf.Min(bounds.BatMaximumY, cameraTop - topExtent);

        if (minY >= maxY) { return false; }

        centerY = (minY + maxY) * 0.5f;
        verticalRadius = (maxY - minY) * 0.5f;
        body.position = new Vector2(startX, GetVerticalPosition(0f));
        isConfigured = true;
        return true;
    }

    private void FixedUpdate()
    {
        if (!isConfigured) { return; }

        elapsedTime += Time.fixedDeltaTime;
        float centerX = startX - horizontalSpeed * elapsedTime;
        float offsetX = GetHorizontalOffset(elapsedTime);
        Vector2 nextPosition = new Vector2(centerX + offsetX, GetVerticalPosition(elapsedTime));

        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        float cameraLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDepth)).x;
        float maxOffsetX = pathShape == PathShape.Spiral ? spiralHorizontalRadius : 0f;

        // Esperamos a que toda la espiral quede fuera antes de destruir el objeto
        if (centerX + maxOffsetX + rightExtent < cameraLeft - 0.5f)
        {
            Destroy(gameObject);
            return;
        }

        body.MovePosition(nextPosition);
    }

    private float GetVerticalPosition(float time)
    {
        float phase = time * Mathf.PI * 2f / cycleDuration;

        if (pathShape == PathShape.Sine) { return centerY + verticalRadius * Mathf.Sin(phase); }
        if (pathShape == PathShape.Cosine) { return centerY + verticalRadius * Mathf.Cos(phase); }

        return centerY + verticalRadius * GetSpiralGrowth(time) * Mathf.Sin(phase + Mathf.PI * 0.5f);
    }

    private float GetHorizontalOffset(float time)
    {
        if (pathShape != PathShape.Spiral) { return 0f; }

        float phase = time * Mathf.PI * 2f / cycleDuration + Mathf.PI * 0.5f;
        return spiralHorizontalRadius * GetSpiralGrowth(time) * Mathf.Cos(phase);
    }

    private float GetSpiralGrowth(float time)
    {
        // El radio crece y luego se mantiene para seguir pasando por ambos mundos
        return Mathf.Lerp(0.25f, 1f, Mathf.Clamp01(time / spiralExpansionDuration));
    }
}
