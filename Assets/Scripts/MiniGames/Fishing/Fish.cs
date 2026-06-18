using UnityEngine;
using UnityEngine.Serialization;

public class Fish : MonoBehaviour
{
    private enum FightMode
    {
        Fighting,
        Resting
    }

    [Header("Movement")]
    [SerializeField] private float _speed = 2f;
    private Vector2 _direction = Vector2.right;

    [Header("Passive Deflection")]
    [SerializeField, Range(0f, 1f)] private float _passiveDeflectChance = 0.5f;
    [SerializeField, Range(0f, 180f)] private float _maxPassiveDeflectAngle = 45f;

   
    [Header("Visibility / Depth")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float maxOpacity = 0.8f;
    [SerializeField, Range(0f, 1f)] private float _minAttractOpacity = 0.35f;
    [SerializeField, Range(0f, 1f)] private float _maxAttractOpacity = 0.75f;

    [Header("Attraction")]
    [SerializeField] private float _attractionRadius = 2f;
    [SerializeField] private float _attractionSpeed = 2.5f;
    [SerializeField] private float _hookDistance = 0.15f;
    [SerializeField] private float _turnSpeed = 720f;

    [Header("Hook Point")]
    [SerializeField] private Transform _hookPoint;

    [Header("Fight Force")]
    [SerializeField] private float _fightForce = 10f;
    [SerializeField] private float _restForceMultiplier = 0.35f;
    [SerializeField, Range(0f, 1f)] private float _minimumSideForce = 0.5f;
    [SerializeField] private float _directionChangeKickMultiplier = 1.25f;
    [SerializeField] private float _directionChangeKickDuration = 0.15f;

    [Header("Edge Escape")]
    [SerializeField, Min(0f)] private float _edgeAvoidanceDistance = 0.75f;
    [SerializeField, Min(1f)] private float _edgeBoostMultiplier = 1.5f;

    [Header("Fight Direction")]
    [SerializeField, Range(0f, 200f)] private float _fightArc = 180f;
    [SerializeField] private float _directionChangeSpeedMultiplier = 2.5f;

    [Header("Fight Rotation")]
    [SerializeField] private float _fightTurnSpeed = 540f;
    [SerializeField] private Vector2 _fightTurnDuration = new Vector2(0.08f, 0.25f);
    [SerializeField] private float _fightAngleReachedThreshold = 2f;
    [SerializeField] private Vector2 _fightWaitTime = new Vector2(0.25f, 0.8f);

    [Header("Stamina Cycle")]
    [SerializeField] private float _maxStamina = 1f;
    [SerializeField] private Vector2 _fightDuration = new Vector2(1.1f, 2.4f);
    [SerializeField] private Vector2 _restDuration = new Vector2(0.75f, 1.4f);
    [SerializeField] private Vector2 _fightStaminaUseRate = new Vector2(0.2f, 0.55f);
    [SerializeField] private Vector2 _fightIntensity = new Vector2(0.85f, 1.2f);
    [SerializeField] private float _staminaRecoveryRate = 0.65f;
    [SerializeField, Range(0f, 1f)] private float _minStaminaToFight = 0.5f;

    [Header("Counter Pull Effects")]
    [SerializeField] private float _counterPullStaminaDrain = 0.9f;
    [SerializeField, Range(0f, 1f)] private float _counterPullFightSlowMultiplier = 0.75f;
    [SerializeField] private float _counterPullSlowDuration = 0.18f;

    [Header("Landing")]
    [SerializeField, Min(0)] private int _exhaustionsBeforeLanding = 2;
    [SerializeField, Min(0f)] private float _landingPanicDistance = 1.5f;
    [SerializeField, Range(0f, 1f)] private float _landingPanicChance = 0.9f;
    [SerializeField, Min(0f)] private float _landingPanicRetryDelay = 0.2f;
    [SerializeField] private Vector2 _landingPanicDuration = new Vector2(0.8f, 1.5f);
    [SerializeField, Range(0f, 1f)] private float _landingPanicStaminaRestore = 0.65f;
    [SerializeField, Min(1f)] private float _landingPanicIntensityMultiplier = 1.6f;
    [SerializeField, Min(1f)] private float _landingPanicTensionMultiplier = 1.6f;
    [SerializeField, Min(0f)] private float _landingPanicCooldown = 0.75f;

    [Header("Escape")]
    [SerializeField] private float _escapeFadeDuration = 0.45f;
    [SerializeField] private float _escapeSinkSpeed = 0.6f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _idleAnimationState = "FishIdleAnimation";
    [SerializeField] private string _fightAnimationState = "FishFightAnimation";
    [SerializeField, Min(0f)] private float _restAnimationSpeed = 1f;
    [SerializeField, Min(0f)] private float _fightAnimationSpeed = 1.75f;

    [Header("Animation Audio")]
    [SerializeField] private SoundId _sloshBlobSfx = SoundId.SloshBlob;
    [SerializeField, Range(0f, 1f)] private float _sloshBlobVolume = 1f;
    [SerializeField, Min(0.01f)] private float _idleSloshPitch = 0.8f;
    [SerializeField, Min(0.01f)] private float _fightSloshPitch = 1.25f;
    private AudioSource _sloshBlobAudioSource;
    
    private SpriteRenderer _renderer;
    private float _timer;
    private float _currentOpacity;
    private FishPool _pool;
    private FishingRodController _rod;
    private bool _hooked;
    private Transform _hookTarget;
    private float _fightTimer;
    private float _targetFightAngle;
    private Quaternion _startFightRotation;
    private Quaternion _targetFightRotation;
    private float _fightTurnTimer;
    private float _activeFightTurnDuration;
    private bool _waitingAtFightAngle;
    private FightMode _fightMode = FightMode.Resting;
    private float _stamina;
    private float _fightModeTimer;
    private float _activeStaminaUseRate;
    private float _activeFightIntensity = 1f;
    private float _directionChangeKickTimer;
    private float _counterPullSlowTimer;
    private int _exhaustionCount;
    private float _landingPanicCooldownTimer;
    private bool _landingPanicActive;
    private bool _escaping;
    private float _escapeTimer;
    private float _activeEscapeFadeDuration;
    private float _activeEscapeSinkSpeed;
    private float _escapeStartOpacity;
    private bool _willPassiveDeflect;
    private bool _hasPassiveDeflected;
    private float _passiveDeflectTime;
    private int _idleAnimationStateHash;
    private int _fightAnimationStateHash;
    private int _currentAnimationStateHash;
    private float _currentAnimationSpeed = -1f;

    public bool IsFighting => _hooked && _fightMode == FightMode.Fighting;
    public Vector2 FightDirection => _hooked ? GetActiveFightForceDirection(out _) : Vector2.zero;
    public Vector2 LandingEscapeDirection => _hooked ? GetActiveFightForceDirection(out _) : Vector2.zero;
    public bool CanBeLanded => !_hooked || _exhaustionCount >= _exhaustionsBeforeLanding;
    public float TensionDamageMultiplier => _hooked && _landingPanicActive ? _landingPanicTensionMultiplier : 1f;
    public float FightIntensity => IsFighting ? _activeFightIntensity : _restForceMultiplier;
    public float StaminaPercent => _maxStamina <= 0f ? 0f : Mathf.Clamp01(_stamina / _maxStamina);

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();

        if (!_animator)
        {
            _animator = GetComponent<Animator>();
            if (!_animator)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        CacheAnimationStateHashes();
        ConfigureAnimationEventRelay();

        if (!_hookPoint)
        {
            _hookPoint = FindHookPoint(transform);
        }

        ConfigureSloshBlobAudio();
    }

    private void OnValidate()
    {
        _restAnimationSpeed = Mathf.Max(0f, _restAnimationSpeed);
        _fightAnimationSpeed = Mathf.Max(0f, _fightAnimationSpeed);
        _idleSloshPitch = Mathf.Max(0.01f, _idleSloshPitch);
        _fightSloshPitch = Mathf.Max(0.01f, _fightSloshPitch);
        CacheAnimationStateHashes();
    }

    private void OnEnable()
    {
        _timer = 0f;
        _hooked = false;
        _hookTarget = null;
        _waitingAtFightAngle = false;
        _fightTimer = 0f;
        _targetFightAngle = 0f;
        _startFightRotation = Quaternion.identity;
        _targetFightRotation = Quaternion.identity;
        _fightTurnTimer = 0f;
        _activeFightTurnDuration = 0f;
        _fightMode = FightMode.Resting;
        _stamina = _maxStamina;
        _fightModeTimer = 0f;
        _activeStaminaUseRate = 0f;
        _activeFightIntensity = 1f;
        _directionChangeKickTimer = 0f;
        _counterPullSlowTimer = 0f;
        _escaping = false;
        _escapeTimer = 0f;
        _activeEscapeFadeDuration = _escapeFadeDuration;
        _activeEscapeSinkSpeed = _escapeSinkSpeed;
        _escapeStartOpacity = 0f;
        SchedulePassiveDeflection();
        _rod = FishingRodController.ActiveRod;
        SetOpacity(0f);
        _currentAnimationStateHash = 0;
        _currentAnimationSpeed = -1f;
        ConfigureSloshBlobAudio();
        UpdateAnimationState(true);
    }

    private void Update()
    {
        if (_escaping)
        {
            UpdateEscape();
            return;
        }

        if (_hooked)
        {
            if (_rod && !_rod.IsFishingActive)
            {
                AlignHookPointTo(_hookTarget);
                SetOpacity(maxOpacity);
                return;
            }

            UpdateHookedMovement();
            AlignHookPointTo(_hookTarget);
            SetOpacity(maxOpacity);
            return;
        }

        if (TryMoveToBob())
        {
            SetOpacity(maxOpacity);
            return;
        }

        if (!_hooked)
        {
            Move();
        }

        UpdateVisibility();
    }

    private void FixedUpdate()
    {
        if (_escaping || !_hooked || !_rod || !_rod.IsFishingActive || !_rod.BobRigidbody)
        {
            return;
        }

        Vector2 fightDirection = GetActiveFightForceDirection(out float edgeBoostMultiplier);
        float kickMultiplier = GetDirectionChangeKickMultiplier();
        float slowMultiplier = GetCounterPullSlowMultiplier();
        _rod.BobRigidbody.AddForce(
            fightDirection * (_fightForce * FightIntensity * kickMultiplier * slowMultiplier * edgeBoostMultiplier),
            ForceMode2D.Force);
    }

    private void Move()
    {
        UpdatePassiveDeflection();
        transform.Translate(_direction * (_speed * Time.deltaTime));
    }

    private void SchedulePassiveDeflection()
    {
        _hasPassiveDeflected = false;
        _willPassiveDeflect = lifeTime > 0f
                              && _maxPassiveDeflectAngle > 0f
                              && Random.value <= _passiveDeflectChance;
        _passiveDeflectTime = _willPassiveDeflect ? Random.Range(0f, lifeTime) : float.PositiveInfinity;
    }

    private void UpdatePassiveDeflection()
    {
        if (!_willPassiveDeflect || _hasPassiveDeflected || _timer < _passiveDeflectTime)
        {
            return;
        }

        float deflectAngle = Random.Range(-_maxPassiveDeflectAngle, _maxPassiveDeflectAngle);
        transform.Rotate(0f, 0f, deflectAngle);
        _hasPassiveDeflected = true;
    }

    private void UpdateVisibility()
    {
        _timer += Time.deltaTime;

        float progress = _timer / lifeTime;

        // 0 -> 1 -> 0
        float opacityCurve = Mathf.Sin(progress * Mathf.PI);

        float opacity = opacityCurve * maxOpacity;
        SetOpacity(opacity);

        if (_timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void SetOpacity(float opacity)
    {
        Color color = _renderer.color;
        color.a = opacity;
        _renderer.color = color;
        _currentOpacity = opacity;
    }

    private bool TryMoveToBob()
    {
        if (!IsAttractableDepth())
        {
            return false;
        }

        if (!_rod)
        {
            _rod = FishingRodController.ActiveRod;
        }

        if (!_rod || !_rod.CanAttractFish)
        {
            return false;
        }

        Vector2 fishPos = transform.position;
        Vector2 hookPos = GetHookPosition();
        Vector2 bobPos = _rod.BobPosition;
        Vector2 directionToBob = bobPos - hookPos;
        float distanceToBob = Vector2.Distance(hookPos, bobPos);

        if (distanceToBob > _attractionRadius)
        {
            return false;
        }

        FaceDirection(directionToBob);

        if (distanceToBob <= _hookDistance && _rod.TryHookFish(this))
        {
            HookTo(_rod.BobTransform);
            return true;
        }

        Vector2 moveDirection = directionToBob.normalized;
        transform.position = fishPos + moveDirection * (_attractionSpeed * Time.deltaTime);

        return true;
    }

    private Vector2 GetHookPosition()
    {
        return _hookPoint ? _hookPoint.position : transform.position;
    }

    private Transform FindHookPoint(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLowerInvariant() == "hookpoint")
            {
                return child;
            }

            Transform hookPoint = FindHookPoint(child);
            if (hookPoint)
            {
                return hookPoint;
            }
        }

        return null;
    }

    private void FaceDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _turnSpeed * Time.deltaTime);
    }

    private void UpdateHookedMovement()
    {
        if (!_hookTarget)
        {
            return;
        }

        UpdateLandingPanicCooldown();
        UpdateFightMode();
        TryStartLandingPanic();

        if (_waitingAtFightAngle)
        {
            _fightTimer -= Time.deltaTime;

            if (_fightTimer <= 0f)
            {
                PickNewFightDirection();
            }

            return;
        }

        _fightTurnTimer += Time.deltaTime;
        float turnProgress = _activeFightTurnDuration <= 0f ? 1f : Mathf.Clamp01(_fightTurnTimer / _activeFightTurnDuration);
        float easedProgress = EaseOutCubic(turnProgress);
        transform.rotation = Quaternion.Slerp(_startFightRotation, _targetFightRotation, easedProgress);

        if (Quaternion.Angle(transform.rotation, _targetFightRotation) <= _fightAngleReachedThreshold)
        {
            transform.rotation = _targetFightRotation;
            _waitingAtFightAngle = true;
            _fightTimer = RandomFromRange(_fightWaitTime) / GetDirectionChangeSpeedMultiplier();
        }
    }

    private void PickNewFightDirection()
    {
        Vector2 awayFromFishingPoint = GetAwayFromFishingPointDirection();
        if (awayFromFishingPoint == Vector2.zero)
        {
            awayFromFishingPoint = transform.right;
        }

        float centerAngle = Mathf.Atan2(awayFromFishingPoint.y, awayFromFishingPoint.x) * Mathf.Rad2Deg;
        float halfArc = _fightArc * 0.5f;
        _targetFightAngle = Random.Range(-halfArc, halfArc);
        _startFightRotation = transform.rotation;
        _targetFightRotation = Quaternion.Euler(0f, 0f, centerAngle + _targetFightAngle);
        _fightTurnTimer = 0f;
        _activeFightTurnDuration = GetFightTurnDuration();
        _directionChangeKickTimer = _directionChangeKickDuration;
        _waitingAtFightAngle = false;
    }

    public void ApplyCounterPull(float pullStrength, float deltaTime)
    {
        if (!IsFighting || pullStrength <= 0f)
        {
            return;
        }

        _stamina = Mathf.Max(0f, _stamina - _counterPullStaminaDrain * pullStrength * deltaTime);
        _counterPullSlowTimer = _counterPullSlowDuration;

        if (_stamina <= 0f)
        {
            StartRestMode(true);
        }
    }

    private void UpdateFightMode()
    {
        if (_fightMode == FightMode.Fighting)
        {
            _fightModeTimer -= Time.deltaTime;
            _stamina = Mathf.Max(0f, _stamina - _activeStaminaUseRate * Time.deltaTime);

            bool exhausted = _stamina <= 0f;
            if (_fightModeTimer <= 0f || exhausted)
            {
                StartRestMode(exhausted);
            }

            return;
        }

        _fightModeTimer -= Time.deltaTime;
        _stamina = Mathf.Min(_maxStamina, _stamina + _staminaRecoveryRate * Time.deltaTime);

        if (_fightModeTimer <= 0f && StaminaPercent >= _minStaminaToFight)
        {
            StartFightMode();
        }
    }

    private void StartFightMode(bool landingPanic = false)
    {
        _fightMode = FightMode.Fighting;
        _landingPanicActive = landingPanic;
        _fightModeTimer = RandomFromRange(landingPanic ? _landingPanicDuration : _fightDuration);
        _activeStaminaUseRate = RandomFromRange(_fightStaminaUseRate);
        _activeFightIntensity = RandomFromRange(_fightIntensity);
        if (landingPanic)
        {
            _activeFightIntensity *= _landingPanicIntensityMultiplier;
        }

        UpdateAnimationState();
        PickNewFightDirection();
    }

    private void StartRestMode(bool exhausted)
    {
        if (_fightMode == FightMode.Resting)
        {
            return;
        }

        if (exhausted)
        {
            _exhaustionCount++;
        }

        _fightMode = FightMode.Resting;
        _landingPanicActive = false;
        _fightModeTimer = RandomFromRange(_restDuration);
        _activeStaminaUseRate = 0f;
        _activeFightIntensity = _restForceMultiplier;
        UpdateAnimationState();
        PickNewFightDirection();
    }

    private void UpdateLandingPanicCooldown()
    {
        if (_landingPanicCooldownTimer > 0f)
        {
            _landingPanicCooldownTimer = Mathf.Max(0f, _landingPanicCooldownTimer - Time.deltaTime);
        }
    }

    private void TryStartLandingPanic()
    {
        if (!_hooked
            || CanBeLanded
            || _fightMode != FightMode.Resting
            || _landingPanicDistance <= 0f
            || _landingPanicCooldownTimer > 0f
            || !_rod
            || !_hookTarget)
        {
            return;
        }

        float distanceToLanding = Vector2.Distance(_hookTarget.position, _rod.FishingPointPosition);
        if (distanceToLanding > _landingPanicDistance)
        {
            return;
        }

        if (Random.value <= _landingPanicChance)
        {
            StartLandingPanic();
            return;
        }

        _landingPanicCooldownTimer = _landingPanicRetryDelay;
    }

    private void StartLandingPanic()
    {
        _stamina = Mathf.Max(_stamina, _maxStamina * _landingPanicStaminaRestore);
        _landingPanicCooldownTimer = _landingPanicCooldown;
        StartFightMode(true);
    }

    public void RejectLandingAttempt()
    {
        if (!_hooked)
        {
            return;
        }

        StartLandingPanic();
    }

    private void CacheAnimationStateHashes()
    {
        _idleAnimationStateHash = string.IsNullOrWhiteSpace(_idleAnimationState)
            ? 0
            : Animator.StringToHash(_idleAnimationState);
        _fightAnimationStateHash = string.IsNullOrWhiteSpace(_fightAnimationState)
            ? 0
            : Animator.StringToHash(_fightAnimationState);
    }

    private void UpdateAnimationState(bool force = false)
    {
        if (!_animator)
        {
            return;
        }

        string stateName = _hooked ? _fightAnimationState : _idleAnimationState;
        int stateHash = _hooked ? _fightAnimationStateHash : _idleAnimationStateHash;
        if (_animator.runtimeAnimatorController
            && !string.IsNullOrWhiteSpace(stateName)
            && (force || _currentAnimationStateHash != stateHash))
        {
            _animator.Play(stateName, 0, 0f);
            _currentAnimationStateHash = stateHash;
        }

        float animationSpeed = IsFighting ? _fightAnimationSpeed : _restAnimationSpeed;
        if (force || !Mathf.Approximately(_currentAnimationSpeed, animationSpeed))
        {
            _animator.speed = animationSpeed;
            _currentAnimationSpeed = animationSpeed;
        }
    }

    public void PlaySloshBlobSfx()
    {
        if (_sloshBlobSfx == SoundId.None || !_rod || !_rod.HasInputFocus)
        {
            return;
        }

        if (!_sloshBlobAudioSource)
        {
            ConfigureSloshBlobAudio();
        }

        if (!_sloshBlobAudioSource)
        {
            return;
        }

        AudioManager.Instance?.PlaySfxOnSource(_sloshBlobAudioSource, _sloshBlobSfx, pitchScale: GetSloshBlobPitch());
    }

    public void StopAudio()
    {
        if (_sloshBlobAudioSource) _sloshBlobAudioSource.Stop();
    }

    private void ConfigureSloshBlobAudio()
    {
        if (_sloshBlobSfx == SoundId.None || !AudioManager.Instance)
        {
            return;
        }

        if (!_sloshBlobAudioSource)
        {
            _sloshBlobAudioSource = AudioManager.Instance.CreateSfxSource(
                _sloshBlobSfx,
                transform,
                baseVolume: _sloshBlobVolume);
            return;
        }

        AudioManager.Instance.ConfigureSfxSource(_sloshBlobAudioSource, _sloshBlobSfx, _sloshBlobVolume);
    }

    private void ConfigureAnimationEventRelay()
    {
        if (!_animator || _animator.gameObject == gameObject)
        {
            return;
        }

        FishAnimationEventRelay relay = _animator.GetComponent<FishAnimationEventRelay>();
        if (!relay)
        {
            relay = _animator.gameObject.AddComponent<FishAnimationEventRelay>();
        }

        relay.SetFish(this);
    }

    private float GetSloshBlobPitch()
    {
        float currentSpeed = _currentAnimationSpeed >= 0f ? _currentAnimationSpeed : GetTargetAnimationSpeed();
        float lowSpeed = Mathf.Min(_restAnimationSpeed, _fightAnimationSpeed);
        float highSpeed = Mathf.Max(_restAnimationSpeed, _fightAnimationSpeed);

        if (Mathf.Approximately(lowSpeed, highSpeed))
        {
            return IsFighting
                ? Mathf.Max(_idleSloshPitch, _fightSloshPitch)
                : Mathf.Min(_idleSloshPitch, _fightSloshPitch);
        }

        float speedPercent = Mathf.InverseLerp(lowSpeed, highSpeed, currentSpeed);
        float lowPitch = Mathf.Min(_idleSloshPitch, _fightSloshPitch);
        float highPitch = Mathf.Max(_idleSloshPitch, _fightSloshPitch);

        return Mathf.Lerp(lowPitch, highPitch, speedPercent);
    }

    private float GetTargetAnimationSpeed()
    {
        return IsFighting ? _fightAnimationSpeed : _restAnimationSpeed;
    }

    private float RandomFromRange(Vector2 range)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);

        return Mathf.Approximately(min, max) ? min : Random.Range(min, max);
    }

    private float GetFightTurnDuration()
    {
        float angle = Quaternion.Angle(_startFightRotation, _targetFightRotation);
        float speedMultiplier = GetDirectionChangeSpeedMultiplier();
        float speedDuration = _fightTurnSpeed <= 0f ? 0f : angle / (_fightTurnSpeed * speedMultiplier);
        float randomDuration = RandomFromRange(_fightTurnDuration) / speedMultiplier;

        return Mathf.Max(0.01f, Mathf.Max(speedDuration, randomDuration));
    }

    private float GetDirectionChangeSpeedMultiplier()
    {
        return Mathf.Max(0.01f, _directionChangeSpeedMultiplier);
    }

    private float EaseOutCubic(float progress)
    {
        float inverseProgress = 1f - progress;
        return 1f - inverseProgress * inverseProgress * inverseProgress;
    }

    private float GetDirectionChangeKickMultiplier()
    {
        if (_directionChangeKickTimer <= 0f || _directionChangeKickDuration <= 0f)
        {
            return 1f;
        }

        float kickProgress = Mathf.Clamp01(_directionChangeKickTimer / _directionChangeKickDuration);
        _directionChangeKickTimer -= Time.fixedDeltaTime;

        return 1f + _directionChangeKickMultiplier * kickProgress;
    }

    private float GetCounterPullSlowMultiplier()
    {
        if (_counterPullSlowTimer <= 0f || _counterPullSlowDuration <= 0f)
        {
            return 1f;
        }

        _counterPullSlowTimer -= Time.fixedDeltaTime;
        return _counterPullFightSlowMultiplier;
    }

    private Vector2 GetFightForceDirection()
    {
        Vector2 fightDirection = transform.right;
        Vector2 awayFromFishingPoint = GetAwayFromFishingPointDirection();

        if (awayFromFishingPoint == Vector2.zero)
        {
            return fightDirection;
        }

        if (Vector2.Dot(fightDirection, awayFromFishingPoint) >= 0f)
        {
            return AddMinimumSideForce(fightDirection.normalized, awayFromFishingPoint);
        }

        float side = Mathf.Sign(Vector3.Cross(awayFromFishingPoint, fightDirection).z);
        if (Mathf.Approximately(side, 0f))
        {
            side = Random.value < 0.5f ? -1f : 1f;
        }

        return new Vector2(-awayFromFishingPoint.y * side, awayFromFishingPoint.x * side).normalized;
    }

    private Vector2 GetActiveFightForceDirection(out float edgeBoostMultiplier)
    {
        edgeBoostMultiplier = 1f;
        Vector2 fightDirection = _rod && _rod.IsLineLocked ? GetLockedLineFightDirection() : GetFightForceDirection();

        if (!TryGetEdgeEscapeDirection(out Vector2 edgeEscapeDirection, out float edgePressure))
        {
            return fightDirection;
        }

        edgeBoostMultiplier = Mathf.Lerp(1f, _edgeBoostMultiplier, edgePressure);

        if (fightDirection == Vector2.zero)
        {
            return edgeEscapeDirection;
        }

        return Vector2.Lerp(fightDirection, edgeEscapeDirection, edgePressure).normalized;
    }

    private Vector2 GetLockedLineFightDirection()
    {
        Vector2 awayFromFishingPoint = GetAwayFromFishingPointDirection();
        if (awayFromFishingPoint == Vector2.zero)
        {
            return GetFightForceDirection();
        }

        Vector2 sideDirection = new Vector2(-awayFromFishingPoint.y, awayFromFishingPoint.x);
        if (Vector2.Dot(GetFightForceDirection(), sideDirection) < 0f)
        {
            sideDirection = -sideDirection;
        }

        return sideDirection.normalized;
    }

    private bool TryGetEdgeEscapeDirection(out Vector2 edgeEscapeDirection, out float edgePressure)
    {
        edgeEscapeDirection = Vector2.zero;
        edgePressure = 0f;

        if (!_rod || !_hookTarget)
        {
            return false;
        }

        return _rod.TryGetEdgeEscapeDirection(
            _hookTarget.position,
            _edgeAvoidanceDistance,
            out edgeEscapeDirection,
            out edgePressure);
    }

    private Vector2 AddMinimumSideForce(Vector2 fightDirection, Vector2 awayFromFishingPoint)
    {
        if (_minimumSideForce <= 0f)
        {
            return fightDirection;
        }

        Vector2 sideAxis = new Vector2(-awayFromFishingPoint.y, awayFromFishingPoint.x);
        float sideAmount = Vector2.Dot(fightDirection, sideAxis);

        if (Mathf.Abs(sideAmount) >= _minimumSideForce)
        {
            return fightDirection;
        }

        float sideSign = Mathf.Sign(sideAmount);
        if (Mathf.Approximately(sideSign, 0f))
        {
            sideSign = Random.value < 0.5f ? -1f : 1f;
        }

        float outwardAmount = Mathf.Sqrt(1f - _minimumSideForce * _minimumSideForce);
        return (awayFromFishingPoint * outwardAmount + sideAxis * (sideSign * _minimumSideForce)).normalized;
    }

    private Vector2 GetAwayFromFishingPointDirection()
    {
        if (!_rod || !_hookTarget)
        {
            return Vector2.zero;
        }

        Vector2 bobPosition = _hookTarget.position;
        Vector2 fishingPointPosition = _rod.FishingPointPosition;
        Vector2 awayFromFishingPoint = bobPosition - fishingPointPosition;

        return awayFromFishingPoint == Vector2.zero ? Vector2.zero : awayFromFishingPoint.normalized;
    }

    private bool IsAttractableDepth()
    {
        return _currentOpacity >= _minAttractOpacity && _currentOpacity <= _maxAttractOpacity;
    }

    private void HookTo(Transform bobTransform)
    {
        if (!bobTransform)
        {
            return;
        }

        _hooked = true;
        _hookTarget = bobTransform;
        _stamina = _maxStamina;
        _exhaustionCount = 0;
        _landingPanicCooldownTimer = 0f;
        _landingPanicActive = false;
        StartFightMode();

        transform.SetParent(bobTransform, true);
        AlignHookPointTo(bobTransform);
        SetOpacity(maxOpacity);
    }

    private void AlignHookPointTo(Transform target)
    {
        if (!target)
        {
            return;
        }

        Vector3 hookPosition = _hookPoint ? _hookPoint.position : transform.position;
        transform.position += target.position - hookPosition;
    }

    public void EscapeFromHook(float fadeDuration, float sinkSpeed)
    {
        _hooked = false;
        _hookTarget = null;
        _exhaustionCount = 0;
        _landingPanicCooldownTimer = 0f;
        _landingPanicActive = false;
        _escaping = true;
        _escapeTimer = 0f;
        _activeEscapeFadeDuration = fadeDuration > 0f ? fadeDuration : _escapeFadeDuration;
        _activeEscapeSinkSpeed = sinkSpeed;
        _escapeStartOpacity = _currentOpacity;
        transform.SetParent(null, true);
        UpdateAnimationState();
    }

    private void UpdateEscape()
    {
        _escapeTimer += Time.deltaTime;
        float progress = _activeEscapeFadeDuration <= 0f ? 1f : Mathf.Clamp01(_escapeTimer / _activeEscapeFadeDuration);

        transform.position += Vector3.down * (_activeEscapeSinkSpeed * Time.deltaTime);
        SetOpacity(Mathf.Lerp(_escapeStartOpacity, 0f, progress));

        if (progress >= 1f)
        {
            ReturnToPool();
        }
    }

    public void Setup(float speed)
    {
        _speed = speed;
    }
    
    public void SetPool(FishPool pool)
    {
        _pool = pool;
    }

    public void ReturnToPool()
    {
        _hooked = false;
        _hookTarget = null;
        _fightTimer = 0f;
        _targetFightAngle = 0f;
        _startFightRotation = Quaternion.identity;
        _targetFightRotation = Quaternion.identity;
        _fightTurnTimer = 0f;
        _activeFightTurnDuration = 0f;
        _waitingAtFightAngle = false;
        _fightMode = FightMode.Resting;
        _stamina = _maxStamina;
        _fightModeTimer = 0f;
        _activeStaminaUseRate = 0f;
        _activeFightIntensity = 1f;
        _directionChangeKickTimer = 0f;
        _counterPullSlowTimer = 0f;
        _exhaustionCount = 0;
        _landingPanicCooldownTimer = 0f;
        _landingPanicActive = false;
        _escaping = false;
        _escapeTimer = 0f;
        _activeEscapeFadeDuration = _escapeFadeDuration;
        _activeEscapeSinkSpeed = _escapeSinkSpeed;
        _escapeStartOpacity = 0f;
        UpdateAnimationState(true);

        if (_pool)
        {
            _pool.ReturnFish(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
