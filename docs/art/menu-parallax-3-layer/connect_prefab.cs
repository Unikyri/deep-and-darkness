using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

const string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
const string expectedSceneHash = "ac18d78a2cc92c4f5d02ed3ff32ef9870e78307c2e10fdd35ea0628457cfcbcc";
const string prefabPath = "Assets/_Project/Art/UI/MenuParallax/Parallax3/MenuParallax3.prefab";
const string expectedBackground = "Assets/_Project/Art/UI/MenuParallax/Background 1.jpg";

if (EditorApplication.isPlayingOrWillChangePlaymode)
    throw new InvalidOperationException("Stop Play mode before changing the saved scene.");
var scene = SceneManager.GetActiveScene();
if (!scene.isLoaded || scene.path != scenePath || scene.isDirty)
    throw new InvalidOperationException("Open the saved MainMenu scene with no unsaved changes.");
using (var sha = SHA256.Create())
{
    var actual = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(scenePath)))
        .Replace("-", "").ToLowerInvariant();
    if (actual != expectedSceneHash)
        throw new InvalidOperationException("MainMenu changed on disk; inspect it again before applying.");
}

var panel = GameObject.Find("MainMenuPanel");
var background = panel == null ? null : panel.transform.Find("Background");
if (background == null) throw new InvalidOperationException("MainMenuPanel/Background was not found.");
var originalImage = background.GetComponent<UnityEngine.UI.Image>();
if (originalImage == null || originalImage.sprite == null ||
    AssetDatabase.GetAssetPath(originalImage.sprite) != expectedBackground)
    throw new InvalidOperationException("The existing background is not the expected source image.");
if (background.Find("MenuParallax3") != null)
    throw new InvalidOperationException("A MenuParallax3 instance already exists.");
var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
if (prefab == null || prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true).Length != 3)
    throw new InvalidOperationException("The expected three-layer prefab is missing.");

Undo.IncrementCurrentGroup();
int group = Undo.GetCurrentGroup();
Undo.SetCurrentGroupName("Add three-layer menu parallax");
try
{
    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
    if (instance == null) throw new InvalidOperationException("Prefab instantiation failed.");
    Undo.RegisterCreatedObjectUndo(instance, "Add three-layer menu parallax");
    Undo.SetTransformParent(instance.transform, background, "Parent menu parallax");
    instance.transform.SetAsFirstSibling();
    var rect = (RectTransform)instance.transform;
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
    EditorSceneManager.MarkSceneDirty(scene);
    if (!EditorSceneManager.SaveScene(scene))
        throw new InvalidOperationException("Could not save MainMenu; undoing the instance.");
    Undo.CollapseUndoOperations(group);
}
catch
{
    Undo.RevertAllDownToGroup(group);
    throw;
}
"Connected the three-layer prefab as the first child of MainMenuPanel/Background and saved MainMenu."
