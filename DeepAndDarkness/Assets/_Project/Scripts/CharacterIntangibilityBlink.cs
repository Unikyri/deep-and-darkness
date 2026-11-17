using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterIntangibilityBlink : MonoBehaviour
{
    [Header("Parpadeo")]
    [Tooltip("Tiempo en segundos entre cada cambio de visibilidad mientras el personaje es intangible.")]
    [SerializeField, Min(0.02f)] private float blinkInterval = 0.12f;
    [Tooltip("Opacidad del sprite en la fase tenue del parpadeo.")]
    [SerializeField, Range(0f, 1f)] private float dimmedAlpha = 0.2f;

    private CharacterLives characterLives;
    private SpriteRenderer spriteRenderer;
    private float originalAlpha;
    private float nextBlinkTime;
    private bool isBlinking;
    private bool isDimmed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        characterLives = GetComponentInParent<CharacterLives>();

        if (characterLives == null)
        {
            Debug.LogError("CharacterIntangibilityBlink necesita un CharacterLives en un objeto padre.", this);
            enabled = false;
            return;
        }

        originalAlpha = spriteRenderer.color.a;
    }

    private void Update()
    {
        if (!characterLives.IsIntangible)
        {
            if (isBlinking) { RestoreOriginalAlpha(); }
            return;
        }

        isBlinking = true;
        if (Time.time < nextBlinkTime) { return; }

        nextBlinkTime = Time.time + blinkInterval;
        isDimmed = !isDimmed;

        Color spriteColor = spriteRenderer.color;
        spriteColor.a = originalAlpha * (isDimmed ? dimmedAlpha : 1f);
        spriteRenderer.color = spriteColor;
    }

    private void OnDisable()
    {
        if (spriteRenderer != null) { RestoreOriginalAlpha(); }
    }

    private void RestoreOriginalAlpha()
    {
        Color spriteColor = spriteRenderer.color;
        spriteColor.a = originalAlpha;
        spriteRenderer.color = spriteColor;

        nextBlinkTime = 0f;
        isBlinking = false;
        isDimmed = false;
    }
}
