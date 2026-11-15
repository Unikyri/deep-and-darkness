using UnityEngine;

public class CollectiblePickup : MonoBehaviour
{
    private GameSessionManager gameSession;
    private int collectibleNumber;
    private bool hasBeenCollected;

    public void Initialize(GameSessionManager session, int number)
    {
        gameSession = session;
        collectibleNumber = number;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected || gameSession == null || gameSession.IsGameOver) { return; }

        CharacterLives characterLives = other.GetComponentInParent<CharacterLives>();

        if (characterLives == null || characterLives.IsInvulnerable) { return; }
        if (!gameSession.RegisterCollectible(collectibleNumber)) { return; }

        hasBeenCollected = true;
        Destroy(gameObject);
    }
}