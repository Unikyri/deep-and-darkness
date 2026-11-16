using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // UI Elements
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject creditsPanel;

    // Duration for fade in/out animations
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float clickDelay = 0.12f;
    private Coroutine panelTransition;

    private void Start()
    {
        // Ensure the main menu is shown at the start
        ShowMainMenu();
    }

    // Methods to show different panels
    public void ShowMainMenu() => ShowPanel(mainMenuPanel);
    public void ShowStoryPanel() => ShowPanel(storyPanel);
    public void ShowControlsPanel() => ShowPanel(controlsPanel);
    public void ShowSoundPanel() => ShowPanel(soundPanel);
    public void ShowCreditsPanel() => ShowPanel(creditsPanel);

    private void ShowPanel(GameObject panelToShow)
    {
        if (panelTransition != null)
            StopCoroutine(panelTransition);

        panelTransition = StartCoroutine(ShowPanelWithFade(panelToShow));
    }


    // Coroutine to handle panel transitions with fade effect
    private IEnumerator ShowPanelWithFade(GameObject panelToShow)
    {
        // Wait for a short delay
        yield return new WaitForSecondsRealtime(clickDelay);

        EventSystem.current?.SetSelectedGameObject(null);

        // List of all panels to manage their visibility
        GameObject[] panels = {mainMenuPanel, storyPanel, controlsPanel, soundPanel, creditsPanel };

        // Activate the target panel and deactivate others
        foreach (GameObject panel in panels)
            panel.SetActive(panel == panelToShow);

        // Get the CanvasGroup component for fade effect
        CanvasGroup group = panelToShow.GetComponent<CanvasGroup>();

        // Panel starts fully transparent for fade-in effect
        group.alpha = 0f;

        // Fade in the panel over the specified duration
        float elapsedTime = 0f;

        // Repeat until the elapsed time reaches the fade duration
        while (elapsedTime < fadeDuration)
        {
            // Increment elapsed time since the last frame
            elapsedTime += Time.unscaledDeltaTime;
            // Transform the fade to a value between 0 and 1 based on elapsed time
            group.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            // Wait for the next frame to update the fade effect
            yield return null;
        }

        // Ensure the panel is fully visible at the end of the fade
        group.alpha = 1f;

        panelTransition = null;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
