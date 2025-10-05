using UnityEngine;

namespace MiniGames.Base
{
    public class MiniGameBase : MonoBehaviour
    {
        protected bool InFocus {get; private set;}
        [SerializeField] protected Camera cam;
        protected TVInputRelay inputRelay;
        
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
            SceneController.Instance.LoadNextOrLoop();
        }

        protected void GameLose()
        {
            SceneController.Instance.LoadScene("OrganSteeling");
        }

        //This will display a warning from the sub mini-games when it needs your attention
        protected void DisplayWarning(bool warning) 
        {
            if (warning)
            {
                print("Warning!");
            }
            
        }

        protected float RandomizeValues(float minValue, float maxValue)
        {
            float newValue = Random.Range(minValue, maxValue);
            return newValue;
        }
    }
}
