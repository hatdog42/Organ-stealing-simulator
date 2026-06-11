using MiniGames.Base;
using UnityEngine;

namespace MiniGames
{
    public class DebugButtonMiniGame : MiniGameBase
    {
        [SerializeField] private Collider2D winButton;
        [SerializeField] private Collider2D loseButton;

        private bool _finished;

        private void Start()
        {
            EnsureButtonColliders();
        }

        public override void OnFocusGained(TVInputRelay relay)
        {
            base.OnFocusGained(relay);
            EnsureButtonColliders();
            _finished = false;
            if (inputRelay != null)
            {
                inputRelay.PointerDown += HandlePointerPress;
                inputRelay.PointerUp += HandlePointerPress;
            }
        }

        public override void OnFocusLost()
        {
            if (inputRelay != null)
            {
                inputRelay.PointerDown -= HandlePointerPress;
                inputRelay.PointerUp -= HandlePointerPress;
            }
            base.OnFocusLost();
        }

        private void HandlePointerPress(Vector3 worldPos)
        {
            if (_finished) return;

            if (IsButtonHit(winButton, worldPos))
            {
                Finish(true);
            }
            else if (IsButtonHit(loseButton, worldPos))
            {
                Finish(false);
            }
        }

        private void EnsureButtonColliders()
        {
            winButton = FindButtonCollider(winButton, "WinButton");
            loseButton = FindButtonCollider(loseButton, "LooseButton");
        }

        private bool IsButtonHit(Collider2D buttonCollider, Vector3 worldPos)
        {
            return buttonCollider &&
                   buttonCollider.enabled &&
                   buttonCollider.gameObject.activeInHierarchy &&
                   buttonCollider.OverlapPoint(worldPos);
        }

        private Collider2D FindButtonCollider(Collider2D existingCollider, string buttonName)
        {
            if (existingCollider) return existingCollider;

            Transform button = transform.Find(buttonName);
            if (!button)
            {
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == buttonName)
                    {
                        button = child;
                        break;
                    }
                }
            }

            if (!button)
            {
                Debug.LogError($"DebugButtonMiniGame on '{name}' could not find child '{buttonName}'.");
                return null;
            }

            Collider2D buttonCollider = button.GetComponent<Collider2D>();
            if (!buttonCollider)
            {
                Debug.LogError($"Debug button '{buttonName}' needs a Collider2D component.");
                return null;
            }

            return buttonCollider;
        }

        private void Finish(bool won)
        {
            if (_finished) return;

            _finished = true;

            if (won) GameWin();
            else GameLose();
        }
    }
}
