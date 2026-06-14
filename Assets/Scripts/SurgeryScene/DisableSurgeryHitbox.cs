using System;
using System.Collections;
using UnityEngine;

namespace SurgeryScene
{
    public class DisableSurgeryHitbox : MonoBehaviour
    {
        [SerializeField] private Collider2D collider1;
        [SerializeField] private Collider2D collider2;
        [SerializeField] private Collider2D collider3;

        [Header("CRT Slide")]
        [SerializeField] private Transform animatedRoot;
        [SerializeField] private Vector3 closedLocalOffset = new(0f, -5f, 0f);
        [SerializeField, Min(0f)] private float slideDuration = 0.18f;

        private Coroutine slideRoutine;
        private Vector3 openLocalPosition;
        private bool hasOpenLocalPosition;
        
        private void OnEnable()
        {
            CacheOpenPosition();
            SetAnimatedPosition(openLocalPosition + closedLocalOffset);
            SetColliderEnabled(collider1, false, nameof(collider1));
            SetColliderEnabled(collider2, false, nameof(collider2));
            SetColliderEnabled(collider3, false, nameof(collider3));
            PlaySlide(openLocalPosition, null);
        }

        private void OnDisable()
        {
            if (slideRoutine != null)
            {
                StopCoroutine(slideRoutine);
                slideRoutine = null;
            }

            if (hasOpenLocalPosition) SetAnimatedPosition(openLocalPosition);

            SetColliderEnabled(collider1, true, nameof(collider1));
            SetColliderEnabled(collider2, true, nameof(collider2));
            SetColliderEnabled(collider3, true, nameof(collider3));
        }

        public void PlayCloseAnimation(Action onComplete)
        {
            CacheOpenPosition();
            PlaySlide(openLocalPosition + closedLocalOffset, onComplete);
        }

        private void PlaySlide(Vector3 targetLocalPosition, Action onComplete)
        {
            if (slideRoutine != null) StopCoroutine(slideRoutine);
            slideRoutine = StartCoroutine(SlideRoutine(targetLocalPosition, onComplete));
        }

        private IEnumerator SlideRoutine(Vector3 targetLocalPosition, Action onComplete)
        {
            Transform target = AnimatedRoot;
            Vector3 startLocalPosition = target.localPosition;
            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = slideDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / slideDuration);
                progress = Mathf.SmoothStep(0f, 1f, progress);
                target.localPosition = Vector3.LerpUnclamped(startLocalPosition, targetLocalPosition, progress);
                yield return null;
            }

            target.localPosition = targetLocalPosition;
            slideRoutine = null;
            onComplete?.Invoke();
        }

        private void CacheOpenPosition()
        {
            if (hasOpenLocalPosition) return;

            openLocalPosition = AnimatedRoot.localPosition;
            hasOpenLocalPosition = true;
        }

        private void SetAnimatedPosition(Vector3 localPosition)
        {
            AnimatedRoot.localPosition = localPosition;
        }

        private Transform AnimatedRoot => animatedRoot ? animatedRoot : transform;

        private void SetColliderEnabled(Collider2D targetCollider, bool enabled, string fieldName)
        {
            if (targetCollider)
            {
                targetCollider.enabled = enabled;
                return;
            }

            if (!enabled)
            {
                Debug.LogError($"{nameof(DisableSurgeryHitbox)} is missing {fieldName}.", this);
            }
        }
    }
}
