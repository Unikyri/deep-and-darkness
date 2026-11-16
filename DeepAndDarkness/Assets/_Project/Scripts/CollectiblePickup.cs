using UnityEngine;

public class CollectiblePickup : MonoBehaviour
{
    private GameSessionManager gameSession;
    private int collectibleNumber;
    private bool hasBeenCollected;

    public void Initialize(GameSessionManager session, int number)
    {
        if (gameSession != null) { gameSession.IntangibilityEnded -= HandleIntangibilityEnded; }

        gameSession = session;
        collectibleNumber = number;

        if (gameSession != null) { gameSession.IntangibilityEnded += HandleIntangibilityEnded; }
    }

    private void OnDestroy()
    {
        if (gameSession != null) { gameSession.IntangibilityEnded -= HandleIntangibilityEnded; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void HandleIntangibilityEnded()
    {
        if (hasBeenCollected || gameSession == null || gameSession.IsGameOver) { return; }

        Collider2D collectibleCollider = GetComponent<Collider2D>();
        if (collectibleCollider == null || !collectibleCollider.enabled) { return; }

        // Si ambos colliders quedaron dormidos, comprobamos si siguen tocándose
        foreach (CharacterLives character in FindObjectsByType<CharacterLives>(FindObjectsSortMode.None))
        {
            foreach (Collider2D characterCollider in character.GetComponentsInChildren<Collider2D>())
            {
                if (!Physics2D.IsTouching(collectibleCollider, characterCollider)) { continue; }

                TryCollect(characterCollider);
                if (hasBeenCollected) { return; }
            }
        }
    }

    private void TryCollect(Collider2D other)
    {
        if (hasBeenCollected || gameSession == null || gameSession.IsGameOver) { return; }

        CharacterLives characterLives = other.GetComponentInParent<CharacterLives>();

        if (characterLives == null || characterLives.IsIntangible) { return; }
        if (!gameSession.RegisterCollectible(collectibleNumber)) { return; }

        hasBeenCollected = true;
        Destroy(gameObject);
    }
}
