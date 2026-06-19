using System.Collections.Generic;
using MiniGames;
using MiniGames.Base;
using SurgeryScene;
using UnityEngine;

public class TVController : MonoBehaviour
{
    public static TVController Instance { get; private set; }
    
    [SerializeField]private GameObject tvRoot;
    [SerializeField]private RenderTexture screenRT;
    [SerializeField] private TVInputRelay inputRelay; 
    [SerializeField]private Camera[] miniGameCameras;

    [Header("Direct Surgery Debug")]
    [SerializeField] private bool useDirectSurgeryFallbackMiniGame = true;
    [SerializeField] private MajorMiniGameType directSurgeryFallbackMiniGame = MajorMiniGameType.Fishing;
    
    private readonly List<Camera> _registeredCameras = new();
    private MiniGameBase _activeGame;
    private Camera _activeCamera;
    private bool _isClosing;
    
    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
            
        foreach (var cam in miniGameCameras)
        {
            if (!cam) continue;
            RegisterMiniGameCamera(cam);
        }

        RegisterSceneMiniGameCameras();

        foreach (var cam in _registeredCameras)
        {
            cam.targetTexture = null;
            cam.enabled = false;
        }

        if (tvRoot) tvRoot.SetActive(false);
    }

    public void RegisterMiniGameCamera(Camera miniGameCamera)
    {
        if (!miniGameCamera || _registeredCameras.Contains(miniGameCamera)) return;

        _registeredCameras.Add(miniGameCamera);
        miniGameCamera.targetTexture = null;
        miniGameCamera.enabled = false;
    }

    public void OpenMiniGame(Camera targetCamera)
    {
        if (!targetCamera)
        {
            Debug.LogError("Cannot open minigame because the requested camera is missing.");
            return;
        }

        OpenMiniGameDirect(targetCamera);
    }

    public void OpenSelectedMajorMiniGame()
    {
        Camera targetCamera = ResolveSelectedMajorMiniGameCamera();
        if (!targetCamera) return;

        OpenMiniGameDirect(targetCamera);
    }

    private void OpenMiniGameDirect(Camera targetCamera)
    {
        if (!targetCamera) return;
        _isClosing = false;

        MiniGameBase nextGame = FindMiniGameForCamera(targetCamera);
        if (!nextGame)
        {
            Debug.LogError($"Cannot open minigame for camera '{targetCamera.name}' because no MiniGameBase was found for it.");
            return;
        }

        ActivateHierarchy(nextGame.transform);
        RegisterMiniGameCamera(targetCamera);
        
        foreach (var cam in _registeredCameras)
        {
            if (!cam) continue;
            bool isTargetCamera = cam == targetCamera;
            cam.targetTexture = isTargetCamera ? screenRT : null;
            cam.enabled = isTargetCamera;
        }
        
        _activeCamera = targetCamera;
        if (_activeGame != null) _activeGame.OnFocusLost();
        _activeGame = nextGame;
        
        if (inputRelay) inputRelay.SetMiniGameCam(targetCamera);
        if (_activeGame != null) _activeGame.OnFocusGained(inputRelay);

        
        if (tvRoot) tvRoot.SetActive(true);
    }

    private static void ActivateHierarchy(Transform child)
    {
        while (child)
        {
            child.gameObject.SetActive(true);
            child = child.parent;
        }
    }

    private MiniGameBase FindMiniGameForCamera(Camera targetCamera)
    {
        if (!targetCamera) return null;

        MiniGameBase game = targetCamera.GetComponentInParent<MiniGameBase>(true);
        if (game) return game;

        Transform root = targetCamera.transform.root;
        game = root ? root.GetComponentInChildren<MiniGameBase>(true) : null;
        if (game)
        {
            Debug.LogWarning($"Camera '{targetCamera.name}' is not under its MiniGameBase. Found '{game.name}' from the root instead. Consider moving the minigame script onto a parent of the camera.");
            return game;
        }

        Debug.LogError($"No MiniGameBase was found for camera '{targetCamera.name}'.");
        return null;
    }

    private Camera ResolveSelectedMajorMiniGameCamera()
    {
        if (!HealthBars.Instance)
        {
            return ResolveDirectSurgeryFallbackCamera("HealthBars.Instance is missing");
        }

        Patient selectedPatient = HealthBars.Instance?.SelectedPatient;
        if (selectedPatient == null)
        {
            return ResolveDirectSurgeryFallbackCamera("no patient has been selected");
        }

        Camera sceneSelectedCamera = SurgerySceneControler.Instance
            ? SurgerySceneControler.Instance.SelectedMajorMiniGameCameraOrDefault(null)
            : null;
        if (sceneSelectedCamera)
        {
            Debug.Log($"Opening selected major minigame '{selectedPatient.majorMiniGameName}'.");
            return sceneSelectedCamera;
        }

        Camera selectedCamera = FindMajorMiniGameCamera(selectedPatient.majorMiniGame);
        if (!selectedCamera)
        {
            Debug.LogError($"Cannot open selected major minigame '{selectedPatient.majorMiniGameName}' ({selectedPatient.majorMiniGame}) because no camera was found for it.");
            return null;
        }

        Debug.Log($"Opening selected major minigame '{selectedPatient.majorMiniGameName}'.");
        return selectedCamera;
    }

    private Camera ResolveDirectSurgeryFallbackCamera(string reason)
    {
        if (!useDirectSurgeryFallbackMiniGame)
        {
            Debug.LogError($"Cannot open selected major minigame because {reason}. Select a patient before entering Surgery.");
            return null;
        }

        Camera fallbackCamera = FindMajorMiniGameCamera(directSurgeryFallbackMiniGame);
        if (!fallbackCamera)
        {
            Debug.LogError($"Cannot open direct Surgery fallback minigame '{directSurgeryFallbackMiniGame}' because no camera was found for it.");
            return null;
        }

        Debug.LogWarning($"Opening direct Surgery fallback minigame '{directSurgeryFallbackMiniGame}' because {reason}.");
        return fallbackCamera;
    }

    private Camera FindMajorMiniGameCamera(MajorMiniGameType majorMiniGameType)
    {
        switch (majorMiniGameType)
        {
            case MajorMiniGameType.Maze:
                return FindMazeCamera();
            case MajorMiniGameType.DebugButtons:
                return FindDebugButtonsCamera();
            case MajorMiniGameType.Wordle:
                return FindWordleCamera();
            case MajorMiniGameType.Fishing:
                return FindFishingCamera();
            default:
                Debug.LogError($"No camera resolver exists for major minigame type '{majorMiniGameType}'.");
                return null;
        }
    }

    private Camera FindMazeCamera()
    {
        return FindCameraForMiniGame<MazeGame>("MazeFolder", "Maze");
    }

    private Camera FindWordleCamera()
    {
        return FindCameraForMiniGame<WordleManager>("WordleFolder", "Wordle");
    }

    private Camera FindFishingCamera()
    {
        return FindCameraForMiniGame<FishingMiniGame>("FishingFolder", "Fishing");
    }

    private Camera FindCameraForMiniGame<TMiniGame>(string rootName, string displayName)
        where TMiniGame : MiniGameBase
    {
        TMiniGame miniGame = null;
        GameObject root = FindSceneObject(rootName);
        if (root) miniGame = root.GetComponentInChildren<TMiniGame>(true);

        if (!miniGame)
        {
            miniGame = FindAnyObjectByType<TMiniGame>(FindObjectsInactive.Include);
        }

        if (!miniGame)
        {
            Debug.LogError($"No {displayName} minigame was found in the Surgery scene.");
            return null;
        }

        Camera miniGameCamera = root
            ? root.GetComponentInChildren<Camera>(true)
            : miniGame.GetComponentInChildren<Camera>(true);
        if (!miniGameCamera && miniGame.transform.root)
        {
            miniGameCamera = miniGame.transform.root.GetComponentInChildren<Camera>(true);
        }

        if (miniGameCamera) return miniGameCamera;

        Debug.LogError($"{displayName} minigame '{miniGame.name}' exists, but no child Camera was found.");
        return null;
    }

    private Camera FindDebugButtonsCamera()
    {
        GameObject root = FindSceneObject("DebugButtonsFolder");
        if (!root)
        {
            Debug.LogError("DebugButtonsFolder was not found in the Surgery scene.");
            return null;
        }

        if (!root.GetComponentInChildren<DebugButtonMiniGame>(true))
        {
            Debug.LogError("DebugButtonsFolder exists, but no DebugButtonMiniGame script was found under it.");
            return null;
        }

        Camera debugCamera = root.GetComponentInChildren<Camera>(true);
        if (!debugCamera) Debug.LogError("DebugButtonsFolder exists, but no child Camera was found.");

        return debugCamera;
    }

    private static GameObject FindSceneObject(string objectName)
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

    private void RegisterSceneMiniGameCameras()
    {
        foreach (Camera miniGameCamera in FindObjectsByType<Camera>(FindObjectsInactive.Include))
        {
            if (!IsMiniGameCamera(miniGameCamera)) continue;

            RegisterMiniGameCamera(miniGameCamera);
        }
    }

    private static bool IsMiniGameCamera(Camera camera)
    {
        if (!camera) return false;
        if (camera.GetComponentInParent<MiniGameBase>(true)) return true;

        Transform root = camera.transform.root;
        return root && root.GetComponentInChildren<MiniGameBase>(true);
    }

    public void CloseMiniGame()
    {
        if (_isClosing) return;

        DisableSurgeryHitbox crtAnimation = tvRoot ? tvRoot.GetComponentInChildren<DisableSurgeryHitbox>() : null;
        if (tvRoot && tvRoot.activeInHierarchy && crtAnimation)
        {
            _isClosing = true;
            crtAnimation.PlayCloseAnimation(FinishCloseMiniGame);
            return;
        }

        FinishCloseMiniGame();
    }

    private void FinishCloseMiniGame()
    {
        _isClosing = false;

        foreach (var cam in _registeredCameras)
        {
            if (!cam) continue;
            cam.targetTexture = null;
            cam.enabled = false;
        }
        if (_activeGame != null) _activeGame.OnFocusLost();
        _activeGame = null;
        _activeCamera = null;
        
        if(inputRelay) inputRelay.SetMiniGameCam(null);
        if (tvRoot) tvRoot.SetActive(false);
    }
}
