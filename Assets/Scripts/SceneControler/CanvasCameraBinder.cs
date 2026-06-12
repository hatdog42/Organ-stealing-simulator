using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasCameraBinder : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float planeDistance = 1f;
    [SerializeField] private bool includeWorldSpaceCanvases;

    private Camera _lastCamera;
    private int _lastCanvasCount = -1;

    private static CanvasCameraBinder _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void ApplyAll()
    {
        EnsureInstance().ApplyToAllCanvases();
    }

    public static void ApplyToChildren(GameObject root)
    {
        if (!root) return;

        EnsureInstance().ApplyToCanvases(root.GetComponentsInChildren<Canvas>(true));
    }

    private static CanvasCameraBinder EnsureInstance()
    {
        if (_instance) return _instance;

        GameObject binder = new("CanvasCameraBinder");
        CanvasCameraBinder instance = binder.AddComponent<CanvasCameraBinder>();
        DontDestroyOnLoad(binder);
        return instance;
    }

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToAllCanvases();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        Camera currentCamera = Camera.main;
        int currentCanvasCount = FindObjectsByType<Canvas>(FindObjectsInactive.Include).Length;

        if (_lastCamera == currentCamera && _lastCanvasCount == currentCanvasCount) return;

        ApplyToAllCanvases();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToAllCanvases();
    }

    private void ApplyToAllCanvases()
    {
        ApplyToCanvases(FindObjectsByType<Canvas>(FindObjectsInactive.Include));
    }

    private void ApplyToCanvases(Canvas[] canvases)
    {
        Camera mainCamera = Camera.main;
        if (!mainCamera) return;

        _lastCamera = mainCamera;
        _lastCanvasCount = canvases.Length;

        foreach (Canvas canvas in canvases)
        {
            if (!canvas) continue;
            if (canvas.name == "AspectRatioBars") continue;
            if (!includeWorldSpaceCanvases && canvas.renderMode == RenderMode.WorldSpace) continue;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = planeDistance;
        }
    }
}
