using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientVfxController : MonoBehaviour
{
    [Header("Efectos ambientales")]
    [Tooltip("Animaciones que pueden aparecer ocasionalmente en el mundo superior.")]
    [SerializeField] private Animator[] upperWorldEffects;
    [Tooltip("Animaciones que pueden aparecer ocasionalmente en el mundo inferior.")]
    [SerializeField] private Animator[] lowerWorldEffects;

    [Header("Frecuencia")]
    [Tooltip("Espera aleatoria antes del primer efecto de la partida.")]
    [SerializeField] private Vector2 initialDelayRange = new Vector2(2f, 6f);
    [Tooltip("Espera aleatoria entre un efecto y el siguiente.")]
    [SerializeField] private Vector2 intervalBetweenEffects = new Vector2(8f, 14f);

    [Header("Ubicación")]
    [Tooltip("Cámara usada para colocar cada efecto junto al borde derecho de la pantalla.")]
    [SerializeField] private Camera gameplayCamera;
    [Tooltip("Separación normalizada entre el efecto y el borde derecho de la cámara.")]
    [SerializeField, Range(0f, 0.2f)] private float horizontalViewportInset = 0.04f;
    [Tooltip("Rango vertical normalizado para colocar efectos en el mundo superior.")]
    [SerializeField] private Vector2 upperWorldViewportYRange = new Vector2(0.62f, 0.9f);
    [Tooltip("Rango vertical normalizado para colocar efectos en el mundo inferior.")]
    [SerializeField] private Vector2 lowerWorldViewportYRange = new Vector2(0.1f, 0.38f);

    private readonly List<Animator> effects = new List<Animator>();
    private Animator lastPlayedEffect;
    private Coroutine playbackRoutine;

    private void Awake()
    {
        AddEffects(upperWorldEffects);
        AddEffects(lowerWorldEffects);

        if (gameplayCamera == null) { gameplayCamera = Camera.main; }

        HideAllEffects();
    }

    private void OnEnable()
    {
        playbackRoutine = StartCoroutine(PlayEffectsPeriodically());
    }

    private void OnDisable()
    {
        if (playbackRoutine != null) { StopCoroutine(playbackRoutine); }
        HideAllEffects();
    }

    private IEnumerator PlayEffectsPeriodically()
    {
        if (effects.Count == 0)
        {
            Debug.LogWarning("AmbientVfxController no tiene efectos asignados.", this);
            yield break;
        }

        if (gameplayCamera == null)
        {
            Debug.LogError("AmbientVfxController necesita la cámara del nivel.", this);
            yield break;
        }

        yield return new WaitForSeconds(GetRandomDuration(initialDelayRange));

        while (true)
        {
            Animator effect = SelectNextEffect();
            bool isUpperWorld = IsUpperWorldEffect(effect);

            PositionEffectAtRightEdge(effect, isUpperWorld);
            PlayFromBeginning(effect);
            yield return new WaitForSeconds(GetPlaybackDuration(effect));
            HideEffect(effect);

            yield return new WaitForSeconds(GetRandomDuration(intervalBetweenEffects));
        }
    }

    private void AddEffects(Animator[] effectsToAdd)
    {
        if (effectsToAdd == null) { return; }

        foreach (Animator effect in effectsToAdd)
        {
            if (effect != null && !effects.Contains(effect)) { effects.Add(effect); }
        }
    }

    private Animator SelectNextEffect()
    {
        if (effects.Count == 1) { return effects[0]; }

        Animator selectedEffect = effects[Random.Range(0, effects.Count)];
        if (selectedEffect == lastPlayedEffect)
        {
            int currentIndex = effects.IndexOf(selectedEffect);
            selectedEffect = effects[(currentIndex + Random.Range(1, effects.Count)) % effects.Count];
        }

        lastPlayedEffect = selectedEffect;
        return selectedEffect;
    }

    private bool IsUpperWorldEffect(Animator effect)
    {
        return upperWorldEffects != null && System.Array.IndexOf(upperWorldEffects, effect) >= 0;
    }

    private void PositionEffectAtRightEdge(Animator effect, bool isUpperWorld)
    {
        Vector2 viewportYRange = isUpperWorld ? upperWorldViewportYRange : lowerWorldViewportYRange;
        float viewportY = Random.Range(Mathf.Min(viewportYRange.x, viewportYRange.y), Mathf.Max(viewportYRange.x, viewportYRange.y));
        float cameraDepth = Mathf.Abs(effect.transform.position.z - gameplayCamera.transform.position.z);
        Vector3 position = gameplayCamera.ViewportToWorldPoint(new Vector3(1f - horizontalViewportInset, viewportY, cameraDepth));
        position.z = effect.transform.position.z;
        effect.transform.position = position;
    }

    private void PlayFromBeginning(Animator effect)
    {
        effect.enabled = true;
        effect.Rebind();
        effect.Update(0f);

        AnimatorStateInfo state = effect.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash != 0)
        {
            effect.Play(state.fullPathHash, 0, 0f);
            effect.Update(0f);
        }

        foreach (SpriteRenderer spriteRenderer in effect.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spriteRenderer.enabled = true;
        }
    }

    private float GetPlaybackDuration(Animator effect)
    {
        RuntimeAnimatorController controller = effect.runtimeAnimatorController;
        if (controller == null || controller.animationClips.Length == 0) { return 1f; }

        float clipDuration = 0f;
        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip != null) { clipDuration = Mathf.Max(clipDuration, clip.length); }
        }

        return clipDuration / Mathf.Max(Mathf.Abs(effect.speed), 0.01f);
    }

    private float GetRandomDuration(Vector2 range)
    {
        return Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
    }

    private void HideAllEffects()
    {
        foreach (Animator effect in effects) { HideEffect(effect); }
    }

    private void HideEffect(Animator effect)
    {
        if (effect == null) { return; }

        foreach (SpriteRenderer spriteRenderer in effect.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spriteRenderer.enabled = false;
        }

        effect.enabled = false;
    }
}
