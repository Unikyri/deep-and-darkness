using System;
using System.Collections;
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

    [Header("Invulnerabilidad")]
    [Tooltip("Duración en segundos de la invulnerabilidad después de recibir daño; durante ese tiempo no recibe daño ni recoge coleccionables.")]
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 5f;

    private int currentLives;
    private bool isInvulnerable;

    public int CurrentLives
    {
        get { return currentLives; }
    }

    public int StartingLives
    {
        get { return startingLives; }
    }

    public bool IsInvulnerable
    {
        get { return isInvulnerable; }
    }

    public event Action<int, int> LivesChanged;

    private void Awake()
    {
        currentLives = Mathf.Max(1, startingLives);
        if (LivesChanged != null) { LivesChanged(currentLives, StartingLives); }
    }

    public bool TakeDamage()
    {
        if (gameSession == null || gameSession.IsGameOver || currentLives <= 0 || isInvulnerable) { return false; }

        currentLives = Mathf.Max(0, currentLives - 1);

        if (LivesChanged != null) { LivesChanged(currentLives, StartingLives); }

        Debug.Log($"{characterName} recibió un impacto. Vidas restantes: {currentLives}.", this);

        if (currentLives == 0)
        {
            gameSession.EndGame(characterName);
            return true;
        }

        isInvulnerable = true;
        StartCoroutine(EndInvulnerabilityAfterDelay());
        Debug.Log($"{characterName} es invulnerable durante {invulnerabilityDuration} segundos.", this);

        return true;
    }

    private IEnumerator EndInvulnerabilityAfterDelay()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
        Debug.Log($"{characterName} volvió a ser vulnerable.", this);
    }
}
