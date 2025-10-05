using System;
using System.Collections;
using MiniGames.Base;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace MiniGames.SubGames.CatchTheBall
{
    public class CatchTheDrop : MiniGameBase
    {
        //Moving Cup
        [Header("Cup"), SerializeField] private GameObject cup;
        [SerializeField] private float cupSpeed = 50f;
        private Rigidbody2D _cupRb;
        private Collider2D _cupCol;
        
        //Drops
        [Header("Drop"), SerializeField] private GameObject drop;
        
        //Drop Spawners
        [Header("Spawn Positions")]
        [SerializeField] private Transform dropPosMin;
        [SerializeField] private Transform dropPosMax;
        private float _newDropPos;
        private int _outsideDropCount;
        
        [Header("ChangePos Timer")]
        [SerializeField] private float posChangeTimeMin;
        [SerializeField] private float posChangeTimeMax;
        private float _changeDropPosTimer;
        
        //Timer
        [Header("Drop Timer"), SerializeField] private float dropTimer;
        [SerializeField] private int maxDropCount = 10;

        private void Start()
        {
            _cupRb = cup.GetComponent<Rigidbody2D>();
            _cupCol = cup.GetComponent<Collider2D>();
            StartCoroutine(ChangeDropPos());
            StartCoroutine(WaitForDrop());
        }

        private void MoveCup()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            
            worldPos.z = 0;
            
            Vector2 direction = worldPos - cup.transform.position;

            _cupRb.linearVelocity = direction * cupSpeed;
            _cupRb.MovePosition(Vector2.Lerp(_cupRb.position, worldPos, 0.2f));
        }

        private void FixedUpdate()
        {
            if (!InFocus) return;
            if (Mouse.current.leftButton.isPressed) MoveCup();
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

        public void OutsideDropCount()
        {
            _outsideDropCount++;
            if (_outsideDropCount < maxDropCount)
            {
                DisplayWarning(true);
            }
            else
            {
                GameLose();
            }
        }

        public void ResetOutsideDrops()
        {
            _outsideDropCount = 0;
            DisplayWarning(false);
        }
    }
}
