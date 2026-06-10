using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MiniGames.Base
{
    public class MiniGameBase : MonoBehaviour
    {
        protected bool InFocus {get; private set;}
        [SerializeField] protected Camera cam;
        protected TVInputRelay inputRelay;

        [Header("Warning Sprites"), SerializeField] private GameObject warningFaceScreen;
        [SerializeField] private GameObject warningFaceOutside;
        
        [Header("Audio"), SerializeField] private AudioSource warningAudio;

        private void Awake()
        {
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
            HealthBars.Instance.ChangePsych(5);
            HealthBars.Instance.ChangeReputation(5);
            SceneController.Instance.LoadNextOrLoop();
        }

        protected void GameLose()
        {
            HealthBars.Instance.ChangeReputation(-10);
            SceneController.Instance.LoadScene("OrganSteeling");
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
                    warningAudio.Play();
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
    }
}
