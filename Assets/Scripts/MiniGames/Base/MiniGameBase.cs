using UnityEngine;
using Random = UnityEngine.Random;

namespace MiniGames.Base
{
    public class MiniGameBase : MonoBehaviour
    {
        protected bool InFocus {get; private set;}
        [SerializeField] protected Camera cam;
        protected TVInputRelay inputRelay;
        private bool _finished;

        [Header("Warning Sprites"), SerializeField] private GameObject warningFaceScreen;
        [SerializeField] private GameObject warningFaceOutside;
        
        [Header("Audio"), SerializeField] private SoundId warningSound = SoundId.Alarm;
        [SerializeField, Range(0f, 1f)] private float warningAudioVolume = 0.2f;
        private AudioSource warningAudio;

        protected virtual void Awake()
        {
            SetupWarningAudio();
            DisplayWarning(false);
        }

        public virtual void OnFocusGained(TVInputRelay relay)
        {
            InFocus = true;
            inputRelay = relay; // Store so Update/FixedUpdate can query mouse-through-CRT
        }
        public virtual void OnFocusLost()
        {
            InFocus = false;
            inputRelay = null;
        }

        protected void GameWin()
        {
            if (_finished) return;
            _finished = true;

            HealthBars.Instance.ApplySavedPatient();
            SceneController.Instance.LoadNextOrLoop();
        }

        protected void GameLose()
        {
            if (_finished) return;
            _finished = true;

            SceneController.Instance.LoadScene("OrganSteeling");
        }

        public void ForceGameLose()
        {
            GameLose();
        }

        //This will display a warning from the sub mini-games when it needs your attention
        protected void DisplayWarning(bool warning) 
        {
            if (warning)
            {
                //print("Warning");
                if (warningFaceScreen) warningFaceScreen.SetActive(true);
                if (warningFaceOutside) warningFaceOutside.SetActive(true);

                if (warningAudio && !warningAudio.isPlaying)
                {
                    SetupWarningAudio();
                    warningAudio.Play();
                }
                else if (!warningAudio)
                {
                    SetupWarningAudio();
                    warningAudio?.Play();
                }
            }
            else
            {
                if (warningFaceScreen) warningFaceScreen.SetActive(false);
                if (warningFaceOutside) warningFaceOutside.SetActive(false);
                if (warningAudio) warningAudio.Stop();
            }
            
        }

        protected float RandomizeValues(float minValue, float maxValue)
        {
            float newValue = Random.Range(minValue, maxValue);
            return newValue;
        }

        private void SetupWarningAudio()
        {
            if (warningAudio || warningSound == SoundId.None || !AudioManager.Instance) return;

            warningAudio = AudioManager.Instance.CreateSfxSource(
                warningSound,
                transform,
                loop: true,
                baseVolume: warningAudioVolume);
        }
    }
}
