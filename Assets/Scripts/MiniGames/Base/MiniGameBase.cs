using UnityEngine;

namespace MiniGames.Base
{
    public class MiniGameBase : MonoBehaviour
    {
        //TODO - Make a time limit.

        protected void GameWin()
        {
            //Send win info
            print("Game Win!");
        }

        protected void GameLose()
        {
            //Send lose info
        }
    }
}
