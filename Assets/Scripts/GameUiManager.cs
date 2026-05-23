using System;
using System.Collections;
using Health;
using level2;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUiManager : MonoBehaviourSingleton<GameUiManager>
{
    [SerializeField] private GameObject topPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Portal portal;
    [SerializeField] private GameObject gotchiRescuedPanel;
    [SerializeField] private float gotchiRescuedPanelDuration = 1.5f;
    [SerializeField] private Transform uiControl;


    public static Action OnContinueToPlayAfterBoss;
    private static string TUTORIAL_KEY = "Tutorial";

    private void Awake()
    {
        gameOverPanel.SetActive(false);

        Player.Instance.OnStarting = true;
        Player.Instance.DisableControlsAndColliders = true;
        Player.Instance.Rb.isKinematic = true;

        if (PlayerPrefs.GetInt(TUTORIAL_KEY) == 0)
        {
            topPanel.SetActive(false);
            tutorialPanel.SetActive(true);
        }
        else
        {
            tutorialPanel.SetActive(false);
            topPanel.SetActive(true);
            portal.StartPortalTransition();
        }
        
        uiControl.gameObject.SetActive(Application.isMobilePlatform);
    }

    public void ShowGameOverPanel()
    {
        StartCoroutine(ShowGameOverPanelCoroutine());
    }
    
    public void ShowAndHideGotchiRescuedPanel()
    {
        gotchiRescuedPanel.SetActive(true);
        StartCoroutine(HideGotchiRescuedPanel());
    }
    
    private IEnumerator HideGotchiRescuedPanel()
    {
        yield return new WaitForSeconds(gotchiRescuedPanelDuration);
        gotchiRescuedPanel.SetActive(false);
        OnContinueToPlayAfterBoss?.Invoke();
    }
    
    public void CloseTutorial()
    {
        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        PlayerPrefs.Save();
        tutorialPanel.SetActive(false);
        topPanel.SetActive(true);
        portal.StartPortalTransition();
    }

    private IEnumerator ShowGameOverPanelCoroutine()
    {
        yield return new WaitForSeconds(1f);
        //PlayfabManager.Instance.StartCoroutine(PlayfabManager.Instance.GetLeaderboard("MaxDistance"));
        yield return new WaitForSeconds(1f);
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1.0f;
        RestartManager.Instance.Restart(0);
    }

    public void TogglePause()
    {
        pausePanel.SetActive(!pausePanel.activeInHierarchy);
        Time.timeScale = !pausePanel.activeInHierarchy ? 1.0f : 0.0f;
    }

    private void OnEnable()
    {
        InputManager.OnPause += TogglePause;
    }

    private void OnDisable()
    {
        InputManager.OnPause -= TogglePause;
    }
}