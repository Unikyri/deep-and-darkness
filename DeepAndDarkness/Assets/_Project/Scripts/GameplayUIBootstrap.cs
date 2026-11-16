using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIBootstrap : MonoBehaviour
{
    [Tooltip("Nombre de la escena que contiene la interfaz de pausa y el HUD")]
    [SerializeField] private string uiSceneName = "PausaScene";

    private void Start()
    {
        Scene uiScene = SceneManager.GetSceneByName(uiSceneName);
        if (uiScene.IsValid() && uiScene.isLoaded) { return; }

        SceneManager.LoadSceneAsync(uiSceneName, LoadSceneMode.Additive);
    }
}
