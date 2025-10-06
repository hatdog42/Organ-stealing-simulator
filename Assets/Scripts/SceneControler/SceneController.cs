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
    private int _loopsDone = 0;
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

    public void LoadNextOrLoop()
    {
        if (_loopsDone < 3)
        {
            _loopsDone++;
            LoadScene("ChosePatient");
        }
        else
        {
            LoadScene("SanetyChek");
        }
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
    public void OnBedButtonPressed()
    {
        var healthBars = HealthBars.Instance;
        if (healthBars == null)
        {
            Debug.LogWarning("HealthBars instance not found!");
            return;
        }
        
        if (healthBars.CurrentFamilyState() == HealthBars.FamilyState.Broken)
        {
            LoadScene("DevorceEnding");
        }
        else
        {
            LoadScene("ChosePatient");
        }
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
