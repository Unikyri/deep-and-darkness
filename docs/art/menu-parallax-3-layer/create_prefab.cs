using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

const string prefabPath = "Assets/_Project/Art/UI/MenuParallax/Parallax3/MenuParallax3.prefab";
if (File.Exists(prefabPath) || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
    throw new InvalidOperationException("The prefab already exists; refusing to overwrite it.");

string[] names = { "Far", "Middle", "Foreground" };
string[] sprites = {
    "Assets/_Project/Art/UI/MenuParallax/Parallax3/01-Far.png",
    "Assets/_Project/Art/UI/MenuParallax/Parallax3/02-Middle.png",
    "Assets/_Project/Art/UI/MenuParallax/Parallax3/03-Foreground.png"
};
float[] amplitudes = { 0.4f, 3f, 6f };
float[] speeds = { 0.18f, 0.35f, 0.5f };

var previewScene = EditorSceneManager.NewPreviewScene();
string report;
try
{
    var root = new GameObject("MenuParallax3", typeof(RectTransform));
    root.SetActive(false);
    SceneManager.MoveGameObjectToScene(root, previewScene);
    var rootRect = (RectTransform)root.transform;
    rootRect.anchorMin = Vector2.zero;
    rootRect.anchorMax = Vector2.one;
    rootRect.offsetMin = Vector2.zero;
    rootRect.offsetMax = Vector2.zero;

    var rects = new RectTransform[3];
    for (int i = 0; i < rects.Length; i++)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprites[i]);
        if (sprite == null || sprite.texture.width != 3840 || sprite.texture.height != 2160)
            throw new InvalidOperationException("Missing or unexpected 4K sprite: " + sprites[i]);
        var child = new GameObject(names[i], typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        child.transform.SetParent(root.transform, false);
        var rect = (RectTransform)child.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var graphic = child.GetComponent<UnityEngine.UI.Image>();
        graphic.sprite = sprite;
        graphic.preserveAspect = false;
        graphic.raycastTarget = false;
        rects[i] = rect;
    }

    var animation = root.AddComponent<MenuParallaxAnimation>();
    var serialized = new SerializedObject(animation);
    var layers = serialized.FindProperty("layers");
    if (layers == null) throw new InvalidOperationException("Expected parallax configuration is missing.");
    layers.arraySize = 3;
    for (int i = 0; i < layers.arraySize; i++)
    {
        var layer = layers.GetArrayElementAtIndex(i);
        layer.FindPropertyRelative("layer").objectReferenceValue = rects[i];
        layer.FindPropertyRelative("amplitude").floatValue = amplitudes[i];
        layer.FindPropertyRelative("speed").floatValue = speeds[i];
    }
    serialized.ApplyModifiedPropertiesWithoutUndo();
    root.SetActive(true);
    bool success;
    var saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out success);
    if (!success || saved == null) throw new InvalidOperationException("Could not save the prefab.");
    report = "Created " + prefabPath + " with three 4K layers and configured movement.";
}
finally
{
    EditorSceneManager.ClosePreviewScene(previewScene);
}
report
