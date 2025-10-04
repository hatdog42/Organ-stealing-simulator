using System;
using UnityEngine;

namespace MiniGames.SubGames.CatchTheBall
{
    public class DropLogic : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Drop"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}