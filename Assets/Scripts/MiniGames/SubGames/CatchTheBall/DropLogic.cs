using System;
using UnityEngine;

namespace MiniGames.SubGames.CatchTheBall
{
    public class DropLogic : MonoBehaviour
    {
        private Collider2D _col;

        private void Start()
        {
            _col = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Cup"))
            {
                Destroy(gameObject);
            }
        }
    }
}