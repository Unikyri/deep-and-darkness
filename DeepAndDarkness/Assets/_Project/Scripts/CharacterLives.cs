using System;
using UnityEngine;

public class CharacterLives : MonoBehaviour
{
    [Header("Vidas")]
    [Tooltip("Cantidad de vidas con la que empieza este personaje. Si llega a cero, se activa la derrota.")]
    [SerializeField, Min(1)] private int startingLives = 3;
    [Tooltip("Nombre usado para identificar a este personaje en los mensajes y en la condición de derrota.")]
    [SerializeField] private string characterName = "Personaje";
    [Tooltip("Sesión que recibe el aviso si este personaje se queda sin vidas.")]
    [SerializeField] private GameSessionManager gameSession;

    private int currentLives;

    public int CurrentLives
    {
        get { return currentLives; }
    }

    public int StartingLives
    {
        get { return startingLives; }
    }

    public string CharacterName
    {
        get { return characterName; }
    }

    public bool IsIntangible
    {
        get { return gameSession != null && gameSession.CharactersIntangible; }
    }

    public event Action<int, int> LivesChanged;

    private void Awake()
    {
        currentLives = Mathf.Max(1, startingLives);
        if (LivesChanged != null) { LivesChanged(currentLives, StartingLives); }
    }

    public bool TakeDamage()
    {
        if (gameSession == null || currentLives <= 0 || !gameSession.TryActivateIntangibility()) { return false; }

        currentLives = Mathf.Max(0, currentLives - 1);

        if (LivesChanged != null) { LivesChanged(currentLives, StartingLives); }

        Debug.Log($"{characterName} recibió un impacto. Vidas restantes: {currentLives}.", this);

        if (currentLives == 0)
        {
            gameSession.EndGame(characterName);
            return true;
        }

        return true;
    }
}
