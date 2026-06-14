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

        foreach (var cam in _registeredCameras)
        {
            cam.targetTexture = null;
        }

        if (tvRoot) tvRoot.SetActive(false);
    }

    public void RegisterMiniGameCamera(Camera miniGameCamera)
    {
        if (!miniGameCamera || _registeredCameras.Contains(miniGameCamera)) return;

        _registeredCameras.Add(miniGameCamera);
        miniGameCamera.targetTexture = null;
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

        RegisterMiniGameCamera(targetCamera);
        
        foreach (var cam in _registeredCameras)
        {
            if (!cam) continue;
            cam.targetTexture = (cam == targetCamera) ? screenRT : null;
        }
        
        _activeCamera = targetCamera;
        if (_activeGame != null) _activeGame.OnFocusLost();
        _activeGame = nextGame;
        
        if (inputRelay) inputRelay.SetMiniGameCam(targetCamera);
        if (_activeGame != null) _activeGame.OnFocusGained(inputRelay);

        
        if (tvRoot) tvRoot.SetActive(true);
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
            Debug.LogError("Cannot open selected major minigame because HealthBars.Instance is missing.");
            return null;
        }

        Patient selectedPatient = HealthBars.Instance?.SelectedPatient;
        if (selectedPatient == null)
        {
            Debug.LogError("Cannot open selected major minigame because no patient has been selected. Select a patient before entering Surgery.");
            return null;
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

    private Camera FindMajorMiniGameCamera(MajorMiniGameType majorMiniGameType)
    {
        switch (majorMiniGameType)
        {
            case MajorMiniGameType.Maze:
                return FindMazeCamera();
            case MajorMiniGameType.DebugButtons:
                return FindDebugButtonsCamera();
            default:
                Debug.LogError($"No camera resolver exists for major minigame type '{majorMiniGameType}'.");
                return null;
        }
    }

    private Camera FindMazeCamera()
    {
        foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsInactive.Include))
        {
            if (camera && camera.GetComponentInParent<MazeGame>(true)) return camera;
        }

        Debug.LogError("No MazeGame camera was found in the Surgery scene.");
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
        }
        if (_activeGame != null) _activeGame.OnFocusLost();
        _activeGame = null;
        _activeCamera = null;
        
        if(inputRelay) inputRelay.SetMiniGameCam(null);
        if (tvRoot) tvRoot.SetActive(false);
    }
}
