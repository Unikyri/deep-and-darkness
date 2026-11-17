using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalCreditsController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup storyPanel;
    [SerializeField] private CanvasGroup creditsPanel;
    [SerializeField] private CanvasGroup fadeToBlack;

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
            elapsed += Time.deltaTime;
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
        yield return new WaitForSeconds(creditsDuration);

        // Fade the entire screen to black.
        elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);

            fadeToBlack.alpha = progress;

            yield return null;
        }

        // Screen is fully black before transitioning to Main menu
        fadeToBlack.alpha = 1f;

        // Return to the main menu.
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
