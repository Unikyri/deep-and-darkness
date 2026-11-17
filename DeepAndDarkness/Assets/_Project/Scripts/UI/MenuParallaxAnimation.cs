using UnityEngine;

public class MenuParallaxAnimation : MonoBehaviour
{
    [System.Serializable]
    public class LayerAnimation
    {
        public RectTransform layer;
        public float amplitude = 10f;
        public float speed = 1f;

        [HideInInspector]
        public Vector2 startPosition;
    }

    [SerializeField] private LayerAnimation[] layers;

    private void Start()
    {
        foreach (LayerAnimation layer in layers)
        {
            if (layer.layer != null)
            {
                layer.startPosition = layer.layer.anchoredPosition;
            }
        }
    }

    private void Update()
    {
        foreach (LayerAnimation layer in layers)
        {
            if (layer.layer != null)
            {
                float movement = Mathf.Sin(Time.time * layer.speed) * layer.amplitude;

                layer.layer.anchoredPosition = layer.startPosition + new Vector2(0f, movement);
            }
        }
    }
}

