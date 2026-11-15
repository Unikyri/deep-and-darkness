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

    [Header("Gameplay HUD")]
    [Tooltip("Texto que muestra las vidas de ambos personajes y el progreso de los objetos de misión.")]
    [SerializeField] private TMPro.TMP_Text gameplayStatusText;
    [Tooltip("Texto que comunica el resultado final de la partida.")]
    [SerializeField] private TMPro.TMP_Text gameResultText;

    private GameSessionManager gameSession;
    private CharacterLives[] characters;
    private bool isPaused;
    private bool isGameEnded;
    private int collectedCollectibles;
    private int requiredCollectibles;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        soundPanel.SetActive(false);
        confirmRestart.SetActive(false);
        confirmQuit.SetActive(false);
        if (gameResultText != null) { gameResultText.gameObject.SetActive(false); }

        gameSession = FindFirstObjectByType<GameSessionManager>();
        if (gameSession != null)
        {
            gameSession.CollectiblesChanged += HandleCollectiblesChanged;
            gameSession.GameOverTriggered += HandleGameOver;
            gameSession.VictoryTriggered += HandleVictory;
            HandleCollectiblesChanged(gameSession.CollectedCollectibles, gameSession.RequiredCollectibles);
        }

        characters = FindObjectsByType<CharacterLives>(FindObjectsSortMode.None);
        foreach (CharacterLives character in characters) { character.LivesChanged += HandleLivesChanged; }
        UpdateGameplayStatus();
    }

    private void OnDestroy()
    {
        if (gameSession != null)
        {
            gameSession.CollectiblesChanged -= HandleCollectiblesChanged;
            gameSession.GameOverTriggered -= HandleGameOver;
            gameSession.VictoryTriggered -= HandleVictory;
        }

        if (characters == null) { return; }
        foreach (CharacterLives character in characters)
        {
            if (character != null) { character.LivesChanged -= HandleLivesChanged; }
        }
    }

    // Panels navigation methods
    // Controls Panel
    public void ShowControls()
    {
        if (isGameEnded) { return; }
        pauseMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    // Go back to Pause Menu from Controls Panel
    public void BackToPauseFromControls()
    {
        if (isGameEnded) { return; }
        controlsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    // Sound Panel
    public void ShowSound()
    {
        if (isGameEnded) { return; }
        pauseMenuPanel.SetActive(false);
        soundPanel.SetActive(true);
    }

    // Go back to Pause Menu from Sound Panel
    public void BackToPauseFromSound()
    {
        if (isGameEnded) { return; }
        soundPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }
    // Confirm Restart Panel
    public void ShowConfirmRestart()
    {
        if (gameResultText != null) { gameResultText.gameObject.SetActive(false); }
        pauseMenuPanel.SetActive(false);
        confirmRestart.SetActive(true);
    }

    // Go back to Pause Menu from Confirm Restart Panel
    public void CancelRestart()
    {
        confirmRestart.SetActive(false);
        pauseMenuPanel.SetActive(true);
        if (isGameEnded && gameResultText != null) { gameResultText.gameObject.SetActive(true); }
    }

    // Confirm Quit Panel
    public void ShowConfirmQuit()
    {
        if (gameResultText != null) { gameResultText.gameObject.SetActive(false); }
        pauseMenuPanel.SetActive(false);
        confirmQuit.SetActive(true);
    }

    // Go back to Pause Menu from Confirm Quit Panel
    public void CancelQuit()
    {
        confirmQuit.SetActive(false);
        pauseMenuPanel.SetActive(true);
        if (isGameEnded && gameResultText != null) { gameResultText.gameObject.SetActive(true); }
    }


    // Restart the game
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay");
    }

    // Return to Main Menu
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Pause the game
    public void PauseGame()
    {
        if (isGameEnded) { return; }
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
    }

    // Resume the game
    public void ResumeGame()
    {
        if (isGameEnded) { return; }
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameEnded || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) { return; }
        if (controlsPanel.activeSelf || soundPanel.activeSelf || confirmRestart.activeSelf || confirmQuit.activeSelf) { return; }

        if (isPaused) { ResumeGame(); }
        else { PauseGame(); }
    }

    private void HandleCollectiblesChanged(int collected, int required)
    {
        collectedCollectibles = collected;
        requiredCollectibles = required;
        UpdateGameplayStatus();
    }

    private void HandleLivesChanged(int currentLives, int startingLives)
    {
        UpdateGameplayStatus();
    }

    private void UpdateGameplayStatus()
    {
        if (gameplayStatusText == null) { return; }

        string vampireStatus = GetCharacterStatus("Vampiro");
        string mermaidStatus = GetCharacterStatus("Sirena");
        gameplayStatusText.text = $"{vampireStatus}     {mermaidStatus}     Objetos: {collectedCollectibles}/{requiredCollectibles}";
    }

    private string GetCharacterStatus(string characterName)
    {
        if (characters != null)
        {
            foreach (CharacterLives character in characters)
            {
                if (character != null && character.CharacterName == characterName)
                {
                    return $"{characterName}: {character.CurrentLives}/{character.StartingLives}";
                }
            }
        }

        return $"{characterName}: --";
    }

    private void HandleGameOver(string defeatedCharacterName)
    {
        ShowGameResult($"DERROTA\n{defeatedCharacterName} se quedó sin vidas");
    }

    private void HandleVictory()
    {
        ShowGameResult("¡VICTORIA!\nEncontraron los objetos mágicos");
    }

    private void ShowGameResult(string message)
    {
        isGameEnded = true;
        isPaused = true;
        controlsPanel.SetActive(false);
        soundPanel.SetActive(false);
        confirmRestart.SetActive(false);
        confirmQuit.SetActive(false);
        HidePauseOptionsAfterGameEnd();
        pauseMenuPanel.SetActive(true);

        if (gameResultText != null)
        {
            gameResultText.text = message;
            gameResultText.gameObject.SetActive(true);
        }
    }

    private void HidePauseOptionsAfterGameEnd()
    {
        Transform pauseBackground = pauseMenuPanel.transform.Find("PauseBackground");
        if (pauseBackground == null) { return; }

        Transform continueButton = pauseBackground.Find("Continue");
        Transform controlsButton = pauseBackground.Find("Controls");
        Transform soundButton = pauseBackground.Find("Sound");
        if (continueButton != null) { continueButton.gameObject.SetActive(false); }
        if (controlsButton != null) { controlsButton.gameObject.SetActive(false); }
        if (soundButton != null) { soundButton.gameObject.SetActive(false); }
    }
}
