using System;
using UnityEngine;

namespace MiniGames.SubGames.CatchTheBall
{
    public class DropFallOutside : MonoBehaviour
    {
        private CatchTheDrop _catchTheDrop;

        private void Start()
        {
            _catchTheDrop = GameObject.Find("CatchTheDrop").GetComponent<CatchTheDrop>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Drop"))
            {
                Destroy(other.gameObject);
                _catchTheDrop.OutsideDropCount();
            }
        }
    }
}
