using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // UI Elements
    [Header("Main Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject storyPanel;
    public GameObject controlsPanel;
    public GameObject soundPanel;
    public GameObject creditsPanel;
    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        storyPanel.SetActive(false);
        controlsPanel.SetActive(false);
        soundPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowStoryPanel()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(true);
        controlsPanel.SetActive(false);
        soundPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowControlsPanel()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(false);
        controlsPanel.SetActive(true);
        soundPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowSoundPanel()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(false);
        controlsPanel.SetActive(false);
        soundPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void ShowCreditsPanel()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(false);
        controlsPanel.SetActive(false);
        soundPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
