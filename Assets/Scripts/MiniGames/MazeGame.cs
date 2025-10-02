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
        
        private Rigidbody2D _ballRigidbody;
        [SerializeField] private float _ballSpeed = 5f;
        [FormerlySerializedAs("_ballMaxSpeed")] [SerializeField] private float _ballMaxVelocity = 10f;

        private void Start()
        {
            _ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }
        
        private void MoveBall()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            
            worldPos.z = 0;
            
            Vector2 direction = worldPos - ball.transform.position;

            _ballRigidbody.linearVelocity = direction * _ballSpeed;
            if (_ballRigidbody.linearVelocity.magnitude > _ballMaxVelocity)
            {
                _ballRigidbody.linearVelocity = _ballRigidbody.linearVelocity.normalized * _ballMaxVelocity;
            }

            //_ballRigidbody.MovePosition(Vector2.Lerp(_ballRigidbody.position, worldPos, 0.2f));
        }
        
        private void FixedUpdate()
        {
            MoveBall();
        }
    }
}

