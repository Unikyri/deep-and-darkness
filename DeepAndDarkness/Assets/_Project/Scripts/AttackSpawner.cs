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

    [Header("Patrones del segundo nivel")]
    [Tooltip("Actívalo para elegir patrones independientes en cada mundo y permitir oleadas dobles.")]
    [SerializeField] private bool independentWorldPatterns = false;

    [Tooltip("Probabilidad de que un patrón sea doble. 0 significa que nunca será doble y 1 que siempre lo será.")]
    [SerializeField, Range(0f, 1f)] private float doublePatternChance = 0.5f;

    [Header("Espacio para esquivar en el nivel 2")]
    [Tooltip("Personaje del mundo superior usado para calcular su tamaño visible")]
    [SerializeField] private Transform upperCharacter;

    [Tooltip("Personaje del mundo inferior usado para calcular su tamaño visible")]
    [SerializeField] private Transform lowerCharacter;

    [Tooltip("Separación mínima entre las imágenes visibles de dos ataques de un mismo patrón")]
    [SerializeField, Min(0f)] private float minimumAttackVisualGap = 0.1f;

    [Tooltip("Margen adicional entre el personaje y el ataque al calcular una ruta")]
    [SerializeField, Min(0f)] private float characterSafetyMargin = 0.05f;

    [Tooltip("Espacio mínimo para mover el centro del personaje dentro de cada ruta segura")]
    [SerializeField, Min(0.01f)] private float minimumSafeRouteWidth = 0.25f;

    [Header("Carriles normalizados")]
    [Tooltip("Posición del carril del patrón 1 entre el límite interior (0) y el exterior (1) de cada mundo.")]
    [SerializeField, Range(0f, 1f)] private float patternOneLaneRatio = 0.315f;

    [Tooltip("Posición normalizada del carril exterior usado por los patrones 2 y 4.")]
    [SerializeField, Range(0f, 1f)] private float outerLaneRatio = 0.726f;

    [Tooltip("Posición normalizada del carril interior usado por los patrones 3 y 4.")]
    [SerializeField, Range(0f, 1f)] private float innerLaneRatio = 0.178f;

    [Header("Frecuencia del nivel 1")]
    [Tooltip("Tiempo en segundos que se espera antes de generar el siguiente patrón en el nivel 1.")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;

    [Header("Frecuencia por mundo del nivel 2")]
    [Tooltip("Espera base del mundo que genera patrones con mayor frecuencia.")]
    [SerializeField, Min(0.1f)] private float fastWorldInterval = 5f;

    [Tooltip("Segundos adicionales de espera para el mundo más lento. El mínimo es 2.")]
    [SerializeField, Min(2f)] private float slowWorldExtraInterval = 2f;

    [Tooltip("Separación mínima entre una aparición de ataques y la última aparición en el otro mundo.")]
    [SerializeField, Min(2f)] private float minimumWorldSpawnGap = 2f;

    [Header("Aparición de ataques")]
    [Tooltip("Tiempo mínimo entre el primer y el segundo ataque del patrón 4.")]
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

    private float lastUpperSpawnTime = float.NegativeInfinity;
    private float lastLowerSpawnTime = float.NegativeInfinity;

    private bool hasWarnedAboutGeometry;
    private bool hasSharedSafeSide;
    private bool sharedSafeOuterSide;

    public event Action<int> PatternCompleted;

    public bool IndependentWorldPatterns
    {
        get { return independentWorldPatterns; }
    }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (independentWorldPatterns)
        {
            StartCoroutine(RunIndependentWorldLoop(true));
            StartCoroutine(RunIndependentWorldLoop(false));
        }
        else
        {
            StartCoroutine(RunPatternLoop());
        }
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

            if (nextPatternIndex == 0)
            {
                SpawnPatternOne();
            }
            else if (nextPatternIndex == 1)
            {
                SpawnPatternTwo();
            }
            else if (nextPatternIndex == 2)
            {
                SpawnPatternThree();
            }
            else
            {
                yield return StartCoroutine(SpawnPatternFour());
            }

            NotifyPatternCompleted();
        }
    }

    private IEnumerator RunIndependentWorldLoop(bool isUpperWorld)
    {
        bool isFirstPattern = true;
        float lastPatternCompletedTime = 0f;

        while (true)
        {
            while (!gameSession.IsGameOver)
            {
                bool intervalElapsed = isFirstPattern || Time.time - lastPatternCompletedTime >= GetWorldInterval(isUpperWorld);

                if (Time.timeScale > 0f && intervalElapsed && HasMinimumWorldGap(isUpperWorld))
                {
                    break;
                }

                yield return null;
            }

            if (gameSession.IsGameOver)
            {
                yield break;
            }

            bool isDoublePattern = doublePatternChance >= 1f || UnityEngine.Random.value < doublePatternChance;

            // El patrón 4 solo puede salir en una oleada sencilla
            int availablePatternCount = isDoublePattern ? 3 : 4;

            int firstPattern = UnityEngine.Random.Range(0, availablePatternCount);
            int secondPattern = -1;

            if (isDoublePattern)
            {
                secondPattern = GetDifferentDoublePattern(firstPattern);
            }

            float selectedSpeed = GetSelectedSpeed();
            float spawnX = GetSpawnX();

            string worldName = isUpperWorld ? "superior" : "inferior";
            string patterns = isDoublePattern ? $"{firstPattern + 1} y {secondPattern + 1}" : $"{firstPattern + 1}";

            Debug.Log($"Mundo {worldName}: patrón {patterns}. Aparición en el segundo {Time.time:F2}.");

            int spawnedPatternCount;
            if (isDoublePattern)
            {
                spawnedPatternCount = SpawnDoublePatternInWorld(firstPattern, secondPattern, isUpperWorld, spawnX, selectedSpeed);
            }
            else
            {
                spawnedPatternCount = SpawnFirstAttackInWorld(firstPattern, isUpperWorld, spawnX, selectedSpeed) ? 1 : 0;
            }

            if (spawnedPatternCount > 0) { RecordWorldSpawn(isUpperWorld); }

            // El patrón 4 también respeta la separación con el otro mundo
            if (firstPattern == 3 && spawnedPatternCount > 0)
            {
                yield return new WaitForSeconds(patternFourSubwaveDelay);

                while (!gameSession.IsGameOver && (Time.timeScale <= 0f || !HasMinimumWorldGap(isUpperWorld)))
                {
                    yield return null;
                }

                if (gameSession.IsGameOver)
                {
                    yield break;
                }

                float secondSpawnX = GetSpawnX();

                if (SpawnAttackInWorld(isUpperWorld, innerLaneRatio, secondSpawnX, selectedSpeed))
                {
                    RecordWorldSpawn(isUpperWorld);
                    Debug.Log($"Mundo {worldName}: segundo ataque del patrón 4 en el segundo {Time.time:F2}.");
                }
            }

            lastPatternCompletedTime = Time.time;
            isFirstPattern = false;

            for (int i = 0; i < spawnedPatternCount; i++) { NotifyPatternCompleted(); }
        }
    }

    private float GetWorldInterval(bool isUpperWorld)
    {
        float fastInterval = Mathf.Max(0.1f, fastWorldInterval);
        float slowInterval = fastInterval + Mathf.Max(2f, slowWorldExtraInterval);

        // Cada objeto intercambia cuál mundo aparece más seguido
        bool upperWorldIsFaster = gameSession.CollectedCollectibles % 2 == 0;

        return isUpperWorld == upperWorldIsFaster ? fastInterval : slowInterval;
    }

    private bool HasMinimumWorldGap(bool isUpperWorld)
    {
        float otherWorldLastSpawn = isUpperWorld ? lastLowerSpawnTime : lastUpperSpawnTime;
        float requiredGap = Mathf.Max(2f, minimumWorldSpawnGap);

        return Time.time - otherWorldLastSpawn >= requiredGap;
    }

    private void RecordWorldSpawn(bool isUpperWorld)
    {
        if (isUpperWorld)
        {
            lastUpperSpawnTime = Time.time;
        }
        else
        {
            lastLowerSpawnTime = Time.time;
        }
    }

    private int GetDifferentDoublePattern(int firstPatternIndex)
    {
        int selectedPatternIndex = UnityEngine.Random.Range(0, 3);

        while (selectedPatternIndex == firstPatternIndex)
        {
            selectedPatternIndex = UnityEngine.Random.Range(0, 3);
        }

        return selectedPatternIndex;
    }

    private bool SpawnFirstAttackInWorld(int patternIndex, bool isUpperWorld, float spawnX, float speed)
    {
        float laneRatio;

        if (patternIndex == 0)
        {
            laneRatio = patternOneLaneRatio;
        }
        else if (patternIndex == 1 || patternIndex == 3)
        {
            laneRatio = outerLaneRatio;
        }
        else
        {
            laneRatio = innerLaneRatio;
        }

        return SpawnAttackInWorld(isUpperWorld, laneRatio, spawnX, speed);
    }

    // Ajustamos la altura para que siempre quede espacio a ambos lados
    private bool SpawnAttackInWorld(bool isUpperWorld, float laneRatio, float spawnX, float speed)
    {
        if (!TryGetWorldGeometry(isUpperWorld, out float minY, out float maxY, out float clearance, out _)) { return false; }

        float routeWidth = Mathf.Max(0.01f, minimumSafeRouteWidth);
        float lowestLane = minY + clearance + routeWidth;
        float highestLane = maxY - clearance - routeWidth;

        if (lowestLane > highestLane)
        {
            WarnAboutGeometry();
            return false;
        }

        float requestedY = isUpperWorld ? gameplayBounds.GetBatLaneY(laneRatio) : gameplayBounds.GetMermaidLaneY(laneRatio);
        float spawnY = Mathf.Clamp(requestedY, lowestLane, highestLane);

        SpawnHazard(spawnX, spawnY, speed);
        return true;
    }

    private int SpawnDoublePatternInWorld(int firstPattern, int secondPattern, bool isUpperWorld, float spawnX, float speed)
    {
        if (!TryGetWorldGeometry(isUpperWorld, out float minY, out float maxY, out float clearance, out float hazardHalfHeight)) { return 0; }

        if (!hasSharedSafeSide || !HasHazardAhead())
        {
            sharedSafeOuterSide = UnityEngine.Random.value < 0.5f;
            hasSharedSafeSide = true;
        }

        float innerY = isUpperWorld ? gameplayBounds.BatInnerY : gameplayBounds.MermaidInnerY;
        float outerY = isUpperWorld ? gameplayBounds.BatOuterY : gameplayBounds.MermaidOuterY;
        bool safeSideIsHigher = (outerY > innerY) == sharedSafeOuterSide;
        int sharedSideLayout = safeSideIsHigher ? 1 : 2;
        int preferredLayout = firstPattern + secondPattern - 1;
        if (preferredLayout != 0) { preferredLayout = sharedSideLayout; }

        // Mantenemos un mismo lado libre hasta que los ataques anteriores pasen
        for (int attempt = 0; attempt < 2; attempt++)
        {
            int layout = attempt == 0 ? preferredLayout : (preferredLayout == 0 ? sharedSideLayout : 0);

            if (!TryGetDoubleLayout(layout, minY, maxY, clearance, hazardHalfHeight, out float lowerY, out float upperY)) { continue; }

            SpawnHazard(spawnX, lowerY, speed);
            SpawnHazard(spawnX, upperY, speed);
            return 2;
        }

        WarnAboutGeometry();
        return SpawnFirstAttackInWorld(firstPattern, isUpperWorld, spawnX, speed) ? 1 : 0;
    }

    private bool HasHazardAhead()
    {
        float upperCenter = (gameplayBounds.BatInnerY + gameplayBounds.BatOuterY) * 0.5f;
        float lowerCenter = (gameplayBounds.MermaidInnerY + gameplayBounds.MermaidOuterY) * 0.5f;

        foreach (HazardDamage hazard in FindObjectsByType<HazardDamage>(FindObjectsSortMode.None))
        {
            if (!VisibleBounds2D.TryGetBounds(hazard.gameObject, out Bounds hazardBounds)) { return true; }

            bool hazardIsUpper = Mathf.Abs(hazard.transform.position.y - upperCenter) <= Mathf.Abs(hazard.transform.position.y - lowerCenter);
            Transform character = hazardIsUpper ? upperCharacter : lowerCharacter;

            if (character == null || !VisibleBounds2D.TryGetBounds(character.gameObject, out Bounds characterBounds)) { return true; }
            if (hazardBounds.max.x >= characterBounds.min.x) { return true; }
        }

        return false;
    }

    private bool TryGetDoubleLayout(int layout, float minY, float maxY, float clearance, float hazardHalfHeight, out float lowerY, out float upperY)
    {
        lowerY = 0f;
        upperY = 0f;

        float routeWidth = Mathf.Max(0.01f, minimumSafeRouteWidth);
        float attackDistance = hazardHalfHeight * 2f + Mathf.Max(0f, minimumAttackVisualGap);
        float worldHeight = maxY - minY;

        if (layout == 0)
        {
            // Los ataques quedan separados visualmente, pero cierran el paso del medio
            if (attackDistance >= clearance * 2f || worldHeight < clearance * 2f + attackDistance + routeWidth * 2f) { return false; }

            float middleY = (minY + maxY) * 0.5f;
            lowerY = middleY - attackDistance * 0.5f;
            upperY = middleY + attackDistance * 0.5f;
            return true;
        }

        // Se deja un hueco medido para el personaje y se cierra uno de los extremos
        if (worldHeight < clearance * 3f + routeWidth * 2f || clearance * 2f + routeWidth < attackDistance) { return false; }

        if (layout == 1)
        {
            lowerY = minY;
            upperY = minY + clearance * 2f + routeWidth;
        }
        else
        {
            upperY = maxY;
            lowerY = maxY - clearance * 2f - routeWidth;
        }

        return true;
    }

    private bool TryGetWorldGeometry(bool isUpperWorld, out float minY, out float maxY, out float clearance, out float hazardHalfHeight)
    {
        minY = 0f;
        maxY = 0f;
        clearance = 0f;
        hazardHalfHeight = 0f;

        Transform character = isUpperWorld ? upperCharacter : lowerCharacter;

        if (gameplayBounds == null || !gameplayBounds.IsConfigured || character == null || hazardPrefab == null || !VisibleBounds2D.TryGetBounds(character.gameObject, out Bounds characterBounds) || !VisibleBounds2D.TryGetBounds(hazardPrefab, out Bounds hazardBounds))
        {
            WarnAboutGeometry();
            return false;
        }

        minY = isUpperWorld ? gameplayBounds.BatMinimumY : gameplayBounds.MermaidMinimumY;
        maxY = isUpperWorld ? gameplayBounds.BatMaximumY : gameplayBounds.MermaidMaximumY;

        float characterHalfHeight = Mathf.Max(characterBounds.max.y - character.position.y, character.position.y - characterBounds.min.y);
        hazardHalfHeight = Mathf.Max(hazardBounds.max.y - hazardPrefab.transform.position.y, hazardPrefab.transform.position.y - hazardBounds.min.y);
        clearance = characterHalfHeight + hazardHalfHeight + Mathf.Max(0f, characterSafetyMargin);
        return true;
    }

    private void WarnAboutGeometry()
    {
        if (hasWarnedAboutGeometry) { return; }

        hasWarnedAboutGeometry = true;
        Debug.LogWarning("No hay suficiente espacio o faltan referencias para algunos patrones del nivel 2", this);
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

        if (PatternCompleted != null)
        {
            PatternCompleted(completedPatternCount);
        }
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
