using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string firstSceneName = "Exposition";

    private void Awake()
    {
        FindMissingButtons();
        BindButtons();
    }

    private void OnEnable()
    {
        BindButtons();
    }

    private void FindMissingButtons()
    {
        if (startButton && quitButton) return;

        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            string buttonName = button.name.ToLowerInvariant();
            if (!startButton && buttonName.Contains("start")) startButton = button;
            if (!quitButton && buttonName.Contains("quit")) quitButton = button;
        }
    }

    private void BindButtons()
    {
        ReplaceClickListener(startButton, StartGame);
        ReplaceClickListener(quitButton, QuitGame);
    }

    private static void ReplaceClickListener(Button button, UnityAction action)
    {
        if (!button) return;

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(action);
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
