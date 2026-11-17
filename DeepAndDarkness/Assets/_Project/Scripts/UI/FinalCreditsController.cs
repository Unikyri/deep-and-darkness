using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalCreditsController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup storyPanel;
    [SerializeField] private CanvasGroup creditsPanel;
    [SerializeField] private CanvasGroup fadeToBlack;
    [SerializeField] private CanvasGroup finalOptionsPanel;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private float creditsDuration = 3f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isTransitioning = false;

    // Keeps story panel hidden temporary
    private void Start()
    {
        storyPanel.alpha = 0f;
        storyPanel.interactable = false;
        storyPanel.blocksRaycasts = false;

        // Keeps credits panel hidden
        creditsPanel.alpha = 0f;
        creditsPanel.interactable = false;
        creditsPanel.blocksRaycasts = false;

        finalOptionsPanel.alpha = 0f;
        finalOptionsPanel.interactable = false;
        finalOptionsPanel.blocksRaycasts = false;

        // Shows the balck panel
        fadeToBlack.alpha = 1f;

        // Starts transition
        StartCoroutine(StoryFadeIn());
    }

    // Shows story panel and hiddens black screen
    private IEnumerator StoryFadeIn()
    {
        float elapsedTime = 0f;

        // Encreases the transperency 
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / transitionDuration);

            fadeToBlack.alpha = 1f - progress;
            storyPanel.alpha = progress;

            yield return null;
        }

        // Ensures story panel is fully visible
        fadeToBlack.alpha = 0f;
        storyPanel.alpha = 1f;

        // Ensures you can interact with the panel
        storyPanel.interactable = true;
        storyPanel.blocksRaycasts = true;
    }
    // Method starts when you click continue button
    public void ShowCredits()
    {
        // Ensures a transition cannot start while there is an active transition
        if (isTransitioning) return;

        StartCoroutine(CreditsSequence());
    }

    // Sequence for credits panal and return to Main Menu
    private IEnumerator CreditsSequence()
    {
        isTransitioning = true;

        float elapsed = 0f;

        // Fade out story and fade in credits.
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);

            storyPanel.alpha = 1f - progress;
            creditsPanel.alpha = progress;

            yield return null;
        }

        // Hiddens story panel and its interactions
        storyPanel.alpha = 0f;
        storyPanel.interactable = false;
        storyPanel.blocksRaycasts = false;

        // Credits come fully visible
        creditsPanel.alpha = 1f;
        creditsPanel.interactable = true;

        // Keep credits visible.
        yield return new WaitForSecondsRealtime(creditsDuration);

        // Fade the entire screen to black.
        elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);

            fadeToBlack.alpha = progress;

            yield return null;
        }

        // Screen is fully black before showing the final options.
        fadeToBlack.alpha = 1f;
        creditsPanel.alpha = 0f;
        creditsPanel.interactable = false;
        creditsPanel.blocksRaycasts = false;

        elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);

            fadeToBlack.alpha = 1f - progress;
            finalOptionsPanel.alpha = progress;

            yield return null;
        }

        fadeToBlack.alpha = 0f;
        finalOptionsPanel.alpha = 1f;
        finalOptionsPanel.interactable = true;
        finalOptionsPanel.blocksRaycasts = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }
}
