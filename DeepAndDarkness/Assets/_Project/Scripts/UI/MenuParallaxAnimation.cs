using UnityEngine;

// Controls the vertical movement of the menu background layers
public class MenuParallaxAnimation : MonoBehaviour
{
    // Stores the animation settings for each background layer
    [System.Serializable]
    public class LayerAnimation
    {
        public RectTransform layer;
        // Controls how far the layer moves from its starting position
        public float amplitude = 10f;
        public float speed = 1f;

        // Stores the original position of the layer
        [HideInInspector]
        public Vector2 startPosition;
    }

    // List of background layers to animate
    [SerializeField] private LayerAnimation[] layers;

    private void Start()
    {
        // Saves the initial position of each assigned layer
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
        // Updates the vertical movement of each background layer
        foreach (LayerAnimation layer in layers)
        {
            if (layer.layer != null)
            {
                // Calculates a smooth oscillating movement using a sine wave
                float movement = Mathf.Sin(Time.time * layer.speed) * layer.amplitude;

                // Applies the movement while keeping the original horizontal position
                layer.layer.anchoredPosition = layer.startPosition + new Vector2(0f, movement);
            }
        }
    }
}

