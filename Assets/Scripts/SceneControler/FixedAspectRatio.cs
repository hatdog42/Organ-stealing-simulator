using UnityEngine;
using UnityEngine.SceneManagement;

public class FixedAspectRatio : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float targetAspect = 16f / 9f;

    private Camera _camera;
    private RectTransform _leftBar;
    private RectTransform _rightBar;
    private RectTransform _topBar;
    private RectTransform _bottomBar;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<FixedAspectRatio>()) return;

        GameObject aspectRatio = new("FixedAspectRatio");
        aspectRatio.AddComponent<FixedAspectRatio>();
        DontDestroyOnLoad(aspectRatio);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshCamera();
        Apply();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        if (Screen.width == _lastScreenWidth && Screen.height == _lastScreenHeight) return;

        Apply();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshCamera();
        Apply();
    }

    private void RefreshCamera()
    {
        _camera = Camera.main;
    }

    private void Apply()
    {
        if (!_camera) RefreshCamera();
        if (!_camera || Screen.width <= 0 || Screen.height <= 0) return;

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        float windowAspect = (float)Screen.width / Screen.height;
        Rect rect = new(0f, 0f, 1f, 1f);

        if (windowAspect > targetAspect)
        {
            float width = targetAspect / windowAspect;
            rect.x = (1f - width) * 0.5f;
            rect.width = width;
        }
        else if (windowAspect < targetAspect)
        {
            float height = windowAspect / targetAspect;
            rect.y = (1f - height) * 0.5f;
            rect.height = height;
        }

        _camera.rect = rect;
        ApplyBlackBars(rect);
    }

    private void ApplyBlackBars(Rect gameRect)
    {
        EnsureBars();

        SetBar(_leftBar, 0f, 0f, gameRect.x, 1f);
        SetBar(_rightBar, gameRect.xMax, 0f, 1f - gameRect.xMax, 1f);
        SetBar(_bottomBar, 0f, 0f, 1f, gameRect.y);
        SetBar(_topBar, 0f, gameRect.yMax, 1f, 1f - gameRect.yMax);
    }

    private void EnsureBars()
    {
        if (_leftBar) return;

        GameObject canvasObject = new("AspectRatioBars");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        _leftBar = CreateBar(canvasObject.transform, "Left");
        _rightBar = CreateBar(canvasObject.transform, "Right");
        _topBar = CreateBar(canvasObject.transform, "Top");
        _bottomBar = CreateBar(canvasObject.transform, "Bottom");
    }

    private static RectTransform CreateBar(Transform parent, string name)
    {
        GameObject bar = new(name);
        bar.transform.SetParent(parent, false);
        UnityEngine.UI.Image image = bar.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = false;
        return image.rectTransform;
    }

    private static void SetBar(RectTransform bar, float x, float y, float width, float height)
    {
        bool visible = width > 0f && height > 0f;
        bar.gameObject.SetActive(visible);
        if (!visible) return;

        bar.anchorMin = new Vector2(x, y);
        bar.anchorMax = new Vector2(x + width, y + height);
        bar.offsetMin = Vector2.zero;
        bar.offsetMax = Vector2.zero;
    }
}
