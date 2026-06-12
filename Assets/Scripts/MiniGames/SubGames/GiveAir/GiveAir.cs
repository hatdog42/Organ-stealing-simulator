using MiniGames.Base;
using UnityEngine;

namespace MiniGames.SubGames.GiveAir
{
    public class GiveAir : MiniGameBase
    {
        [Header("Air")]
        [SerializeField] private float airTimer = 30f;
        [SerializeField] private float airLoss = 1f;
        [SerializeField] private float airGain = 2f;
        [SerializeField, Range(0f, 1f)] private float alarmThreshold = 0.3f;
        private float _currentTimer;
        
        [Header("Button"), SerializeField] private Collider2D airButton;
        
        [Header("ScaleBar"),SerializeField] private GameObject airScaleBar;
        private SpriteRenderer _fillBar;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        
        private bool _pressingAir;

        private void Start()
        {
            _currentTimer = airTimer;

            _fillBar = airScaleBar.GetComponent<SpriteRenderer>();
            _originalScale = _fillBar.transform.localScale;
            _originalPosition = _fillBar.transform.localPosition;
        }

        public override void OnFocusGained(TVInputRelay relay)
        {
            base.OnFocusGained(relay);
            if (inputRelay != null)
            {
                inputRelay.PointerDown += OnPointerDown;
                inputRelay.PointerDrag += OnPointerDrag;
                inputRelay.PointerUp   += OnPointerUp;
            }
        }
        public override void OnFocusLost()
        {
            if (inputRelay != null)
            {
                inputRelay.PointerDown -= OnPointerDown;
                inputRelay.PointerDrag -= OnPointerDrag;
                inputRelay.PointerUp   -= OnPointerUp;
            }
            _pressingAir = false;
            base.OnFocusLost();
        }
        private void OnPointerDown(Vector3 miniWorld)
        {
            _pressingAir = HitAirButton(miniWorld);
        }

        private void OnPointerDrag(Vector3 miniWorld)
        {
            // keep updating if the pointer stays over / leaves the button
            _pressingAir = HitAirButton(miniWorld);
        }
        private bool HitAirButton(Vector3 miniWorld)
        {
            if (!airButton) return false;
            
            if (airButton.OverlapPoint(miniWorld)) return true;
 
            return false;
        }
        private void OnPointerUp(Vector3 miniWorld)
        {
            _pressingAir = false;
        }
        
        private void AirController()
        {
            if (_pressingAir)
            {
                _currentTimer = Mathf.Min(airTimer, _currentTimer + airGain * Time.deltaTime);
            }
            else
            {
                _currentTimer = Mathf.Max(0f, _currentTimer - airLoss * Time.deltaTime);
            }

            float oxygenNormalized = Mathf.Clamp01(_currentTimer / airTimer);
            bool oxygenAlarm = oxygenNormalized < alarmThreshold;
            DisplayWarning(oxygenAlarm);
            PatientHealthController.Instance?.ReportOxygen(oxygenNormalized, alarmThreshold);
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
