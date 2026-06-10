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
            winButton = FindOrCreateButtonCollider(winButton, "WinButton");
            loseButton = FindOrCreateButtonCollider(loseButton, "LooseButton");
        }

        public override void OnFocusGained(TVInputRelay relay)
        {
            base.OnFocusGained(relay);
            if (inputRelay != null) inputRelay.PointerUp += HandlePointerUp;
        }

        public override void OnFocusLost()
        {
            if (inputRelay != null) inputRelay.PointerUp -= HandlePointerUp;
            base.OnFocusLost();
        }

        private void HandlePointerUp(Vector3 worldPos)
        {
            if (_finished) return;

            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (!hit) return;

            if (hit == winButton)
            {
                Finish(true);
            }
            else if (hit == loseButton)
            {
                Finish(false);
            }
        }

        private Collider2D FindOrCreateButtonCollider(Collider2D existingCollider, string buttonName)
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

            if (!button) return null;

            Collider2D buttonCollider = button.GetComponent<Collider2D>();
            if (!buttonCollider) buttonCollider = button.gameObject.AddComponent<CircleCollider2D>();

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
