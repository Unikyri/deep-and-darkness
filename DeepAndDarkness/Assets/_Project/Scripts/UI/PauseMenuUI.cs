using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
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

    [Header("Transition")]
    [Tooltip("Duración en segundos de la transición de entrada de cada panel.")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

    [Tooltip("Pausa breve antes de cambiar de panel para respetar la animación del botón.")]
    [SerializeField, Min(0f)] private float clickDelay = 0.12f;

    [Header("Gameplay HUD")]
    [Tooltip("Texto que muestra el progreso de los objetos de misión.")]
    [SerializeField] private TMPro.TMP_Text gameplayStatusText;

    [Tooltip("Imagen que muestra el estado de vida del vampiro.")]
    [SerializeField] private UnityEngine.UI.Image vampireLifeIcon;
    [Tooltip("Sprite del vial con sangre completa.")]
    [SerializeField] private Sprite vampireLifeFullSprite;
    [Tooltip("Sprite del vial con la mitad de la sangre.")]
    [SerializeField] private Sprite vampireLifeHalfSprite;
    [Tooltip("Sprite del vial vacío.")]
    [SerializeField] private Sprite vampireLifeEmptySprite;

    [Tooltip("Imagen que muestra el estado de vida de la sirena.")]
    [SerializeField] private UnityEngine.UI.Image mermaidLifeIcon;
    [Tooltip("Sprite de la concha abierta.")]
    [SerializeField] private Sprite mermaidLifeFullSprite;
    [Tooltip("Sprite de la concha semiabierta.")]
    [SerializeField] private Sprite mermaidLifeHalfSprite;
    [Tooltip("Sprite de la concha cerrada.")]
    [SerializeField] private Sprite mermaidLifeEmptySprite;

    [Tooltip("Texto que comunica el resultado final de la partida.")]
    [SerializeField] private TMPro.TMP_Text gameResultText;

    // Referencia a la sesión del nivel y a los personajes.
    private GameSessionManager gameSession;
    private CharacterLives[] characters;

    // Paneles del menú y transición que se esté ejecutando.
    private GameObject[] panels;
    private Coroutine panelTransition;

    private bool isPaused;
    private bool isGameEnded;

    // Valores que se muestran en el HUD.
    private int collectedCollectibles;
    private int requiredCollectibles;

    private void Awake()
    {
        // Reunimos los paneles para poder activarlos o desactivarlos en conjunto.
        panels = new[] { pauseMenuPanel, controlsPanel, soundPanel, confirmRestart, confirmQuit };
    }

    private void Start()
    {
        SetAllPanelsInactive();

        if (gameResultText != null)
        {
            gameResultText.gameObject.SetActive(false);
        }

        // Buscamos la sesión del nivel para escuchar sus eventos.
        gameSession = FindFirstObjectByType<GameSessionManager>();

        if (gameSession != null)
        {
            gameSession.CollectiblesChanged += HandleCollectiblesChanged;
            gameSession.GameOverTriggered += HandleGameOver;

            HandleCollectiblesChanged(gameSession.CollectedCollectibles, gameSession.RequiredCollectibles);
        }

        // Escuchamos los cambios de vida de los dos personajes.
        characters = FindObjectsByType<CharacterLives>(FindObjectsSortMode.None);

        foreach (CharacterLives character in characters)
        {
            if (character != null)
            {
                character.LivesChanged += HandleLivesChanged;
            }
        }

        UpdateGameplayStatus();
    }

    private void OnDestroy()
    {
        // Dejamos de escuchar los eventos al destruir esta interfaz.
        if (gameSession != null)
        {
            gameSession.CollectiblesChanged -= HandleCollectiblesChanged;
            gameSession.GameOverTriggered -= HandleGameOver;
        }

        if (characters == null) { return; }

        foreach (CharacterLives character in characters)
        {
            if (character != null)
            {
                character.LivesChanged -= HandleLivesChanged;
            }
        }
    }

    public void ShowControls()
    {
        if (isGameEnded) { return; }

        ShowPanel(controlsPanel);
    }

    public void BackToPauseFromControls()
    {
        if (isGameEnded) { return; }

        ShowPanel(pauseMenuPanel);
    }

    public void ShowSound()
    {
        if (isGameEnded) { return; }

        ShowPanel(soundPanel);
    }

    public void BackToPauseFromSound()
    {
        if (isGameEnded) { return; }

        ShowPanel(pauseMenuPanel);
    }

    public void ShowConfirmRestart()
    {
        if (gameResultText != null)
        {
            gameResultText.gameObject.SetActive(false);
        }

        ShowPanel(confirmRestart);
    }

    public void CancelRestart()
    {
        ShowPanel(pauseMenuPanel);

        if (isGameEnded && gameResultText != null)
        {
            gameResultText.gameObject.SetActive(true);
        }
    }

    public void ShowConfirmQuit()
    {
        if (gameResultText != null)
        {
            gameResultText.gameObject.SetActive(false);
        }

        ShowPanel(confirmQuit);
    }

    public void CancelQuit()
    {
        ShowPanel(pauseMenuPanel);

        if (isGameEnded && gameResultText != null)
        {
            gameResultText.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // La sesión pertenece a la escena del nivel, aunque la UI esté en PausaScene.
        if (gameSession == null)
        {
            Debug.LogError("No se puede reiniciar: falta la sesión del nivel.", this);
            return;
        }

        string currentLevelName = gameSession.gameObject.scene.name;
        string sceneToLoad = gameSession.IsVictory ? "Gameplay" : currentLevelName;

        // Tras ganar empezamos desde el primer nivel; tras perder reiniciamos el nivel actual.
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        if (isGameEnded || isPaused) { return; }

        isPaused = true;
        Time.timeScale = 0f;

        ShowPanel(pauseMenuPanel);
    }

    public void ResumeGame()
    {
        if (isGameEnded || !isPaused) { return; }

        StopPanelTransition();
        panelTransition = StartCoroutine(ResumeAfterDelay());
    }

    private void Update()
    {
        if (isGameEnded || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) { return; }

        if (panelTransition != null || controlsPanel.activeSelf || soundPanel.activeSelf || confirmRestart.activeSelf || confirmQuit.activeSelf) { return; }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null) { return; }

        StopPanelTransition();
        panelTransition = StartCoroutine(ShowPanelWithFade(panelToShow));
    }

    private IEnumerator ShowPanelWithFade(GameObject panelToShow)
    {
        // El tiempo real permite que la transición funcione durante la pausa.
        yield return new WaitForSecondsRealtime(clickDelay);

        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(panel == panelToShow);
            }
        }

        CanvasGroup group = panelToShow.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.alpha = 0f;
            float elapsedTime = 0f;

            // Aumentamos la transparencia gradualmente hasta mostrar el panel.
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

                yield return null;
            }

            group.alpha = 1f;
        }

        panelTransition = null;
    }

    private IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(clickDelay);

        SetAllPanelsInactive();
        Time.timeScale = 1f;
        isPaused = false;
        panelTransition = null;
    }

    private void StopPanelTransition()
    {
        if (panelTransition == null) { return; }

        StopCoroutine(panelTransition);
        panelTransition = null;
    }

    private void SetAllPanelsInactive()
    {
        if (panels == null) { return; }

        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
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
        UpdateLifeIconVisibility();

        if (gameplayStatusText != null) { gameplayStatusText.text = $"Objetos: {collectedCollectibles}/{requiredCollectibles}"; }

        UpdateLifeIcon(vampireLifeIcon, GetCharacterLives("Vampiro"), vampireLifeFullSprite, vampireLifeHalfSprite, vampireLifeEmptySprite);
        UpdateLifeIcon(mermaidLifeIcon, GetCharacterLives("Sirena"), mermaidLifeFullSprite, mermaidLifeHalfSprite, mermaidLifeEmptySprite);
    }

    private void UpdateLifeIconVisibility()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        bool isGameplayScene = gameSession != null && activeScene == gameSession.gameObject.scene && (activeScene.name == "Gameplay" || activeScene.name == "GameplayLevel2");

        if (vampireLifeIcon != null) { vampireLifeIcon.gameObject.SetActive(isGameplayScene); }
        if (mermaidLifeIcon != null) { mermaidLifeIcon.gameObject.SetActive(isGameplayScene); }
    }

    private CharacterLives GetCharacterLives(string characterName)
    {
        if (characters != null)
        {
            foreach (CharacterLives character in characters)
            {
                if (character != null && character.CharacterName == characterName) { return character; }
            }
        }

        return null;
    }

    private void UpdateLifeIcon(UnityEngine.UI.Image lifeIcon, CharacterLives character, Sprite fullSprite, Sprite halfSprite, Sprite emptySprite)
    {
        if (lifeIcon == null || character == null) { return; }

        if (character.CurrentLives >= 3) { lifeIcon.sprite = fullSprite; }
        else if (character.CurrentLives == 2) { lifeIcon.sprite = halfSprite; }
        else { lifeIcon.sprite = emptySprite; }
    }

    private void HandleGameOver(string defeatedCharacterName)
    {
        ShowGameResult($"DERROTA\n{defeatedCharacterName} se quedó sin vidas");
    }

    private void ShowGameResult(string message)
    {
        StopPanelTransition();

        Time.timeScale = 0f;
        isGameEnded = true;
        isPaused = true;

        if (controlsPanel != null) { controlsPanel.SetActive(false); }
        if (soundPanel != null) { soundPanel.SetActive(false); }
        if (confirmRestart != null) { confirmRestart.SetActive(false); }
        if (confirmQuit != null) { confirmQuit.SetActive(false); }

        HidePauseOptionsAfterGameEnd();

        if (pauseMenuPanel != null)
        {
            CanvasGroup group = pauseMenuPanel.GetComponent<CanvasGroup>();

            if (group != null)
            {
                group.alpha = 1f;
            }

            pauseMenuPanel.SetActive(true);
        }

        if (gameResultText != null)
        {
            gameResultText.text = message;
            gameResultText.gameObject.SetActive(true);
        }
    }

    private void HidePauseOptionsAfterGameEnd()
    {
        if (pauseMenuPanel == null) { return; }

        Transform pauseBackground = pauseMenuPanel.transform.Find("PauseBackground");

        if (pauseBackground == null) { return; }

        Transform continueButton = pauseBackground.Find("ContinueGame");
        Transform controlsButton = pauseBackground.Find("Controls");
        Transform soundButton = pauseBackground.Find("Sound");

        if (continueButton != null) { continueButton.gameObject.SetActive(false); }
        if (controlsButton != null) { controlsButton.gameObject.SetActive(false); }
        if (soundButton != null) { soundButton.gameObject.SetActive(false); }
    }
}
