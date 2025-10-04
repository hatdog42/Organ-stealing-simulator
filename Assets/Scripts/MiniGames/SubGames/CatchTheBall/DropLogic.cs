using System;
using UnityEngine;

namespace MiniGames.SubGames.CatchTheBall
{
    public class DropLogic : MonoBehaviour
    {
        [SerializeField] private Collider2D cupCol;

        private Collider2D _col;

        private void Start()
        {
            _col = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (_col.IsTouching(cupCol))
            {
                Destroy(this.gameObject);
            }
        }
    }
}