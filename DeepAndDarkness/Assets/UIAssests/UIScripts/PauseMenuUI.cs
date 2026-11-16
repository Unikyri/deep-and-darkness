using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    // Pause Menu Panels management

    // UI Elements
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject confirmRestart;
    [SerializeField] private GameObject confirmQuit;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float clickDelay = 0.12f;

    private bool isPaused;
    private Coroutine panelTransition;
    private GameObject[] panels;

    private void Awake()
    {
        // Initialize the panels array for easy management
        panels = new GameObject[] { pauseMenuPanel, controlsPanel, soundPanel, confirmRestart, confirmQuit };
    }

    private void Start()
    {
        // Ensure the pause menu is hidden at the start
        foreach (GameObject panel in panels)
            panel.SetActive(false);

        // Ensure the game is not paused at the start
        Time.timeScale = 1f;
        isPaused = false;
    }

    // Panels navigation methods
    public void ShowControls() => ShowPanel(controlsPanel);
    public void ShowSound() => ShowPanel(soundPanel);
    public void ShowConfirmRestart() => ShowPanel(confirmRestart);
    public void ShowConfirmQuit() => ShowPanel(confirmQuit);

    public void BackToPauseFromControls() => ShowPanel(pauseMenuPanel);
    public void BackToPauseFromSound() => ShowPanel(pauseMenuPanel);
    public void CancelRestart() => ShowPanel(pauseMenuPanel);
    public void CancelQuit() => ShowPanel(pauseMenuPanel);


    private void ShowPanel(GameObject panelToShow)
    {
        if (panelTransition != null)
            StopCoroutine(panelTransition);

        panelTransition = StartCoroutine(ShowPanelWithFade(panelToShow));
    }
    
    private IEnumerator ShowPanelWithFade(GameObject panelToShow)
    {
        // Wait for a short delay
        yield return new WaitForSecondsRealtime(clickDelay);
        // Activates the target panel and deactivates others
        foreach (GameObject panel in panels)
            panel.SetActive(panel == panelToShow);
        
        // Get the CanvasGroup component for fade effect
        CanvasGroup group = panelToShow.GetComponent<CanvasGroup>();

        if (group != null)
        {
            // Start with the panel fully transparent
            group.alpha = 0f;
            // Fade in the panel over the specified duration
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                // Use unscaled time for UI transitions
                elapsedTime += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            // Ensure the panel is fully visible at the end of the fade
            group.alpha = 1f;
        }

        panelTransition = null;
    }


    // Pause the game
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pause the game
        ShowPanel(pauseMenuPanel);
    }

    // Resume the game
    public void ResumeGame()
    {
        if (panelTransition != null)
            StopCoroutine(panelTransition);

        panelTransition = StartCoroutine(ResumeAfterDelay());
    }

    private IEnumerator ResumeAfterDelay()
    {
        // Wait for a short delay before resuming the game
        yield return new WaitForSecondsRealtime(clickDelay);

        foreach (GameObject panel in panels)
            panel.SetActive(false);

        Time.timeScale = 1f; // Resume the game
        isPaused = false;
        panelTransition = null;
    }

    // Restart the game
    public void RestartGame()
    {
        // Implementation for restarting the game
        // Ensure the game is unpaused before restarting
        Time.timeScale = 1f;
        // Load game scene
        SceneManager.LoadScene(1);
    }

    // Return to Main Menu
    public void ReturnToMainMenu()
    {
        // Implementation for returning to the main menu
        // unpause the game
        Time.timeScale = 1f;
        //Load main menu scene
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        // Ensure Keyboard is not null
        if (Keyboard.current == null) 
            return;

        // Check for the Escape key press to toggle pause
        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        // Avoid changing state while a panel transition is in progress
        if (panelTransition != null)
            return;

        // Ensure the other panels are not active before toggling pause
        // Ensure ESC is not pressed while in other panels
        if (controlsPanel.activeSelf || soundPanel.activeSelf || confirmRestart.activeSelf || confirmQuit.activeSelf)
            return;
        {
            // Toggle pause state
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
}