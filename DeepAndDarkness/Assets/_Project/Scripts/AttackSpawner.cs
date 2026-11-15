using System;
using System.Collections;
using UnityEngine;

public class AttackSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab que se instancia para cada ataque. Debe incluir el componente HazardMovement.")]
    [SerializeField] private GameObject hazardPrefab;
    [Tooltip("Cámara cuya vista se usa para calcular el punto de aparición de los ataques.")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("Límites que determinan las alturas de los carriles en ambos mundos.")]
    [SerializeField] private GameplayBounds gameplayBounds;
    [Tooltip("Sesión usada para consultar los coleccionables recogidos y ajustar la velocidad de los ataques.")]
    [SerializeField] private GameSessionManager gameSession;

    [Header("Carriles normalizados")]
    [Tooltip("Posición del carril del patrón 1 entre el límite interior (0) y el exterior (1) de cada mundo.")]
    [SerializeField, Range(0f, 1f)] private float patternOneLaneRatio = 0.315f;
    [Tooltip("Posición normalizada del carril exterior usado por los patrones 2 y 4.")]
    [SerializeField, Range(0f, 1f)] private float outerLaneRatio = 0.726f;
    [Tooltip("Posición normalizada del carril interior usado por los patrones 3 y 4.")]
    [SerializeField, Range(0f, 1f)] private float innerLaneRatio = 0.178f;

    [Header("Frecuencia")]
    [Tooltip("Tiempo en segundos que se espera antes de generar el siguiente patrón.")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;
    [Tooltip("Tiempo en segundos entre las dos parejas de ataques del patrón 4.")]
    [SerializeField, Min(0.1f)] private float patternFourSubwaveDelay = 2f;
    [Tooltip("Distancia adicional fuera del borde derecho de la cámara donde aparecen los ataques.")]
    [SerializeField, Min(0f)] private float spawnMargin = 0.75f;

    [Header("Variantes de velocidad")]
    [Tooltip("Velocidad horizontal base de la variante normal; aumenta con cada coleccionable recogido.")]
    [SerializeField, Min(0f)] private float normalSpeed = 5f;
    [Tooltip("Velocidad horizontal base de la variante rápida; aumenta con cada coleccionable recogido.")]
    [SerializeField, Min(0f)] private float fastSpeed = 6.5f;
    [Tooltip("Probabilidad entre 0 y 1 de elegir la variante rápida para un patrón.")]
    [SerializeField, Range(0f, 1f)] private float fastSpeedChance = 0.2f;
    [Tooltip("Velocidad que se suma a ambas variantes por cada coleccionable de misión recogido.")]
    [SerializeField, Min(0f)] private float speedIncreasePerCollectible = 0.5f;

    private int lastPatternIndex = -1;
    private int completedPatternCount;


    public event Action<int> PatternCompleted;

    private void Awake()
    {
        if (mainCamera == null) { mainCamera = Camera.main; }
    }

    private void Start()
    {
        StartCoroutine(RunPatternLoop());
    }

    private IEnumerator RunPatternLoop()
    {
        SpawnPatternOne();
        lastPatternIndex = 0;
        NotifyPatternCompleted();

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            int nextPatternIndex = GetNextPatternIndex();

            if (nextPatternIndex == 0) { SpawnPatternOne(); }
            else if (nextPatternIndex == 1) { SpawnPatternTwo(); }
            else if (nextPatternIndex == 2) { SpawnPatternThree(); }
            else { yield return StartCoroutine(SpawnPatternFour()); }

            NotifyPatternCompleted();
        }
    }

    private int GetNextPatternIndex()
    {
        int selectedPatternIndex = UnityEngine.Random.Range(0, 4);

        while (selectedPatternIndex == lastPatternIndex)
        {
            selectedPatternIndex = UnityEngine.Random.Range(0, 4);
        }

        lastPatternIndex = selectedPatternIndex;
        return selectedPatternIndex;
    }

    private void NotifyPatternCompleted()
    {
        completedPatternCount++;
        Debug.Log($"Patrón {completedPatternCount} completado.");

        if (PatternCompleted != null) { PatternCompleted(completedPatternCount); }
    }

    private void SpawnPatternOne()
    {
        SpawnMirroredPair(GetSpawnX(), patternOneLaneRatio, GetSelectedSpeed());
    }

    private void SpawnPatternTwo()
    {
        SpawnMirroredPair(GetSpawnX(), outerLaneRatio, GetSelectedSpeed());
    }

    private void SpawnPatternThree()
    {
        SpawnMirroredPair(GetSpawnX(), innerLaneRatio, GetSelectedSpeed());
    }

    private IEnumerator SpawnPatternFour()
    {
        float selectedSpeed = GetSelectedSpeed();

        SpawnMirroredPair(GetSpawnX(), outerLaneRatio, selectedSpeed);

        yield return new WaitForSeconds(patternFourSubwaveDelay);

        SpawnMirroredPair(GetSpawnX(), innerLaneRatio, selectedSpeed);
    }

    private float GetSelectedSpeed()
    {
        int collectedCount = gameSession.CollectedCollectibles;
        float currentNormalSpeed = normalSpeed + collectedCount * speedIncreasePerCollectible;
        float currentFastSpeed = fastSpeed + collectedCount * speedIncreasePerCollectible;

        return UnityEngine.Random.value < fastSpeedChance ? currentFastSpeed : currentNormalSpeed;
    }

    private float GetSpawnX()
    {
        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, cameraDepth));
        return cameraRightEdge.x + spawnMargin;
    }

    private void SpawnMirroredPair(float spawnX, float laneRatio, float speed)
    {
        float batY = gameplayBounds.GetBatLaneY(laneRatio);
        float mermaidY = gameplayBounds.GetMermaidLaneY(laneRatio);

        SpawnHazard(spawnX, batY, speed);
        SpawnHazard(spawnX, mermaidY, speed);
    }

    private void SpawnHazard(float spawnX, float spawnY, float speed)
    {
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, transform.position.z);
        GameObject spawnedHazard = Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);
        HazardMovement hazardMovement = spawnedHazard.GetComponent<HazardMovement>();

        if (hazardMovement == null)
        {
            Destroy(spawnedHazard);
            return;
        }

        hazardMovement.SetHorizontalSpeed(speed);
    }
}
