using UnityEngine;

public class GameplayBounds : MonoBehaviour
{
    public enum WorldSide { Upper, Lower }

    [Header("Límites del mundo superior")]
    [Tooltip("Transform que marca el límite interior del recorrido del vampiro; también sirve para calcular carriles y su zona de reposo.")]
    [SerializeField] private Transform batInnerLimit;
    [Tooltip("Transform que marca el límite exterior del recorrido del vampiro y sirve para calcular sus carriles.")]
    [SerializeField] private Transform batOuterLimit;

    [Header("Límites del mundo inferior")]
    [Tooltip("Transform que marca el límite interior del recorrido de la sirena; también sirve para calcular carriles y su zona de reposo.")]
    [SerializeField] private Transform mermaidInnerLimit;
    [Tooltip("Transform que marca el límite exterior del recorrido de la sirena y sirve para calcular sus carriles.")]
    [SerializeField] private Transform mermaidOuterLimit;

    public bool IsConfigured
    {
        get
        {
            return batInnerLimit != null
                && batOuterLimit != null
                && mermaidInnerLimit != null
                && mermaidOuterLimit != null;
        }
    }

    public float BatInnerY
    {
        get { return batInnerLimit.position.y; }
    }

    public float BatOuterY
    {
        get { return batOuterLimit.position.y; }
    }

    public float MermaidInnerY
    {
        get { return mermaidInnerLimit.position.y; }
    }

    public float MermaidOuterY
    {
        get { return mermaidOuterLimit.position.y; }
    }

    public float BatMinimumY
    {
        get { return Mathf.Min(BatInnerY, BatOuterY); }
    }

    public float BatMaximumY
    {
        get { return Mathf.Max(BatInnerY, BatOuterY); }
    }

    public float MermaidMinimumY
    {
        get { return Mathf.Min(MermaidInnerY, MermaidOuterY); }
    }

    public float MermaidMaximumY
    {
        get { return Mathf.Max(MermaidInnerY, MermaidOuterY); }
    }

    public float GetBatLaneY(float laneRatio)
    {
        float ratio = Mathf.Clamp01(laneRatio);
        return Mathf.Lerp(BatInnerY, BatOuterY, ratio);
    }

    public float GetMermaidLaneY(float laneRatio)
    {
        float ratio = Mathf.Clamp01(laneRatio);
        return Mathf.Lerp(MermaidInnerY, MermaidOuterY, ratio);
    }

    public float GetInnerLimitY(WorldSide side)
    {
        if (side == WorldSide.Upper)
        {
            return BatInnerY;
        }

        return MermaidInnerY;
    }
}
