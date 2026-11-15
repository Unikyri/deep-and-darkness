using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    // Pause Menu Panels management

    // UI Elements
    [Header("Pause Menu Panels")]
    [Tooltip("Panel principal del menú de pausa; se muestra al pausar y se oculta al reanudar.")]
    [SerializeField] private GameObject pauseMenuPanel;
    [Tooltip("Panel que muestra los controles del juego desde el menú de pausa.")]
    [SerializeField] private GameObject controlsPanel;
    [Tooltip("Panel con las opciones de sonido que se abre desde el menú de pausa.")]
    [SerializeField] private GameObject soundPanel;
    [Tooltip("Panel de confirmación que aparece antes de reiniciar la partida.")]
    [SerializeField] private GameObject confirmRestart;
    [Tooltip("Panel de confirmación que aparece antes de salir de la partida.")]
    [SerializeField] private GameObject confirmQuit;

    private bool isPaused = false;

    private void Start()
    {
        //Game starts unpaused, so we ensure the pause menu is hidden
        pauseMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        soundPanel.SetActive(false);
        confirmRestart.SetActive(false);
        confirmQuit.SetActive(false);

        // Ensure the game is not paused at the start
        Time.timeScale = 1f;
        isPaused = false;
    }

    // Panels navigation methods
    // Controls Panel
    public void ShowControls()
    {
        pauseMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    // Go back to Pause Menu from Controls Panel
    public void BackToPauseFromControls()
    {
        controlsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    // Sound Panel
    public void ShowSound()
    {
        pauseMenuPanel.SetActive(false);
        soundPanel.SetActive(true);
    }

    // Go back to Pause Menu from Sound Panel
    public void BackToPauseFromSound()
    {
        soundPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }
    // Confirm Restart Panel
    public void ShowConfirmRestart()
    {
        pauseMenuPanel.SetActive(false);
        confirmRestart.SetActive(true);
    }

    // Go back to Pause Menu from Confirm Restart Panel
    public void CancelRestart()
    {
        confirmRestart.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    // Confirm Quit Panel
    public void ShowConfirmQuit()
    {
        pauseMenuPanel.SetActive(false);
        confirmQuit.SetActive(true);
    }

    // Go back to Pause Menu from Confirm Quit Panel
    public void CancelQuit()
    {
        confirmQuit.SetActive(false);
        pauseMenuPanel.SetActive(true);
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

    // Pause the game
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pause the game
        pauseMenuPanel.SetActive(true);
    }

    // Resume the game
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume the game
        pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        // Ensure Keyboard is not null
        if (Keyboard.current == null) 
            return;

        // Check for the Escape key press to toggle pause
        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
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
