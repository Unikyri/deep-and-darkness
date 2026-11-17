using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("SpriteRenderer del mundo superior. El script repetirá su sprite en ambas direcciones para cubrir la cámara.")]
    [SerializeField] private SpriteRenderer upperWorldRenderer;

    [Tooltip("SpriteRenderer del mundo inferior. El script repetirá su sprite en ambas direcciones para cubrir la cámara.")]
    [SerializeField] private SpriteRenderer lowerWorldRenderer;

    [Tooltip("Cámara usada para detectar cuándo una franja salió completamente de la pantalla.")]
    [SerializeField] private Camera mainCamera;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento de esta capa hacia la izquierda, en unidades por segundo.")]
    [SerializeField, Min(0f)] private float scrollSpeed = 1f;

    [Tooltip("Voltea horizontalmente las copias alternas para que los bordes de los sprites empalmen al repetirse.")]
    [SerializeField] private bool mirrorAlternateTiles = true;

    private List<SpriteRenderer> upperWorldTiles;
    private List<SpriteRenderer> lowerWorldTiles;

    private void Start()
    {
        if (mainCamera == null) { mainCamera = Camera.main; }

        if (mainCamera == null)
        {
            Debug.LogError("ParallaxLayer necesita una cámara asignada o una cámara con el tag MainCamera.", this);
            enabled = false;
            return;
        }

        upperWorldTiles = CreateLoopTiles(upperWorldRenderer);
        lowerWorldTiles = CreateLoopTiles(lowerWorldRenderer);
    }

    private void Update()
    {
        MoveAndLoop(upperWorldRenderer, upperWorldTiles);
        MoveAndLoop(lowerWorldRenderer, lowerWorldTiles);
    }

    private List<SpriteRenderer> CreateLoopTiles(SpriteRenderer sourceRenderer)
    {
        List<SpriteRenderer> tiles = new List<SpriteRenderer>();
        if (sourceRenderer == null || sourceRenderer.sprite == null) { return tiles; }

        tiles.Add(sourceRenderer);
        EnsureViewportCoverage(sourceRenderer, tiles);
        return tiles;
    }

    private void EnsureViewportCoverage(SpriteRenderer sourceRenderer, List<SpriteRenderer> tiles)
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null || tiles == null || tiles.Count == 0) { return; }

        float tileWidth = sourceRenderer.bounds.size.x;
        if (tileWidth <= 0f) { return; }

        GetCameraHorizontalBounds(sourceRenderer, out float cameraLeftEdge, out float cameraRightEdge);
        const float coverageMargin = 0.01f;

        while (tiles[0].bounds.min.x > cameraLeftEdge - coverageMargin)
        {
            SpriteRenderer leftmostTile = tiles[0];
            SpriteRenderer leftCopy = CreateLoopCopy(sourceRenderer, leftmostTile, -tileWidth, tiles.Count);
            if (leftCopy == null) { break; }
            tiles.Insert(0, leftCopy);
        }

        while (tiles.Count < 2 || tiles[tiles.Count - 1].bounds.max.x < cameraRightEdge + coverageMargin)
        {
            SpriteRenderer rightmostTile = tiles[tiles.Count - 1];
            SpriteRenderer rightCopy = CreateLoopCopy(sourceRenderer, rightmostTile, tileWidth, tiles.Count);
            if (rightCopy == null) { break; }
            tiles.Add(rightCopy);
        }
    }

    private SpriteRenderer CreateLoopCopy(SpriteRenderer sourceRenderer, SpriteRenderer neighbor, float horizontalOffset, int copyIndex)
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null || neighbor == null) { return null; }

        GameObject copyObject = Instantiate(sourceRenderer.gameObject, sourceRenderer.transform.parent);
        copyObject.name = sourceRenderer.gameObject.name + "_Loop_" + copyIndex;
        SpriteRenderer copyRenderer = copyObject.GetComponent<SpriteRenderer>();
        copyRenderer.flipX = mirrorAlternateTiles ? !neighbor.flipX : sourceRenderer.flipX;
        PositionTileAdjacent(copyRenderer, neighbor, horizontalOffset > 0f);
        return copyRenderer;
    }

    private void MoveAndLoop(SpriteRenderer sourceRenderer, List<SpriteRenderer> tiles)
    {
        if (sourceRenderer == null || tiles == null || tiles.Count == 0) { return; }

        Vector3 movement = Vector3.left * scrollSpeed * Time.deltaTime;
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].transform.position += movement;
        }

        GetCameraHorizontalBounds(sourceRenderer, out float cameraLeftEdge, out _);

        while (tiles.Count > 1 && tiles[0].bounds.max.x <= cameraLeftEdge)
        {
            SpriteRenderer leftmostTile = tiles[0];
            SpriteRenderer rightmostTile = tiles[tiles.Count - 1];
            tiles.RemoveAt(0);
            if (mirrorAlternateTiles) { leftmostTile.flipX = !rightmostTile.flipX; }
            PositionTileAdjacent(leftmostTile, rightmostTile, true);
            tiles.Add(leftmostTile);
        }

        EnsureViewportCoverage(sourceRenderer, tiles);
    }

    private void GetCameraHorizontalBounds(SpriteRenderer sourceRenderer, out float leftEdge, out float rightEdge)
    {
        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - sourceRenderer.bounds.center.z);
        leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, cameraDepth)).x;
        rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, cameraDepth)).x;
    }

    private void PositionTileAdjacent(SpriteRenderer tile, SpriteRenderer neighbor, bool placeOnRight)
    {
        float targetMinX = placeOnRight ? neighbor.bounds.max.x : neighbor.bounds.min.x - tile.bounds.size.x;
        tile.transform.position += Vector3.right * (targetMinX - tile.bounds.min.x);
    }
}
