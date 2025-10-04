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
            //Send win info
            print("Game Win!");
        }

        protected void GameLose()
        {
            //Send lose info
        }

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
