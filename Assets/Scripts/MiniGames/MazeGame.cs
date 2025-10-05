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
        
        private SpawnRandomMaze _spawnRandomMaze;

        private void Start()
        {
            _ballCollider = ball.GetComponent<Collider2D>();
            _ballRigidbody = ball.GetComponent<Rigidbody2D>();
            _spawnRandomMaze = gameObject.GetComponent<SpawnRandomMaze>();
            
            _spawnRandomMaze.SpawnMaze();
        }
        
        private void MoveBall(Vector3 worldPos)
        {
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
            if (_gameWon || !InFocus) return;
            if (!Mouse.current.leftButton.isPressed) return;
            
            var screen = Mouse.current.position.ReadValue();

            if (inputRelay.TryMapScreenToMiniWorld(screen, out Vector3 miniWorldPos))
            {
                MoveBall(miniWorldPos);
            }

        }
    }
}

