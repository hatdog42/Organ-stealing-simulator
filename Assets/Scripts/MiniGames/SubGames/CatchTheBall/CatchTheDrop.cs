using System.Collections;
using MiniGames.Base;
using UnityEngine;

namespace MiniGames.SubGames.CatchTheBall
{
    public class CatchTheDrop : MiniGameBase
    {
        //Moving Cup
        [Header("Cup"), SerializeField] private GameObject cup;
        private Rigidbody2D _cupRb;
        private Collider2D _cupCol;
        
        //Drops
        [Header("Drop"), SerializeField] private GameObject drop;
        
        //Drop Spawners
        [Header("Spawn Positions")]
        [SerializeField] private Transform dropPosMin;
        [SerializeField] private Transform dropPosMax;
        private float _newDropPos;
        
        [Header("ChangePos Timer")]
        [SerializeField] private float posChangeTimeMin;
        [SerializeField] private float posChangeTimeMax;
        private float _changeDropPosTimer;
        
        //Timer
        [Header("Drop Timer"), SerializeField] private float dropTimer;

        private void Start()
        {
            _cupRb = cup.GetComponent<Rigidbody2D>();
            _cupCol = cup.GetComponent<Collider2D>();
            StartCoroutine(ChangeDropPos());
            StartCoroutine(WaitForDrop());
        }

        private IEnumerator ChangeDropPos()
        {
            while (true)
            {
                _newDropPos = Random.Range(dropPosMin.position.x, dropPosMax.position.x);
                _changeDropPosTimer = RandomizeValues(posChangeTimeMin, posChangeTimeMax);
                yield return new WaitForSeconds(_changeDropPosTimer);
            }
        }

        private IEnumerator WaitForDrop()
        {
            while (true)
            {
                Instantiate(drop, new Vector3(_newDropPos, dropPosMax.position.y), Quaternion.identity);
                yield return new WaitForSeconds(dropTimer);
            }
        }
    }
}
