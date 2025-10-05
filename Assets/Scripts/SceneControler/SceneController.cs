using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private ScreenFader faderPrefab;
    [SerializeField] private float fadeDuration = 0.7f;
    
    private ScreenFader _fader;
    private bool _isLoading;
    public static SceneController Instance { get; private set; }
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        EnsureFader(); 
        _fader.gameObject.SetActive(false);
    }
    
    private void EnsureFader()
    {
        if (_fader) return;
        
        _fader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
        if (!_fader && faderPrefab) _fader = Instantiate(faderPrefab);
        
        if (_fader) DontDestroyOnLoad(_fader.gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (!_isLoading) StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        _isLoading = true;
        EnsureFader();
        
        _fader.gameObject.SetActive(true);
        
        yield return _fader.FadeOut(fadeDuration);
        
        yield return SceneManager.LoadSceneAsync(sceneName);
        
        yield return _fader.FadeIn(fadeDuration);
        _fader.gameObject.SetActive(false);
        
        _isLoading = false;
    }
}
