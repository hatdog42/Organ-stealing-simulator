using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenueControler : MonoBehaviour
{
    private static PauseMenueControler _instance;

    [Header("Global Pause")]
    [SerializeField] private string[] blockedSceneNames = { "MainMenue" };
    [SerializeField] private string mainMenuSceneName = "MainMenue";

    [Header("Menu Prefab")]
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private string resourcesPrefabPath = "PauseMenue";

    [Header("UI Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private GameObject _pauseMenuRoot;
    private bool _isPaused;
    private float _timeScaleBeforePause = 1f;

    public static bool IsPaused => _instance && _instance._isPaused;
    public static event Action<bool> PauseChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance) return;

        GameObject pauseManager = new("PauseMenueControler");
        pauseManager.AddComponent<PauseMenueControler>();
        DontDestroyOnLoad(pauseManager);
    }

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void Update()
    {
        HandlePauseInput();
    }

    public void Resume()
    {
        ResumeGame();
        HidePauseMenu();
    }

    public void QuitToMainMenu()
    {
        ResumeGame();
        HidePauseMenu();

        if (SceneController.Instance)
        {
            SceneController.Instance.LoadScene(mainMenuSceneName);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void HandlePauseInput()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame && !Keyboard.current.pKey.wasPressedThisFrame) return;

        if (_isPaused)
        {
            Resume();
            return;
        }

        if (CanPauseCurrentScene()) PauseGame();
    }

    private bool CanPauseCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid()) return false;

        foreach (string blockedSceneName in blockedSceneNames)
        {
            if (activeScene.name == blockedSceneName) return false;
        }

        return true;
    }

    private void PauseGame()
    {
        if (_isPaused) return;

        _isPaused = true;
        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;

        PauseChanged?.Invoke(true);
        AudioManager.Instance?.PauseMusic();

        ShowPauseMenu();
    }

    private void ResumeGame()
    {
        if (!_isPaused) return;

        _isPaused = false;
        Time.timeScale = _timeScaleBeforePause <= 0f ? 1f : _timeScaleBeforePause;

        PauseChanged?.Invoke(false);
        AudioManager.Instance?.ResumeMusic();
    }

    private void ShowPauseMenu()
    {
        if (!_pauseMenuRoot) _pauseMenuRoot = GetOrCreatePauseMenuRoot();

        if (!_pauseMenuRoot)
        {
            Debug.LogError("Cannot open pause menu because no pauseMenuPrefab is assigned.");
            ResumeGame();
            return;
        }

        _pauseMenuRoot.SetActive(true);
        CanvasCameraBinder.ApplyToChildren(_pauseMenuRoot);
        FindMissingButtons();
        BindButtons();
    }

    private void HidePauseMenu()
    {
        if (_pauseMenuRoot) _pauseMenuRoot.SetActive(false);
    }

    private GameObject GetOrCreatePauseMenuRoot()
    {
        if (_pauseMenuRoot) return _pauseMenuRoot;

        GameObject prefab = pauseMenuPrefab;
        if (!prefab && !string.IsNullOrWhiteSpace(resourcesPrefabPath))
        {
            prefab = Resources.Load<GameObject>(resourcesPrefabPath);
        }

        if (!prefab) return null;

        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;
        DontDestroyOnLoad(instance);

        foreach (AudioListener listener in instance.GetComponentsInChildren<AudioListener>(true))
        {
            listener.enabled = false;
        }

        return instance;
    }

    private void FindMissingButtons()
    {
        if (!_pauseMenuRoot || resumeButton && quitButton) return;

        foreach (Button button in _pauseMenuRoot.GetComponentsInChildren<Button>(true))
        {
            string buttonName = button.name.ToLowerInvariant();
            if (!resumeButton && buttonName.Contains("resume")) resumeButton = button;
            if (!quitButton && buttonName.Contains("quit")) quitButton = button;
        }
    }

    private void BindButtons()
    {
        BindButton(resumeButton, ResumeButtonClicked);
        BindButton(quitButton, QuitButtonClicked);
    }

    private static void BindButton(Button button, UnityAction action)
    {
        if (!button) return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void ResumeButtonClicked()
    {
        PlayClickSound();
        Resume();
    }

    private void QuitButtonClicked()
    {
        PlayClickSound();
        QuitToMainMenu();
    }

    private static void PlayClickSound()
    {
        AudioManager.Instance?.PlayButtonClick();
    }
}
