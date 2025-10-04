using UnityEngine;

namespace MiniGames.Base
{
    public class MiniGameBase : MonoBehaviour
    {
        public bool inFocus;
        [SerializeField] protected Camera cam;

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
