using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    [Header("Victoria")]
    [Tooltip("Cantidad de coleccionables de misión que hay que recoger para completar el nivel")]
    [SerializeField, Min(1)] private int requiredCollectibles = 3;

    [Tooltip("Nombre de la siguiente escena. Déjalo vacío si este es el último nivel")]
    [SerializeField] private string nextSceneName;

    [Header("Intangibilidad compartida")]
    [Tooltip("Tiempo durante el cual ambos personajes atraviesan ataques y no pueden recoger objetos después de un golpe")]
    [SerializeField, Min(0f)] private float intangibilityDuration = 5f;

    private bool isGameOver;
    private bool charactersIntangible;

    private int collectedCollectibles;

    public bool IsGameOver
    {
        get { return isGameOver; }
    }

    public int CollectedCollectibles
    {
        get { return collectedCollectibles; }
    }

    public int RequiredCollectibles
    {
        get { return requiredCollectibles; }
    }

    public bool CharactersIntangible
    {
        get { return charactersIntangible; }
    }

    public event Action<int, int> CollectiblesChanged;
    public event Action<string> GameOverTriggered;
    public event Action VictoryTriggered;
    public event Action IntangibilityEnded;

    private void Awake()
    {
        // Restauramos el tiempo al entrar al nivel
        Time.timeScale = 1f;
    }

    public bool TryActivateIntangibility()
    {
        if (isGameOver || charactersIntangible) { return false; }

        charactersIntangible = true;
        StartCoroutine(EndIntangibilityAfterDelay());
        return true;
    }

    private IEnumerator EndIntangibilityAfterDelay()
    {
        yield return new WaitForSeconds(intangibilityDuration);
        charactersIntangible = false;

        if (IntangibilityEnded != null)
        {
            IntangibilityEnded();
        }
    }

    public void EndGame(string defeatedCharacterName)
    {
        // Evita ejecutar la derrota más de una vez
        if (isGameOver) { return; }

        isGameOver = true;
        Time.timeScale = 0f;


        if (GameOverTriggered != null)
        {
            GameOverTriggered(defeatedCharacterName);
        }
    }

    public bool RegisterCollectible(int collectibleNumber)
    {
        // Sirve para comprobar que sí se puede recolectar otro objeto mientras la partida esté activa
        if (isGameOver || collectibleNumber != collectedCollectibles + 1)
        {
            return false;
        }

        collectedCollectibles++;


        // Es para actualizar la cantidad de objetos recogidos
        if (CollectiblesChanged != null)
        {
            CollectiblesChanged(collectedCollectibles, requiredCollectibles);
        }

        if (collectedCollectibles >= requiredCollectibles)
        {
            FinishVictory();
        }

        return true;
    }

    private void FinishVictory()
    {
        // Evita que quede en bucle durante un solo nivel
        if (isGameOver) { return; }

        isGameOver = true;

        // Esto sirve para comprobar si hay otro nivel (escena) listo
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            return;
        }

        // Si no existe otra escena (o nivel) entonces esta es la victoria final
        Time.timeScale = 0f;

        Debug.Log($"Victoria: se recogieron {requiredCollectibles} coleccionables.");

        if (VictoryTriggered != null)
        {
            VictoryTriggered();
        }
    }
}
