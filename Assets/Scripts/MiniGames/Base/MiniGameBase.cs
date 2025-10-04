using UnityEngine;

namespace MiniGames.Base
{
    public class MiniGameBase : MonoBehaviour
    {
        public bool inFocus;

        protected void GameWin()
        {
            //Send win info
            print("Game Win!");
        }

        protected void GameLose()
        {
            //Send lose info
        }

        protected float RandomizeValues(float minValue, float maxValue)
        {
            float newValue = Random.Range(minValue, maxValue);
            return newValue;
        }
    }
}
