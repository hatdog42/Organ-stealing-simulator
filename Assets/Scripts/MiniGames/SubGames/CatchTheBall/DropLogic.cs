using System;
using UnityEngine;

namespace MiniGames.SubGames.CatchTheBall
{
    public class DropLogic : MonoBehaviour
    {
        [SerializeField] private CatchTheDrop catchTheDrop;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Drop"))
            {
                Destroy(other.gameObject);
                catchTheDrop.ResetOutsideDrops();
            }
        }
    }
}