using System;
using MiniGames.Base;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MiniGames
{
    public class MazeGame : MiniGameBase
    {
        //The ball we will move through the maze
        [SerializeField] private GameObject ball;
        
        //Ball components
        private Collider2D _ballCollider;
        private Rigidbody2D _ballRigidbody;
        [SerializeField] private float _ballSpeed = 5f;
        [SerializeField] private float _ballMaxVelocity = 10f;
        
        //Goal
        [SerializeField] private Collider2D goalCol;
        private bool _gameWon;

        private void Start()
        {
            _ballCollider = ball.GetComponent<Collider2D>();
            _ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }
        
        private void MoveBall()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            
            worldPos.z = 0;
            
            Vector2 direction = worldPos - ball.transform.position;

            _ballRigidbody.linearVelocity = direction * _ballSpeed;
            if (_ballRigidbody.linearVelocity.magnitude > _ballMaxVelocity)
            {
                _ballRigidbody.linearVelocity = _ballRigidbody.linearVelocity.normalized * _ballMaxVelocity;
            }

            //_ballRigidbody.MovePosition(Vector2.Lerp(_ballRigidbody.position, worldPos, 0.2f));
        }

        private void Update()
        {
            if (goalCol.IsTouching(_ballCollider))
            {
                GameWin();
                _gameWon = true;
            }
        }

        private void FixedUpdate()
        {
            if (_gameWon || !inFocus) return;
            MoveBall();
        }
    }
}

