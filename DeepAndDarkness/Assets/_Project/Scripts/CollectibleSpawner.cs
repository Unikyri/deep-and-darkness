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

    private GameObject activeCollectible;
    private Coroutine pendingSpawn;

    private void Awake()
    {
        if (mainCamera == null) { mainCamera = Camera.main; }

        if (attackSpawner == null || gameSession == null || gameplayBounds == null || !gameplayBounds.IsConfigured || collectiblePrefab == null || mainCamera == null)
        {
            Debug.LogError("CollectibleSpawner necesita sus referencias asignadas.", this);
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
        if (patternNumber == 3 || patternNumber == 7 || patternNumber == 10) { return true; }

        return patternNumber > 10 && (patternNumber - 10) % 3 == 0;
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
        bool spawnInUpperWorld = nextCollectibleNumber % 2 == 1;
        float spawnY = spawnInUpperWorld ? gameplayBounds.GetBatLaneY(collectibleLaneRatio) : gameplayBounds.GetMermaidLaneY(collectibleLaneRatio);
        Vector3 cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(mainCamera.transform.position.z - transform.position.z)));
        float spawnX = cameraRightEdge.x + spawnMargin + trailingOffset;
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, transform.position.z);

        activeCollectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);

        CollectiblePickup pickup = activeCollectible.GetComponent<CollectiblePickup>();

        if (pickup == null)
        {
            Debug.LogError("Collectible.prefab necesita el componente CollectiblePickup.", activeCollectible);
            Destroy(activeCollectible);
            activeCollectible = null;
            return;
        }

        pickup.Initialize(gameSession, nextCollectibleNumber);

        string worldName = spawnInUpperWorld ? "superior" : "inferior";
        Debug.Log($"Apareció el coleccionable {nextCollectibleNumber} en el mundo {worldName}.");
    }
}
