using System;
using System.Collections;
using MiniGames.Base;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MiniGames.SubGames.GiveAir
{
    public class GiveAir : MiniGameBase
    {
        [Header("Air")]
        [SerializeField] private float airTimer = 30f;
        [SerializeField] private float airLoss = 1f;
        [SerializeField] private float airGain = 2f;
        private float _currentTimer;
        
        [Header("Button"), SerializeField] private Collider2D airButton;
        
        [Header("ScaleBar"),SerializeField] private GameObject airScaleBar;
        private SpriteRenderer _fillBar;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;

        private void Start()
        {
            _currentTimer = airTimer;

            _fillBar = airScaleBar.GetComponent<SpriteRenderer>();
            _originalScale = _fillBar.transform.localScale;
            _originalPosition = _fillBar.transform.localPosition;
        }

        private void AirController()
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            if (Mouse.current.leftButton.isPressed && hit.collider == airButton)
            {
                if (_currentTimer > airTimer) return;
                _currentTimer += airGain * Time.deltaTime;
                DisplayWarning(false);
            }
            else
            {
                _currentTimer -= airLoss * Time.deltaTime;
                if (_currentTimer < airTimer * 0.3f)
                {
                    DisplayWarning(true);
                }
                else
                {
                    DisplayWarning(false);
                }
                
            }
            print(_currentTimer);

            if (_currentTimer < 0)
            {
                GameLose();
            }
        }

        private void UpdateFillBar()
        {
            float fill = Mathf.Clamp01(_currentTimer / airTimer);

            // Update scale
            _fillBar.transform.localScale = new Vector3(_originalScale.x * fill, _originalScale.y, _originalScale.z);

            // Adjust position so the left side stays fixed
            float offset = (_originalScale.x - _fillBar.transform.localScale.x) / 2f;
            _fillBar.transform.localPosition = new Vector3(_originalPosition.x - offset, _originalPosition.y, _originalPosition.z);
        }

        private void Update()
        {
            AirController();
            UpdateFillBar();
        }
    }
}
