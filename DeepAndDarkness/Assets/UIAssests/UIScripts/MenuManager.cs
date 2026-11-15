using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    // UI Elements
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject creditsPanel;

    [SerializeField] private float fadeDuration = 0.2f; // Duration for fade in/out animations

    private void Start()
    {
        ShowMainMenu();
    }

    // Methods to show different panels
    public void ShowMainMenu() => StartCoroutine(ShowPanelWithFade(mainMenuPanel));
    public void ShowStoryPanel() => StartCoroutine(ShowPanelWithFade(storyPanel));
    public void ShowControlsPanel() => StartCoroutine(ShowPanelWithFade(controlsPanel));
    public void ShowSoundPanel() => StartCoroutine(ShowPanelWithFade(soundPanel));
    public void ShowCreditsPanel() => StartCoroutine(ShowPanelWithFade(creditsPanel));

    private IEnumerator ShowPanelWithFade(GameObject panelToShow)
    {
        GameObject[] panels = {mainMenuPanel, storyPanel, controlsPanel, soundPanel, creditsPanel };

        foreach (GameObject panel in panels)
            panel.SetActive(panel == panelToShow);
        CanvasGroup group = panelToShow.GetComponent<CanvasGroup>();
        group.alpha = 0f;


        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        group.alpha = 1f; // Ensure the panel is fully visible at the end of the fade
    }

    public void StartGame()
    {
        // Load the next scene (assuming the next scene is at index 1)
        SceneManager.LoadScene(1);
    }
}
