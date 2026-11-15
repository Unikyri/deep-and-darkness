using System;
using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    [Header("Victoria")]
    [Tooltip("Cantidad de coleccionables de misión que hay que recoger para activar la victoria.")]
    [SerializeField, Min(1)] private int requiredCollectibles = 3;

    private bool isGameOver;
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

    public event Action<int, int> CollectiblesChanged;
    public event Action<string> GameOverTriggered;
    public event Action VictoryTriggered;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    public void EndGame(string defeatedCharacterName)
    {
        if (isGameOver) { return; }

        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log($"Derrota: {defeatedCharacterName} se quedó sin vidas.");

        if (GameOverTriggered != null) { GameOverTriggered(defeatedCharacterName); }
    }

    public bool RegisterCollectible(int collectibleNumber)
    {
        if (isGameOver || collectibleNumber != collectedCollectibles + 1) { return false; }

        collectedCollectibles++;
        Debug.Log($"Coleccionable {collectedCollectibles}/{requiredCollectibles} recogido.");

        if (CollectiblesChanged != null) { CollectiblesChanged(collectedCollectibles, requiredCollectibles); }

        if (collectedCollectibles >= requiredCollectibles) { FinishVictory(); }

        return true;
    }

    private void FinishVictory()
    {
        if (isGameOver) { return; }

        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("Victoria: se recogieron los tres coleccionables.");

        if (VictoryTriggered != null) { VictoryTriggered(); }
    }
}
