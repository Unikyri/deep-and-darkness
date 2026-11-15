using UnityEngine;

public class HazardDamage : MonoBehaviour
{
    private bool hasAppliedDamage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasAppliedDamage) { return; }

        CharacterLives targetLives = other.GetComponentInParent<CharacterLives>();

        if (targetLives == null) { return; }
        if (!targetLives.TakeDamage()) { return; }

        hasAppliedDamage = true;
        Destroy(gameObject);
    }
}