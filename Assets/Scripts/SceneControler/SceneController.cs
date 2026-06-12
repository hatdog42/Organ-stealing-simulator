using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private ScreenFader faderPrefab;
    [SerializeField, Min(0f)] private float fadeDuration = 1.2f;
    [SerializeField, Min(0)] private int framesToWaitBeforeFadeIn = 1;

    private const int MinimumSurgeriesBeforeSanetyCheck = 3;

    [Header("Loop Settings")]
    [SerializeField, Min(MinimumSurgeriesBeforeSanetyCheck)] private int surgeriesBeforeSanetyCheck = MinimumSurgeriesBeforeSanetyCheck;
    
    private ScreenFader _fader;
    private bool _isLoading;
    private int _surgeriesDoneThisCycle;
    public static SceneController Instance { get; private set; }
    public bool IsTransitioning => _isLoading;
    

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
        if (_isLoading) return;

        _surgeriesDoneThisCycle++;

        int requiredSurgeries = Mathf.Max(MinimumSurgeriesBeforeSanetyCheck, surgeriesBeforeSanetyCheck);
        if (_surgeriesDoneThisCycle >= requiredSurgeries)
        {
            _surgeriesDoneThisCycle = 0;
            LoadScene("SanetyChek");
            return;
        }

        LoadScene("ChosePatient");
    }

    public void StartPatientLoop()
    {
        _surgeriesDoneThisCycle = 0;
        LoadScene("DayCounter");
    }

    public void StartNewGame(string firstSceneName = "Exposition")
    {
        _surgeriesDoneThisCycle = 0;
        DayCounterMangager.ResetDayCount();
        if (HealthBars.Instance) HealthBars.Instance.ResetForNewGame();
        LoadScene(firstSceneName);
    }
    
    private void EnsureFader()
    {
        if (_fader) return;
        
        _fader = FindAnyObjectByType<ScreenFader>(FindObjectsInactive.Include);
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
            return;
        }

        healthBars.ApplyFamilyDinnerResult();

        if (healthBars.CurrentFamilyState() == HealthBars.FamilyState.Broken)
        {
            LoadScene("DevorceEnding");
            return;
        }

        StartPatientLoop();
    }

    public void Quit()
    {
        Application.Quit();
    }
    private IEnumerator LoadRoutine(string sceneName)
    {
        _isLoading = true;
        EnsureFader();
        
        _fader.gameObject.SetActive(true);
        
        yield return _fader.FadeOut(fadeDuration);

        yield return SceneManager.LoadSceneAsync(sceneName);
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < framesToWaitBeforeFadeIn; i++)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
        }
        
        yield return _fader.FadeIn(fadeDuration);
        _fader.gameObject.SetActive(false);
        
        _isLoading = false;
    }
}
