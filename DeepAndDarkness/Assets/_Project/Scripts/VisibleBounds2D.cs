using UnityEngine;

public static class VisibleBounds2D
{
    public static bool TryGetBounds(GameObject target, out Bounds bounds)
    {
        bounds = new Bounds();
        if (target == null) { return false; }

        bool hasBounds = false;

        foreach (SpriteRenderer spriteRenderer in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!spriteRenderer.enabled || !IsActiveWithinTarget(spriteRenderer.transform, target.transform) || spriteRenderer.sprite == null) { continue; }

            Bounds spriteBounds = spriteRenderer.bounds;
            if (spriteBounds.size.x <= 0f && spriteBounds.size.y <= 0f) { continue; }

            AddBounds(ref bounds, spriteBounds, ref hasBounds);
        }

        foreach (Collider2D collider in target.GetComponentsInChildren<Collider2D>(true))
        {
            if (!collider.enabled || !IsActiveWithinTarget(collider.transform, target.transform)) { continue; }

            Bounds colliderBounds = collider.bounds;

            // Los colliders de un prefab sin instanciar pueden tener bounds vacíos
            if (collider is BoxCollider2D box && colliderBounds.size.x <= 0f && colliderBounds.size.y <= 0f)
            {
                Vector3 center = box.transform.TransformPoint(box.offset);
                Vector3 horizontal = box.transform.TransformVector(new Vector3(box.size.x * 0.5f, 0f, 0f));
                Vector3 vertical = box.transform.TransformVector(new Vector3(0f, box.size.y * 0.5f, 0f));
                Vector3 size = new Vector3(2f * (Mathf.Abs(horizontal.x) + Mathf.Abs(vertical.x)), 2f * (Mathf.Abs(horizontal.y) + Mathf.Abs(vertical.y)), 0f);
                colliderBounds = new Bounds(center, size);
            }

            if (colliderBounds.size.x <= 0f && colliderBounds.size.y <= 0f) { continue; }

            AddBounds(ref bounds, colliderBounds, ref hasBounds);
        }

        return hasBounds;
    }

    private static bool IsActiveWithinTarget(Transform current, Transform target)
    {
        while (current != null)
        {
            if (!current.gameObject.activeSelf) { return false; }
            if (current == target) { return true; }
            current = current.parent;
        }

        return false;
    }

    private static void AddBounds(ref Bounds bounds, Bounds additionalBounds, ref bool hasBounds)
    {
        if (!hasBounds)
        {
            bounds = additionalBounds;
            hasBounds = true;
            return;
        }

        bounds.Encapsulate(additionalBounds);
    }
}
