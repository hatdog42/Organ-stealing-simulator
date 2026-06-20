using MiniGames;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Toggle forceDebugMiniGameToggle;
    [SerializeField] private string firstSceneName = "Exposition";

    private void Awake()
    {
        FindMissingControls();
        SyncForceDebugMiniGameToggle();
        BindControls();
    }

    private void OnEnable()
    {
        FindMissingControls();
        SyncForceDebugMiniGameToggle();
        BindControls();
    }

    private void OnDisable()
    {
        if (forceDebugMiniGameToggle)
        {
            forceDebugMiniGameToggle.onValueChanged.RemoveListener(SetForceDebugMiniGame);
        }
    }

    private void FindMissingControls()
    {
        if (!startButton || !quitButton)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                string buttonName = button.name.ToLowerInvariant();
                if (!startButton && buttonName.Contains("start")) startButton = button;
                if (!quitButton && buttonName.Contains("quit")) quitButton = button;
            }
        }

        if (forceDebugMiniGameToggle) return;

        foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true))
        {
            string toggleName = toggle.name.ToLowerInvariant();
            if (toggleName.Contains("debug") && toggleName.Contains("minigame"))
            {
                forceDebugMiniGameToggle = toggle;
                return;
            }
        }
    }

    private void SyncForceDebugMiniGameToggle()
    {
        if (!forceDebugMiniGameToggle) return;

        if (!MajorMiniGameDebugSettings.HasForceDebugMiniGamePreference)
        {
            SetForceDebugMiniGame(forceDebugMiniGameToggle.isOn);
            return;
        }

        forceDebugMiniGameToggle.SetIsOnWithoutNotify(MajorMiniGameDebugSettings.ForceDebugMiniGame);
    }

    private void BindControls()
    {
        ReplaceClickListener(startButton, StartGame);
        ReplaceClickListener(quitButton, QuitGame);
        ReplaceToggleListener(forceDebugMiniGameToggle, SetForceDebugMiniGame);
    }

    private static void ReplaceClickListener(Button button, UnityAction action)
    {
        if (!button) return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static void ReplaceToggleListener(Toggle toggle, UnityAction<bool> action)
    {
        if (!toggle) return;

        toggle.onValueChanged.RemoveListener(action);
        toggle.onValueChanged.AddListener(action);
    }

    private void SetForceDebugMiniGame(bool forceDebugMiniGame)
    {
        MajorMiniGameDebugSettings.ForceDebugMiniGame = forceDebugMiniGame;
    }

    private void StartGame()
    {
        if (!SceneController.Instance)
        {
            Debug.LogWarning("Cannot start game because no SceneController exists.");
            return;
        }

        SceneController.Instance.StartNewGame(firstSceneName);
    }

    private void QuitGame()
    {
        if (SceneController.Instance)
        {
            SceneController.Instance.Quit();
            return;
        }

        Application.Quit();
    }
}
