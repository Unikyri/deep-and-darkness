using System.Collections;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Spawner de ataques que comunica cuándo termina cada patrón y permite programar la aparición de coleccionables.")]
    [SerializeField] private AttackSpawner attackSpawner;
    [Tooltip("Sesión consultada para comprobar el progreso, la victoria y si el juego terminó.")]
    [SerializeField] private GameSessionManager gameSession;
    [Tooltip("Límites que determinan la altura del carril donde aparece cada coleccionable.")]
    [SerializeField] private GameplayBounds gameplayBounds;
    [Tooltip("Prefab que se crea para representar el siguiente coleccionable de misión.")]
    [SerializeField] private GameObject collectiblePrefab;
    [Tooltip("Sprites de los coleccionables de este nivel, en el orden en que se recogen.")]
    [SerializeField] private Sprite[] collectibleSprites;
    [Tooltip("Cámara cuya vista se usa para calcular la posición de aparición del coleccionable.")]
    [SerializeField] private Camera mainCamera;

    [Header("Aparición")]
    [Tooltip("Posición normalizada del carril entre el límite interior (0) y el exterior (1) del mundo elegido.")]
    [SerializeField, Range(0f, 1f)] private float collectibleLaneRatio = 0.75f;
    [Tooltip("Distancia adicional más allá del borde derecho de la cámara donde se crea el coleccionable.")]
    [SerializeField, Min(0f)] private float spawnMargin = 0.75f;
    [Tooltip("Distancia adicional que retrasa la aparición del coleccionable respecto al borde de la cámara.")]
    [SerializeField, Min(0f)] private float trailingOffset = 2f;
    [Tooltip("Tiempo en segundos que se espera después del patrón indicado antes de crear el coleccionable.")]
    [SerializeField, Min(0f)] private float spawnDelayAfterPattern = 2f;
    [Tooltip("Separación visible mínima entre el coleccionable y los ataques ya presentes en el nivel 2")]
    [SerializeField, Min(0f)] private float minimumAttackCollectibleGap = 1.5f;

    [Header("Trayectorias de coleccionables del nivel 2")]
    [Tooltip("Velocidad horizontal de los coleccionables mientras recorren ambos mundos")]
    [SerializeField, Min(0.1f)] private float collectibleTrajectorySpeed = 5f;
    [Tooltip("Segundos que tarda el coleccionable en completar un ciclo de su trayectoria")]
    [SerializeField, Min(0.5f)] private float collectibleTrajectoryPeriod = 4f;
    [Tooltip("Radio horizontal máximo de la trayectoria en espiral")]
    [SerializeField, Min(0f)] private float collectibleSpiralRadius = 3.5f;
    [Tooltip("Tiempo que tarda la espiral en alcanzar su radio máximo")]
    [SerializeField, Min(0.1f)] private float collectibleSpiralExpansionTime = 2f;

    private GameObject activeCollectible;
    private Coroutine pendingSpawn;

    private void Awake()
    {
        if (mainCamera == null) { mainCamera = Camera.main; }

        if (attackSpawner == null || gameSession == null || gameplayBounds == null || !gameplayBounds.IsConfigured || collectiblePrefab == null || mainCamera == null)
        {
            Debug.LogError("CollectibleSpawner necesita sus referencias asignadas.", this);
            enabled = false;
            return;
        }

        if (collectibleSprites == null || collectibleSprites.Length < gameSession.RequiredCollectibles)
        {
            Debug.LogError("CollectibleSpawner necesita un sprite por cada coleccionable requerido en este nivel.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (attackSpawner != null) { attackSpawner.PatternCompleted += HandlePatternCompleted; }
    }

    private void OnDisable()
    {
        if (attackSpawner != null) { attackSpawner.PatternCompleted -= HandlePatternCompleted; }
    }

    private void HandlePatternCompleted(int patternNumber)
    {
        if (!IsCollectibleMilestone(patternNumber)) { return; }
        if (gameSession.IsGameOver || gameSession.CollectedCollectibles >= gameSession.RequiredCollectibles) { return; }
        if (activeCollectible != null || pendingSpawn != null) { return; }

        pendingSpawn = StartCoroutine(SpawnCollectibleAfterDelay());
    }

    private bool IsCollectibleMilestone(int patternNumber)
    {
        return patternNumber > 0 && patternNumber % 2 == 0;
    }

    private IEnumerator SpawnCollectibleAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelayAfterPattern);

        pendingSpawn = null;

        if (gameSession.IsGameOver || gameSession.CollectedCollectibles >= gameSession.RequiredCollectibles || activeCollectible != null) { yield break; }

        SpawnCollectible();
    }

    private void SpawnCollectible()
    {
        int nextCollectibleNumber = gameSession.CollectedCollectibles + 1;
        if (nextCollectibleNumber > collectibleSprites.Length || collectibleSprites[nextCollectibleNumber - 1] == null)
        {
            Debug.LogError($"Falta asignar el sprite del coleccionable {nextCollectibleNumber} en CollectibleSpawner.", this);
            return;
        }

        bool spawnInUpperWorld = nextCollectibleNumber % 2 == 1;
        float spawnY = spawnInUpperWorld ? gameplayBounds.GetBatLaneY(collectibleLaneRatio) : gameplayBounds.GetMermaidLaneY(collectibleLaneRatio);
        Vector3 cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(mainCamera.transform.position.z - transform.position.z)));
        float spawnX = cameraRightEdge.x + spawnMargin + trailingOffset;

        if (attackSpawner.IndependentWorldPatterns)
        {
            spawnX = GetSeparatedSpawnX(spawnX);
        }

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, transform.position.z);

        activeCollectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);

        SpriteRenderer spriteRenderer = activeCollectible.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("Collectible.prefab necesita un SpriteRenderer en su jerarquía.", activeCollectible);
            Destroy(activeCollectible);
            activeCollectible = null;
            return;
        }

        spriteRenderer.sprite = collectibleSprites[nextCollectibleNumber - 1];
        spriteRenderer.color = Color.white;

        CollectiblePickup pickup = activeCollectible.GetComponent<CollectiblePickup>();

        if (pickup == null)
        {
            Debug.LogError("Collectible.prefab necesita el componente CollectiblePickup.", activeCollectible);
            Destroy(activeCollectible);
            activeCollectible = null;
            return;
        }

        pickup.Initialize(gameSession, nextCollectibleNumber);

        if (attackSpawner.IndependentWorldPatterns)
        {
            HazardMovement straightMovement = activeCollectible.GetComponent<HazardMovement>();
            if (straightMovement != null) { straightMovement.enabled = false; }

            CollectibleTrajectory trajectory = activeCollectible.AddComponent<CollectibleTrajectory>();

            if (!trajectory.Initialize(gameplayBounds, mainCamera, nextCollectibleNumber, collectibleTrajectorySpeed, collectibleTrajectoryPeriod, collectibleSpiralRadius, collectibleSpiralExpansionTime))
            {
                Debug.LogError("No se pudo configurar la trayectoria del coleccionable", activeCollectible);
                Destroy(activeCollectible);
                activeCollectible = null;
                return;
            }

            Debug.Log($"Apareció el coleccionable {nextCollectibleNumber} con trayectoria de {trajectory.PathName}.");
            return;
        }

        string worldName = spawnInUpperWorld ? "superior" : "inferior";
        Debug.Log($"Apareció el coleccionable {nextCollectibleNumber} en el mundo {worldName}.");
    }

    private float GetSeparatedSpawnX(float spawnX)
    {
        if (!VisibleBounds2D.TryGetBounds(collectiblePrefab, out Bounds collectibleBounds))
        {
            Debug.LogWarning("No se pudo medir el tamaño del coleccionable para separarlo de los ataques", this);
            return spawnX;
        }

        float collectibleLeft = Mathf.Max(0f, collectiblePrefab.transform.position.x - collectibleBounds.min.x);
        // El coleccionable cruzará ambos mundos, así que miramos todos los ataques presentes
        foreach (HazardDamage hazard in FindObjectsByType<HazardDamage>(FindObjectsSortMode.None))
        {
            if (!VisibleBounds2D.TryGetBounds(hazard.gameObject, out Bounds hazardBounds)) { continue; }

            float separatedX = hazardBounds.max.x + collectibleLeft + Mathf.Max(0f, minimumAttackCollectibleGap);
            spawnX = Mathf.Max(spawnX, separatedX);
        }

        return spawnX;
    }
}
