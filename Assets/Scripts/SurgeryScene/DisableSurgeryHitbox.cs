using System;
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
            collider1.enabled = false;
            collider2.enabled = false;
            collider3.enabled = false;
        }

        private void OnDisable()
        {
            collider1.enabled = true;
            collider2.enabled = true;
            collider3.enabled = true;
        }
    }
}
