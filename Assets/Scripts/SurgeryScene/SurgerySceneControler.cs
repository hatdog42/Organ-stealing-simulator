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

    private MajorMiniGameBinding _discoveredMaze;
    private MajorMiniGameBinding _discoveredDebugButtons;
    private MajorMiniGameBinding _discoveredWordle;
    private MajorMiniGameBinding _discoveredFishing;
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
        if (!_discoveredMaze.root)
        {
            _discoveredMaze = DiscoverMiniGame<MiniGames.MazeGame>("MazeFolder", MajorMiniGameType.Maze);
        }

        if (!_discoveredDebugButtons.root)
        {
            _discoveredDebugButtons = DiscoverDebugButtons();
        }

        if (!_discoveredWordle.root)
        {
            _discoveredWordle = DiscoverWordle();
        }

        if (!_discoveredFishing.root)
        {
            _discoveredFishing = DiscoverFishing();
        }

        List<MajorMiniGameBinding> combinedMiniGames = majorMiniGames != null
            ? new List<MajorMiniGameBinding>(majorMiniGames)
            : new List<MajorMiniGameBinding>();

        AddDiscoveredMiniGame(combinedMiniGames, _discoveredMaze);
        AddDiscoveredMiniGame(combinedMiniGames, _discoveredDebugButtons);
        AddDiscoveredMiniGame(combinedMiniGames, _discoveredWordle);
        AddDiscoveredMiniGame(combinedMiniGames, _discoveredFishing);
        return combinedMiniGames.ToArray();
    }

    private void AddDiscoveredMiniGame(List<MajorMiniGameBinding> miniGames, MajorMiniGameBinding discoveredMiniGame)
    {
        if (!discoveredMiniGame.root) return;

        foreach (MajorMiniGameBinding miniGame in miniGames)
        {
            if (miniGame.type == discoveredMiniGame.type) return;
        }

        miniGames.Add(discoveredMiniGame);
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

    private MajorMiniGameBinding DiscoverWordle()
    {
        return DiscoverMiniGame<WordleManager>("WordleFolder", MajorMiniGameType.Wordle);
    }

    private MajorMiniGameBinding DiscoverFishing()
    {
        return DiscoverMiniGame<FishingMiniGame>("FishingFolder", MajorMiniGameType.Fishing);
    }

    private MajorMiniGameBinding DiscoverMiniGame<TMiniGame>(string rootName, MajorMiniGameType type)
        where TMiniGame : Component
    {
        GameObject root = FindSceneObject(rootName);
        if (!root)
        {
            TMiniGame miniGame = FindAnyObjectByType<TMiniGame>(FindObjectsInactive.Include);
            if (miniGame) root = miniGame.transform.root.gameObject;
        }

        if (!root) return default;

        return new MajorMiniGameBinding
        {
            type = type,
            root = root,
            camera = root.GetComponentInChildren<Camera>(true)
        };
    }

    private GameObject FindSceneObject(string objectName)
    {
        GameObject root = GameObject.Find(objectName);
        if (root) return root;

        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject.name == objectName && gameObject.scene.IsValid())
            {
                return gameObject;
            }
        }

        return null;
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
