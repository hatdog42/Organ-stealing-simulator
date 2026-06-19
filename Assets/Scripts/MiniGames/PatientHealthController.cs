using System.Collections;
using MiniGames.Base;
using UnityEngine;

namespace MiniGames
{
    public class PatientHealthController : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float recoveryPerSecond = 1.5f;
        [SerializeField, Min(0f)] private float dropDamage = 12f;

        [Header("Difficulty Scaling")]
        [SerializeField, Min(0f)] private float difficultyIncreasePerMinute = 0.08f;
        [SerializeField, Min(1f)] private float maxDifficultyMultiplier = 2f;

        [Header("Oxygen Damage")]
        [SerializeField, Min(0f)] private float oxygenDamageMinPerSecond = 2f;
        [SerializeField, Min(0f)] private float oxygenDamageMaxPerSecond = 18f;
        [SerializeField, Min(0f)] private float oxygenReportTimeout = 0.25f;

        [Header("Heartbeat")]
        [SerializeField] private SoundId heartBeepSound = SoundId.HeartBeep;
        [SerializeField] private SoundId flatlineSound = SoundId.Flatline;
        [SerializeField, Range(0f, 1f)] private float heartBeepVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float flatlineVolume = 1f;
        [SerializeField, Min(0.05f)] private float fastestHeartBeepInterval = 0.25f;
        [SerializeField, Min(0.05f)] private float slowestHeartBeepInterval = 1.4f;
        [SerializeField, Range(0.5f, 1f)] private float flatlineHoldBeforeLose = 0.75f;
        [SerializeField, Min(0f)] private float flatlineFadeOutDuration = 1.2f;

        private float _currentHealth;
        private float _startTime;
        private float _oxygenNormalized = 1f;
        private float _oxygenAlarmThreshold = 0.3f;
        private float _lastOxygenReportTime = -999f;
        private float _nextHeartBeepTime;
        private AudioSource _flatlineSource;
        private bool _failed;
        private bool _flatlinePausedByPause;

        public static PatientHealthController Instance { get; private set; }
        public float Health01 => Mathf.Clamp01(_currentHealth / maxHealth);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _currentHealth = maxHealth;
            _startTime = Time.time;
            _nextHeartBeepTime = Time.unscaledTime + slowestHeartBeepInterval;
            CreateFlatlineSource();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            PauseMenueControler.PauseChanged += OnPauseChanged;
            if (PauseMenueControler.IsPaused) OnPauseChanged(true);
        }

        private void OnDisable()
        {
            PauseMenueControler.PauseChanged -= OnPauseChanged;
        }

        private void Update()
        {
            if (_failed) return;

            RecoverHealth();
            ApplyOxygenDamage();
            PlayHeartbeat();
        }

        public void ReportOxygen(float oxygenNormalized, float alarmThreshold)
        {
            _oxygenNormalized = Mathf.Clamp01(oxygenNormalized);
            _oxygenAlarmThreshold = Mathf.Clamp01(alarmThreshold);
            _lastOxygenReportTime = Time.unscaledTime;
        }

        public void ApplyDropDamage()
        {
            ApplyDamage(dropDamage);
        }

        private void RecoverHealth()
        {
            if (_currentHealth >= maxHealth) return;

            _currentHealth = Mathf.Min(maxHealth, _currentHealth + GetScaledRecoveryPerSecond() * Time.deltaTime);
        }

        private void ApplyOxygenDamage()
        {
            if (Time.unscaledTime - _lastOxygenReportTime > oxygenReportTimeout) return;
            if (_oxygenNormalized >= _oxygenAlarmThreshold) return;
            if (_oxygenAlarmThreshold <= 0f) return;

            float severity = 1f - Mathf.Clamp01(_oxygenNormalized / _oxygenAlarmThreshold);
            float damagePerSecond = Mathf.Lerp(
                oxygenDamageMinPerSecond,
                oxygenDamageMaxPerSecond,
                severity * severity);

            ApplyDamage(damagePerSecond * Time.deltaTime);
        }

        private void ApplyDamage(float amount)
        {
            if (_failed || amount <= 0f) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount * DifficultyMultiplier);
            if (_currentHealth > 0f) return;

            FailPatient();
        }

        private void PlayHeartbeat()
        {
            if (PauseMenueControler.IsPaused)
            {
                _nextHeartBeepTime = Mathf.Max(_nextHeartBeepTime, Time.unscaledTime + 0.05f);
                return;
            }

            if (heartBeepSound == SoundId.None || Time.unscaledTime < _nextHeartBeepTime) return;

            AudioManager.Instance?.PlaySfx(heartBeepSound, heartBeepVolume);

            float interval = Mathf.Lerp(fastestHeartBeepInterval, slowestHeartBeepInterval, Health01);
            _nextHeartBeepTime = Time.unscaledTime + interval;
        }

        private void FailPatient()
        {
            _failed = true;
            StartCoroutine(FlatlineThenLoseRoutine());
        }

        private IEnumerator FlatlineThenLoseRoutine()
        {
            PlayFlatline();

            yield return WaitForUnpausedSeconds(flatlineHoldBeforeLose);

            TriggerGameLose();

            float startVolume = _flatlineSource ? _flatlineSource.volume : 0f;
            float elapsed = 0f;

            while (_flatlineSource && elapsed < flatlineFadeOutDuration)
            {
                if (PauseMenueControler.IsPaused)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.unscaledDeltaTime;
                float progress = flatlineFadeOutDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / flatlineFadeOutDuration);
                _flatlineSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }

            if (_flatlineSource)
            {
                _flatlineSource.Stop();
            }
        }

        private void TriggerGameLose()
        {
            MiniGameBase miniGame = FindAnyObjectByType<MiniGameBase>(FindObjectsInactive.Include);
            if (miniGame)
            {
                miniGame.ForceGameLose();
                return;
            }

            SceneController.Instance?.LoadScene("OrganSteeling");
        }

        private float DifficultyMultiplier
        {
            get
            {
                float minutesPlayed = Mathf.Max(0f, Time.time - _startTime) / 60f;
                float multiplier = 1f + minutesPlayed * difficultyIncreasePerMinute;
                return Mathf.Min(maxDifficultyMultiplier, multiplier);
            }
        }

        private float GetScaledRecoveryPerSecond()
        {
            return recoveryPerSecond / DifficultyMultiplier;
        }

        private void CreateFlatlineSource()
        {
            if (_flatlineSource || flatlineSound == SoundId.None || !AudioManager.Instance) return;

            _flatlineSource = AudioManager.Instance.CreateSfxSource(
                flatlineSound,
                transform,
                loop: true,
                baseVolume: flatlineVolume);
        }

        private void PlayFlatline()
        {
            if (!_flatlineSource) CreateFlatlineSource();
            if (!_flatlineSource || flatlineSound == SoundId.None) return;

            AudioManager.Instance?.ConfigureSfxSource(
                _flatlineSource,
                flatlineSound,
                flatlineVolume,
                loop: true);
            _flatlineSource.Play();
        }

        private static IEnumerator WaitForUnpausedSeconds(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!PauseMenueControler.IsPaused)
                {
                    elapsed += Time.unscaledDeltaTime;
                }

                yield return null;
            }
        }

        private void OnPauseChanged(bool paused)
        {
            if (!_flatlineSource) return;

            if (paused)
            {
                _flatlinePausedByPause = _flatlineSource.isPlaying;
                if (_flatlinePausedByPause) _flatlineSource.Pause();
                return;
            }

            if (!_flatlinePausedByPause) return;

            _flatlineSource.UnPause();
            _flatlinePausedByPause = false;
        }
    }
}
