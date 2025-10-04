using MiniGames.Base;
using UnityEngine;

public class TVController : MonoBehaviour
{
    public static TVController Instance { get; private set; }
    
    [SerializeField]private GameObject tvRoot;
    [SerializeField]private RenderTexture screenRT;
    [SerializeField] private TVInputRelay inputRelay; 
    [SerializeField]private Camera[] miniGameCameras;
    
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
            cam.targetTexture = null;
        }
        if (tvRoot) tvRoot.SetActive(false);
    }

    public void OpenMiniGame(Camera targetCamera)
    {
        if(!targetCamera) return;
        
        foreach (var cam in miniGameCameras)
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

    public void CloseMiniGame()
    {
        foreach (var cam in miniGameCameras)
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
