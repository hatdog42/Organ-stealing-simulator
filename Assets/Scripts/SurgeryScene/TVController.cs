using System.Collections.Generic;
using MiniGames;
using MiniGames.Base;
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
        if(!targetCamera) return;

        targetCamera = ResolveSelectedMajorMiniGameCamera(targetCamera);
        RegisterMiniGameCamera(targetCamera);
        
        foreach (var cam in _registeredCameras)
        {
            if (!cam) continue;
            cam.targetTexture = (cam == targetCamera) ? screenRT : null;
        }
        
        _activeCamera = targetCamera;
        
        var nextGame = targetCamera.GetComponentInParent<MiniGameBase>();
        if (_activeGame != null) _activeGame.OnFocusLost();
        _activeGame = nextGame;
        
        if (inputRelay) inputRelay.SetMiniGameCam(targetCamera);
        if (_activeGame != null) _activeGame.OnFocusGained(inputRelay);

        
        if (tvRoot) tvRoot.SetActive(true);
    }

    private Camera ResolveSelectedMajorMiniGameCamera(Camera requestedCamera)
    {
        if (!requestedCamera.GetComponentInParent<MazeGame>()) return requestedCamera;

        Patient selectedPatient = HealthBars.Instance?.SelectedPatient;
        if (selectedPatient == null)
        {
            Camera fallbackDebugCamera = FindDebugButtonsCamera();
            if (!fallbackDebugCamera)
            {
                Debug.LogWarning("Maze opened because no selected patient was found and no DebugButtonsFolder camera exists.");
                return requestedCamera;
            }

            Debug.LogWarning("No selected patient was found. Opening Debug Buttons instead of Maze for testing.");
            return fallbackDebugCamera;
        }

        if (selectedPatient.majorMiniGame == MajorMiniGameType.Maze) return requestedCamera;

        Camera selectedCamera = FindMajorMiniGameCamera(selectedPatient.majorMiniGame);
        if (!selectedCamera)
        {
            Debug.LogWarning($"Maze opened because no camera was found for major minigame '{selectedPatient.majorMiniGameName}'.");
            return requestedCamera;
        }

        Debug.Log($"Opening selected major minigame '{selectedPatient.majorMiniGameName}' instead of Maze.");
        return selectedCamera;
    }

    private Camera FindMajorMiniGameCamera(MajorMiniGameType majorMiniGameType)
    {
        return majorMiniGameType switch
        {
            MajorMiniGameType.DebugButtons => FindDebugButtonsCamera(),
            _ => null
        };
    }

    private Camera FindDebugButtonsCamera()
    {
        GameObject root = FindSceneObject("DebugButtonsFolder");
        if (!root)
        {
            Debug.LogWarning("DebugButtonsFolder was not found in the Surgery scene.");
            return null;
        }

        if (!root.GetComponentInChildren<DebugButtonMiniGame>(true))
        {
            root.AddComponent<DebugButtonMiniGame>();
        }

        Camera debugCamera = root.GetComponentInChildren<Camera>(true);
        if (!debugCamera) Debug.LogWarning("DebugButtonsFolder exists, but no child Camera was found.");

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
