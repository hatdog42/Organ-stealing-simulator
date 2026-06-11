using UnityEngine;

namespace SurgeryScene
{
    public class DisableSurgeryHitbox : MonoBehaviour
    {
        [SerializeField] private Collider2D collider1;
        [SerializeField] private Collider2D collider2;
        [SerializeField] private Collider2D collider3;
        
        private void OnEnable()
        {
            SetColliderEnabled(collider1, false, nameof(collider1));
            SetColliderEnabled(collider2, false, nameof(collider2));
            SetColliderEnabled(collider3, false, nameof(collider3));
        }

        private void OnDisable()
        {
            SetColliderEnabled(collider1, true, nameof(collider1));
            SetColliderEnabled(collider2, true, nameof(collider2));
            SetColliderEnabled(collider3, true, nameof(collider3));
        }

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
