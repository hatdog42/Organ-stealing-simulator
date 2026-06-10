using System;
using System.Collections.Generic;
using MiniGames;
using UnityEngine;

public class SurgerySceneControler : MonoBehaviour
{
    public static SurgerySceneControler Instance { get; private set; }

    [SerializeField] private SpriteRenderer tvSprite;

    [Header("Major MiniGames")]
    [SerializeField] private MajorMiniGameBinding[] majorMiniGames;

    private MajorMiniGameBinding _discoveredDebugButtons;
    private Camera _selectedMajorMiniGameCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Patient selectedPatient = HealthBars.Instance?.SelectedPatient;
        MajorMiniGameType selectedMiniGame = selectedPatient?.majorMiniGame ?? MajorMiniGameType.Maze;

        ApplyMajorMiniGameSelection(selectedMiniGame);
        _selectedMajorMiniGameCamera = ResolveMajorMiniGameCamera(selectedMiniGame);

        if (_selectedMajorMiniGameCamera && TVController.Instance)
            TVController.Instance.RegisterMiniGameCamera(_selectedMajorMiniGameCamera);
    }

    public Camera SelectedMajorMiniGameCameraOrDefault(Camera fallbackCamera)
    {
        return _selectedMajorMiniGameCamera ? _selectedMajorMiniGameCamera : fallbackCamera;
    }

    private void ApplyMajorMiniGameSelection(MajorMiniGameType selectedMiniGame)
    {
        foreach (MajorMiniGameBinding miniGame in GetMajorMiniGames())
        {
            if (miniGame.root)
            {
                miniGame.root.SetActive(miniGame.type == selectedMiniGame);
            }

            if (miniGame.type == MajorMiniGameType.DebugButtons && miniGame.root)
            {
                EnsureDebugButtonMiniGame(miniGame.root);
            }
        }
    }

    private Camera ResolveMajorMiniGameCamera(MajorMiniGameType selectedMiniGame)
    {
        foreach (MajorMiniGameBinding miniGame in GetMajorMiniGames())
        {
            if (miniGame.type == selectedMiniGame) return miniGame.Camera;
        }

        return null;
    }

    private MajorMiniGameBinding[] GetMajorMiniGames()
    {
        if (!_discoveredDebugButtons.root)
        {
            _discoveredDebugButtons = DiscoverDebugButtons();
        }

        if (majorMiniGames == null || majorMiniGames.Length == 0)
        {
            return _discoveredDebugButtons.root ? new[] { _discoveredDebugButtons } : Array.Empty<MajorMiniGameBinding>();
        }

        if (!_discoveredDebugButtons.root || HasMajorMiniGameBinding(MajorMiniGameType.DebugButtons))
        {
            return majorMiniGames;
        }

        List<MajorMiniGameBinding> combinedMiniGames = new(majorMiniGames);
        combinedMiniGames.Add(_discoveredDebugButtons);
        return combinedMiniGames.ToArray();
    }

    private MajorMiniGameBinding DiscoverDebugButtons()
    {
        GameObject root = GameObject.Find("DebugButtonsFolder");
        if (!root)
        {
            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject.name == "DebugButtonsFolder" && gameObject.scene.IsValid())
                {
                    root = gameObject;
                    break;
                }
            }
        }

        if (!root) return default;

        return new MajorMiniGameBinding
        {
            type = MajorMiniGameType.DebugButtons,
            root = root,
            camera = root.GetComponentInChildren<Camera>(true)
        };
    }

    private bool HasMajorMiniGameBinding(MajorMiniGameType type)
    {
        if (majorMiniGames == null) return false;

        foreach (MajorMiniGameBinding miniGame in majorMiniGames)
        {
            if (miniGame.type == type) return true;
        }

        return false;
    }

    private void EnsureDebugButtonMiniGame(GameObject root)
    {
        if (root.GetComponentInChildren<DebugButtonMiniGame>(true)) return;

        root.AddComponent<DebugButtonMiniGame>();
    }
}

[Serializable]
public class MajorMiniGameBinding
{
    public MajorMiniGameType type;
    public GameObject root;
    public Camera camera;

    public Camera Camera => camera ? camera : root ? root.GetComponentInChildren<Camera>(true) : null;
}
