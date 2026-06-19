using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FishingRodController : MonoBehaviour
{
    public static FishingRodController ActiveRod { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject bob;
    [SerializeField] private GameObject fishingPoint;
    [SerializeField] private FishingMiniGame fishingMiniGame;
    [SerializeField] private Camera fishingCamera;
    private Rigidbody2D _bobRb;
    private Camera _camera;
    
    [Header("Throw")]
    [SerializeField] private float throwForceMultiplier = 1f;
    private Vector2 _throwDir = Vector2.zero;
    private Vector2 _throwForce = Vector2.zero;
    
    [Header("Reel Speed")]
    [FormerlySerializedAs("realInSpeed")]
    [SerializeField] private float maxReelSpeed = 2f;
    [SerializeField] private float hookedMaxReelSpeed = 2.8f;
    [SerializeField] private float counterPullMaxReelSpeed = 3.4f;
    [SerializeField] private float reelAcceleration = 10f;

    [Header("Mouse Pull")]
    [SerializeField] private float hookedMouseReelForce = 4f;
    [SerializeField] private float emptyMouseReelForce = 1f;
    [SerializeField, Min(0f)] private float minMousePullPixels = 12f;
    [SerializeField, Min(1f)] private float maxMousePullPixels = 120f;

    [Header("Mouse Pull Visual")]
    [SerializeField] private bool showMousePullVisual = true;
    [SerializeField, Min(0f)] private float mousePullAnchorRadius = 0.12f;
    [SerializeField, Min(0.001f)] private float mousePullVisualWidth = 0.025f;
    [SerializeField] private Color mousePullAnchorColor = Color.white;
    [SerializeField] private Color mousePullLineColor = Color.cyan;

    [Header("Counter Pull")]
    [SerializeField] private float counterPullAngle = 14f;
    [SerializeField] private float counterPullAssistAngle = 42f;
    [SerializeField] private float counterPullForceMultiplier = 1.5f;
    [SerializeField, Range(0f, 1f)] private float counterPullTensionMultiplier = 0.3f;

    [Header("Fight / Rest Reeling")]
    [SerializeField] private float fightingReelMultiplier = 0.18f;
    [SerializeField] private float restingReelMultiplier = 0.52f;

    [Header("Reel Timing")]
    [SerializeField] private float reelStartDelay = 0.3f;
    [SerializeField] private float readyDistance = 0.5f;
    private Vector2 _fishingPointDir = Vector2.zero;

    [Header("Fishing Line")]
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private int lineSortingOrder = 1;

    [Header("Pull Force Indicator")]
    [SerializeField] private bool showPullForceIndicator = true;
    [SerializeField] private float pullForceIndicatorLength = 1.25f;
    [SerializeField] private float pullForceIndicatorWidth = 0.035f;
    [SerializeField] private float pullForceIndicatorSmoothSpeed = 18f;
    [SerializeField, Min(0.01f)] private float pullForceIndicatorFullLengthForce = 16f;
    [SerializeField] private Color pullForceIndicatorColor = Color.cyan;
    private LineRenderer _pullForceIndicator;

    [Header("Line Tension")]
    [SerializeField] private float tensionBuildSpeed = 0.65f;
    [SerializeField] private float tensionRecoverSpeed = 0.85f;
    [SerializeField, Min(0f)] private float gentlePullTensionRecoverMultiplier = 1.15f;
    [SerializeField, Range(0f, 1f)] private float gentlePullMaxStrength = 0.6f;
    [SerializeField] private float lineSnapTension = 1.15f;
    [SerializeField] private float highTensionLineWidth = 0.015f;
    [SerializeField] private Color highTensionLineColor = Color.red;

    [Header("Line Snap")]
    [SerializeField] private float fishEscapeFadeDuration = 0.45f;
    [SerializeField] private float fishEscapeSinkSpeed = 0.6f;
    [SerializeField] private float bobRespawnDelay = 0.35f;
    [SerializeField] private float rejectedLandingPushDistance = 0.8f;
    [SerializeField] private float rejectedLandingPushImpulse = 3f;

    [Header("Fishing Audio")]
    [SerializeField] private SoundId sfxReelClick = SoundId.ReelTick;
    [SerializeField] private SoundId sfxSplash = SoundId.Splash;
    [SerializeField, Range(0f, 1f)] private float reelClickVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float splashVolume = 1f;
    [SerializeField, Min(0f)] private float reelClickMinSpeed = 0.2f;
    [SerializeField, Min(0f)] private float reelClickMaxSpeed = 8f;
    [SerializeField, Min(0.01f)] private float slowReelClickInterval = 0.22f;
    [SerializeField, Min(0.01f)] private float fastReelClickInterval = 0.06f;
    [SerializeField, Min(0.01f)] private float lowReelClickPitch = 0.75f;
    [SerializeField, Min(0.01f)] private float highReelClickPitch = 1.45f;
    [SerializeField, Min(0f)] private float splashStopAwaySpeed = 0.05f;

    [Header("Focus")]
    [SerializeField, Min(0f)] private float focusInputDelay = 0.2f;

    [Header("Bob Bounds")]
    [SerializeField] private bool keepBobInsideCamera = true;
    [SerializeField] private float cameraBoundsPadding = 0.2f;
    [SerializeField] private bool keepBobAboveFishingPoint = true;
    [SerializeField] private float minFishingPointYOffset = 0.05f;
    [SerializeField] private float sideBoundaryReelMultiplier = 0.55f;
    [SerializeField] private float sideBoundaryTensionBuildSpeed = 0.45f;
    [SerializeField] private float sideBoundaryReleaseForce = 8f;

    [Header("Counter Pull Guide")]
    [SerializeField] private bool showCounterPullGuide = true;
    [SerializeField] private float counterPullGuideLength = 1.5f;
    [SerializeField] private float counterPullGuideWidth = 0.025f;
    [SerializeField] private Color perfectCounterGuideColor = Color.green;
    [SerializeField] private Color assistCounterGuideColor = Color.yellow;
    [SerializeField] private Color currentPullGuideColor = Color.white;
    private LineRenderer _perfectCounterGuide;
    private LineRenderer _perfectCounterLeftGuide;
    private LineRenderer _perfectCounterRightGuide;
    private LineRenderer _assistCounterLeftGuide;
    private LineRenderer _assistCounterRightGuide;
    private LineRenderer _currentPullGuide;

    private bool _thrown = false;
    private bool _isReeling = false;
    private bool _isMouseButtonHeld = false;
    private bool _hasStartedReelingThisThrow = false;
    private bool _bobHasLeftFishingPoint = false;
    private Fish _hookedFish;
    private float _reelReadyTime = 0f;
    private float _lineTension = 0f;
    private bool _lineDistanceLocked = false;
    private float _lockedLineDistance = 0f;
    private float _currentPullStrength = 0f;
    private Vector2 _currentPullDirection = Vector2.zero;
    private bool _counterPullingThisFrame = false;
    private float _counterPullStrength = 0f;
    private bool _pinnedToSideBoundary = false;
    private Vector2 _sideBoundaryReleaseDirection = Vector2.zero;
    private bool _snapInProgress = false;
    private Vector2 _lastAppliedBobForce = Vector2.zero;
    private Vector2 _smoothedPullIndicatorDirection = Vector2.zero;
    private float _smoothedPullIndicatorLength = 0f;
    private TVInputRelay _crtInputRelay;
    private bool _hasInputFocus = false;
    private bool _inputReady = false;
    private bool _waitingForPointerRelease = false;
    private float _inputUnlockTime = 0f;
    private bool _relayPointerHeld = false;
    private bool _relayPointerPressedThisFrame = false;
    private bool _hasRelayPointerPosition = false;
    private Vector2 _relayPointerWorldPosition = Vector2.zero;
    private bool _hasMousePullAnchor = false;
    private Vector2 _mousePullAnchorScreenPosition = Vector2.zero;
    private LineRenderer _mousePullAnchorCircle;
    private LineRenderer _mousePullLine;
    private float _nextReelClickTime = 0f;
    private bool _castMovedAwayFromFishingPoint = false;
    private bool _splashPlayedThisThrow = false;
    private AudioSource reelClickAudioSource;
    private AudioSource splashAudioSource;
    private int _castId = 0;

    public bool CanAttractFish => IsFishingActive && _bobHasLeftFishingPoint && !_hookedFish && bob;
    public Transform BobTransform => bob ? bob.transform : null;
    public Rigidbody2D BobRigidbody => _bobRb;
    public Vector2 BobPosition => bob ? bob.transform.position : transform.position;
    public Vector2 FishingPointPosition => fishingPoint ? fishingPoint.transform.position : transform.position;
    public bool IsLineLocked => ShouldLockLineDistance();
    public bool IsFishingActive => _thrown && !_snapInProgress;
    public bool HasInputFocus => _hasInputFocus;
    public int CastId => _castId;

    public bool TryGetEdgeEscapeDirection(Vector2 position, float edgeDistance, out Vector2 escapeDirection, out float edgePressure)
    {
        escapeDirection = Vector2.zero;
        edgePressure = 0f;

        if (edgeDistance <= 0f)
        {
            return false;
        }

        if (!TryGetPlayableArea(out float minX, out float maxX, out float minY, out float maxY))
        {
            return false;
        }

        AddEdgeEscape(position.x - minX, Vector2.right, edgeDistance, ref escapeDirection, ref edgePressure);
        AddEdgeEscape(maxX - position.x, Vector2.left, edgeDistance, ref escapeDirection, ref edgePressure);
        AddEdgeEscape(position.y - minY, Vector2.up, edgeDistance, ref escapeDirection, ref edgePressure);
        AddEdgeEscape(maxY - position.y, Vector2.down, edgeDistance, ref escapeDirection, ref edgePressure);

        if (escapeDirection == Vector2.zero)
        {
            return false;
        }

        escapeDirection.Normalize();
        return true;
    }

    private void Awake()
    {
        ActiveRod = this;
    }

    private void Start()
    {
        if (bob)
        {
            _bobRb = bob.GetComponent<Rigidbody2D>();
        }

        if (!fishingPoint)
        {
            fishingPoint = gameObject;
        }

        if (!fishingMiniGame)
        {
            fishingMiniGame = GetComponentInParent<FishingMiniGame>();
        }

        ResolveCamera();
        ConfigureFishingLine();
        ConfigureCounterPullGuide();
        ConfigurePullForceIndicator();
        ConfigureMousePullVisual();
        ConfigureFishingAudio();
        SetInputFocus(false);
    }

    private void OnDestroy()
    {
        if (ActiveRod == this)
        {
            ActiveRod = null;
        }

        SetCrtInput(null);
    }

    private void Update()
    {
        UpdateInputFocusReadiness();

        if (!_inputReady)
        {
            StopActiveInput();
            return;
        }

        if (_snapInProgress)
        {
            StopActiveInput();
            ClearFrameInput();
            return;
        }

        if (!IsPointerInputAvailable())
        {
            StopActiveInput();
            ClearFrameInput();
            return;
        }

        _isMouseButtonHeld = IsPointerHeld();

        if (WasPointerPressedThisFrame() && !_thrown)
        {
            GetThrowDirection();
            Throw();
            ClearFrameInput();
            return;
        }

        _isReeling = _isMouseButtonHeld && CanStartReeling();
        if (_isReeling)
        {
            UpdateMousePullAnchor();
        }
        else
        {
            ClearMousePullAnchor();
        }

        ClearFrameInput();
    }

    public void SetCrtInput(TVInputRelay relay)
    {
        if (_crtInputRelay == relay)
        {
            return;
        }

        if (_crtInputRelay)
        {
            _crtInputRelay.PointerDown -= HandleRelayPointerDown;
            _crtInputRelay.PointerDrag -= HandleRelayPointerDrag;
            _crtInputRelay.PointerUp -= HandleRelayPointerUp;
        }

        _crtInputRelay = relay;
        ResetRelayPointerState();

        if (_crtInputRelay)
        {
            _crtInputRelay.PointerDown += HandleRelayPointerDown;
            _crtInputRelay.PointerDrag += HandleRelayPointerDrag;
            _crtInputRelay.PointerUp += HandleRelayPointerUp;
        }
    }

    public void SetInputFocus(bool hasFocus)
    {
        if (_hasInputFocus == hasFocus)
        {
            if (!hasFocus)
            {
                StopActiveInput();
                StopFishingAudio();
            }

            return;
        }

        _hasInputFocus = hasFocus;
        _inputReady = false;
        _waitingForPointerRelease = false;
        _inputUnlockTime = 0f;
        StopActiveInput();

        if (_hasInputFocus)
        {
            _inputUnlockTime = Time.unscaledTime + focusInputDelay;
            _waitingForPointerRelease = IsAnyPointerHeld();
            return;
        }

        StopFishingAudio();
    }

    private void LateUpdate()
    {
        UpdateFishingLine();
        UpdatePullForceIndicator();
        UpdateMousePullVisual();
        UpdateCounterPullGuide();
    }

    private void FixedUpdate()
    {
        UpdateInputFocusReadiness();

        if (!_thrown || _snapInProgress)
        {
            return;
        }

        if (!_bobHasLeftFishingPoint && !IsBobAtFishingPoint())
        {
            _bobHasLeftFishingPoint = true;
            _castMovedAwayFromFishingPoint = true;
        }

        if (_isReeling)
        {
            _hasStartedReelingThisThrow = true;
            ClearLineDistanceLock();
            ReelIn(GetReelForce());
        }
        else if (ShouldLockLineDistance())
        {
            LockLineDistance();
        }
        else
        {
            ClearLineDistanceLock();
            _lastAppliedBobForce = Vector2.zero;
        }

        KeepBobInPlayableArea();
        UpdateFishingAudio();

        if (_inputReady)
        {
            UpdateLineTension(Time.fixedDeltaTime);
        }

        if (_inputReady && _bobHasLeftFishingPoint && IsBobAtFishingPoint())
        {
            if (_hookedFish && !_hookedFish.CanBeLanded)
            {
                RejectLandingAttempt();
                return;
            }

            ResetBobForThrow();
        }
    }

    private void Throw()
    {
        if (!_bobRb)
        {
            return;
        }

        _thrown = true;
        _castId++;
        _isReeling = false;
        _hasStartedReelingThisThrow = false;
        _bobHasLeftFishingPoint = false;
        ClearLineDistanceLock();
        ClearMousePullAnchor();
        _reelReadyTime = Time.time + reelStartDelay;
        ResetFishingAudioState();

        _bobRb.linearVelocity = Vector2.zero;
        _bobRb.AddForce(_throwForce, ForceMode2D.Impulse);
    }

    public bool TryHookFish(Fish fish)
    {
        if (!CanAttractFish || !fish)
        {
            return false;
        }

        _hookedFish = fish;
        return true;
    }

    private void ReelIn(Vector2 reelForce)
    {
        if (!_bobRb || reelForce == Vector2.zero)
        {
            _lastAppliedBobForce = Vector2.zero;
            return;
        }

        _lastAppliedBobForce = reelForce;
        _bobRb.AddForce(reelForce, ForceMode2D.Force);

        float maxSpeed = GetMaxBobSpeed();
        if (_bobRb.linearVelocity.magnitude > maxSpeed)
        {
            _bobRb.linearVelocity = _bobRb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void ResetBobForThrow()
    {
        bool caughtFish = _hookedFish;

        _thrown = false;
        _isReeling = false;
        _isMouseButtonHeld = false;
        _hasStartedReelingThisThrow = false;
        _bobHasLeftFishingPoint = false;
        _snapInProgress = false;
        _throwDir = Vector2.zero;
        _throwForce = Vector2.zero;
        _fishingPointDir = Vector2.zero;
        _reelReadyTime = 0f;
        _lineTension = 0f;
        ClearLineDistanceLock();
        ResetFishingAudioState();
        _currentPullStrength = 0f;
        _currentPullDirection = Vector2.zero;
        _counterPullingThisFrame = false;
        _counterPullStrength = 0f;
        _lastAppliedBobForce = Vector2.zero;
        _pinnedToSideBoundary = false;
        _sideBoundaryReleaseDirection = Vector2.zero;
        ClearMousePullAnchor();
        HidePullForceIndicator();
        PrepareInputForNextThrow();

        if (_bobRb)
        {
            _bobRb.linearVelocity = Vector2.zero;
            _bobRb.angularVelocity = 0f;
        }

        if (bob && fishingPoint)
        {
            MoveBobToFishingPoint();
            ReturnHookedFish();
            RegisterCaughtFishIfNeeded(caughtFish);
            return;
        }

        ReturnHookedFish();
        RegisterCaughtFishIfNeeded(caughtFish);
    }

    private void MoveBobToFishingPoint()
    {
        if (!bob || !fishingPoint)
        {
            return;
        }

        Vector3 fishingPointPosition = fishingPoint.transform.position;
        bob.transform.position = fishingPointPosition;

        if (_bobRb)
        {
            _bobRb.position = fishingPointPosition;
            _bobRb.linearVelocity = Vector2.zero;
            _bobRb.angularVelocity = 0f;
        }
    }

    private void RegisterCaughtFishIfNeeded(bool caughtFish)
    {
        if (caughtFish && fishingMiniGame)
        {
            fishingMiniGame.RegisterCaughtFish();
        }
    }

    private void RejectLandingAttempt()
    {
        if (!_bobRb || !fishingPoint)
        {
            return;
        }

        _hookedFish?.RejectLandingAttempt();

        Vector2 escapeDirection = _hookedFish ? _hookedFish.LandingEscapeDirection : Vector2.zero;
        if (escapeDirection == Vector2.zero)
        {
            escapeDirection = Vector2.up;
        }

        ClearLineDistanceLock();
        ClearMousePullAnchor();
        _bobHasLeftFishingPoint = true;
        _hasStartedReelingThisThrow = false;
        _reelReadyTime = Time.time + reelStartDelay;

        Vector2 fishingPointPosition = fishingPoint.transform.position;
        _bobRb.position = fishingPointPosition + escapeDirection.normalized * Mathf.Max(readyDistance, rejectedLandingPushDistance);
        _bobRb.linearVelocity = Vector2.zero;
        _bobRb.angularVelocity = 0f;
        _bobRb.AddForce(escapeDirection.normalized * rejectedLandingPushImpulse, ForceMode2D.Impulse);
    }

    private void ReturnHookedFish()
    {
        if (!_hookedFish)
        {
            return;
        }

        Fish hookedFish = _hookedFish;
        _hookedFish = null;
        hookedFish.ReturnToPool();
    }

    private void ConfigureFishingLine()
    {
        if (!fishingLine)
        {
            fishingLine = GetComponent<LineRenderer>();
        }

        if (!fishingLine)
        {
            fishingLine = gameObject.AddComponent<LineRenderer>();
        }

        fishingLine.useWorldSpace = true;
        fishingLine.positionCount = 2;
        fishingLine.startWidth = lineWidth;
        fishingLine.endWidth = lineWidth;
        fishingLine.startColor = lineColor;
        fishingLine.endColor = lineColor;
        fishingLine.sortingOrder = lineSortingOrder;

        if (!fishingLine.sharedMaterial)
        {
            fishingLine.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    private void ConfigureCounterPullGuide()
    {
        _perfectCounterGuide = CreateCounterPullGuideLine("Perfect Counter Direction", perfectCounterGuideColor);
        _perfectCounterLeftGuide = CreateCounterPullGuideLine("Perfect Counter Left", perfectCounterGuideColor);
        _perfectCounterRightGuide = CreateCounterPullGuideLine("Perfect Counter Right", perfectCounterGuideColor);
        _assistCounterLeftGuide = CreateCounterPullGuideLine("Assist Counter Left", assistCounterGuideColor);
        _assistCounterRightGuide = CreateCounterPullGuideLine("Assist Counter Right", assistCounterGuideColor);
        _currentPullGuide = CreateCounterPullGuideLine("Current Pull Direction", currentPullGuideColor);
        HideCounterPullGuide();
    }

    private void ConfigurePullForceIndicator()
    {
        _pullForceIndicator = CreateCounterPullGuideLine("Pull Force Indicator", pullForceIndicatorColor);
        HidePullForceIndicator();
    }

    private void ConfigureMousePullVisual()
    {
        _mousePullAnchorCircle = CreateLineRenderer(
            "Mouse Pull Anchor",
            mousePullAnchorColor,
            mousePullVisualWidth,
            lineSortingOrder + 2);
        _mousePullAnchorCircle.loop = true;
        _mousePullAnchorCircle.positionCount = 32;

        _mousePullLine = CreateLineRenderer(
            "Mouse Pull Line",
            mousePullLineColor,
            mousePullVisualWidth,
            lineSortingOrder + 2);
        _mousePullLine.positionCount = 2;

        HideMousePullVisual();
    }

    private void ConfigureFishingAudio()
    {
        if (sfxReelClick != SoundId.None)
        {
            ConfigureSfxAudioSource(ref reelClickAudioSource, sfxReelClick, reelClickVolume);
        }

        if (sfxSplash != SoundId.None)
        {
            ConfigureSfxAudioSource(ref splashAudioSource, sfxSplash, splashVolume);
        }
    }

    private void ConfigureSfxAudioSource(ref AudioSource source, SoundId soundId, float baseVolume)
    {
        if (soundId == SoundId.None || !AudioManager.Instance) return;

        if (!source)
        {
            source = AudioManager.Instance.CreateSfxSource(
                soundId,
                transform,
                baseVolume: baseVolume);
            return;
        }

        AudioManager.Instance.ConfigureSfxSource(source, soundId, baseVolume);
    }

    private void UpdateFishingAudio()
    {
        if (PauseMenueControler.IsPaused || !_hasInputFocus || !_bobRb || !bob || !fishingPoint)
        {
            return;
        }

        UpdateCastLandingAudio();
        UpdateReelClickAudio();
    }

    private void UpdateReelClickAudio()
    {
        if (sfxReelClick == SoundId.None)
        {
            return;
        }

        bool isCasting = _thrown && !_splashPlayedThisThrow;
        bool shouldClick = isCasting || _isReeling;
        if (!shouldClick)
        {
            _nextReelClickTime = Time.time;
            return;
        }

        float speed = _bobRb.linearVelocity.magnitude;
        float audibleMinSpeed = Mathf.Max(0.001f, reelClickMinSpeed);
        if (speed < audibleMinSpeed)
        {
            _nextReelClickTime = Time.time;
            return;
        }

        if (!reelClickAudioSource)
        {
            ConfigureSfxAudioSource(ref reelClickAudioSource, sfxReelClick, reelClickVolume);
        }

        if (!reelClickAudioSource || Time.time < _nextReelClickTime)
        {
            return;
        }

        float maxSpeed = Mathf.Max(audibleMinSpeed + 0.01f, reelClickMaxSpeed);
        float speedPercent = Mathf.InverseLerp(audibleMinSpeed, maxSpeed, speed);
        float slowInterval = Mathf.Max(slowReelClickInterval, fastReelClickInterval);
        float fastInterval = Mathf.Min(slowReelClickInterval, fastReelClickInterval);
        float lowPitch = Mathf.Min(lowReelClickPitch, highReelClickPitch);
        float highPitch = Mathf.Max(lowReelClickPitch, highReelClickPitch);

        AudioManager.Instance?.PlaySfxOnSource(
            reelClickAudioSource,
            sfxReelClick,
            pitchScale: Mathf.Lerp(lowPitch, highPitch, speedPercent));
        _nextReelClickTime = Time.time + Mathf.Lerp(slowInterval, fastInterval, speedPercent);
    }

    private void UpdateCastLandingAudio()
    {
        if (_splashPlayedThisThrow || !_bobHasLeftFishingPoint)
        {
            return;
        }

        Vector2 bobOffset = _bobRb.position - (Vector2)fishingPoint.transform.position;
        float distanceFromFishingPoint = bobOffset.magnitude;
        if (distanceFromFishingPoint <= readyDistance)
        {
            return;
        }

        Vector2 awayDirection = bobOffset / distanceFromFishingPoint;
        float awaySpeed = Vector2.Dot(_bobRb.linearVelocity, awayDirection);
        if (awaySpeed > splashStopAwaySpeed)
        {
            _castMovedAwayFromFishingPoint = true;
            return;
        }

        if (!_castMovedAwayFromFishingPoint)
        {
            return;
        }

        PlaySplashAudio();
        _splashPlayedThisThrow = true;
    }

    private void PlaySplashAudio()
    {
        if (sfxSplash == SoundId.None)
        {
            return;
        }

        if (!splashAudioSource)
        {
            ConfigureSfxAudioSource(ref splashAudioSource, sfxSplash, splashVolume);
        }

        if (!splashAudioSource)
        {
            return;
        }

        AudioManager.Instance?.PlaySfxOnSource(splashAudioSource, sfxSplash);
    }

    private void ResetFishingAudioState()
    {
        _nextReelClickTime = 0f;
        _castMovedAwayFromFishingPoint = false;
        _splashPlayedThisThrow = false;
    }

    public void StopFishingAudio()
    {
        if (reelClickAudioSource) reelClickAudioSource.Stop();
        if (splashAudioSource) splashAudioSource.Stop();
    }

    private LineRenderer CreateCounterPullGuideLine(string lineName, Color color)
    {
        return CreateLineRenderer(lineName, color, counterPullGuideWidth, lineSortingOrder + 1);
    }

    private LineRenderer CreateLineRenderer(string lineName, Color color, float width, int sortingOrder)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = sortingOrder;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader)
        {
            line.material = new Material(shader);
        }

        return line;
    }

    private void UpdateFishingLine()
    {
        if (!fishingLine)
        {
            return;
        }

        if (_snapInProgress || !bob || !bob.activeInHierarchy || !fishingPoint)
        {
            fishingLine.enabled = false;
            return;
        }

        fishingLine.enabled = true;
        UpdateFishingLineVisuals();
        fishingLine.SetPosition(0, fishingPoint.transform.position);
        fishingLine.SetPosition(1, bob.transform.position);
    }

    private void UpdatePullForceIndicator()
    {
        if (!showPullForceIndicator || !_isReeling || !bob || !bob.activeInHierarchy)
        {
            HidePullForceIndicator();
            return;
        }

        Vector2 origin = bob.transform.position;
        Vector2 indicatorDirection = _lastAppliedBobForce == Vector2.zero
            ? Vector2.zero
            : _lastAppliedBobForce.normalized;
        float fullLengthForce = Mathf.Max(0.01f, pullForceIndicatorFullLengthForce);
        float targetLength = pullForceIndicatorLength
                             * Mathf.Clamp01(_lastAppliedBobForce.magnitude / fullLengthForce);
        float smoothing = 1f - Mathf.Exp(-pullForceIndicatorSmoothSpeed * Time.deltaTime);

        if (_smoothedPullIndicatorDirection == Vector2.zero)
        {
            _smoothedPullIndicatorDirection = indicatorDirection;
        }
        else if (indicatorDirection != Vector2.zero)
        {
            _smoothedPullIndicatorDirection = Vector2
                .Lerp(_smoothedPullIndicatorDirection, indicatorDirection, smoothing)
                .normalized;
        }

        _smoothedPullIndicatorLength = Mathf.Lerp(_smoothedPullIndicatorLength, targetLength, smoothing);

        SetGuideLine(
            _pullForceIndicator,
            origin,
            _smoothedPullIndicatorDirection,
            pullForceIndicatorColor,
            _smoothedPullIndicatorLength,
            pullForceIndicatorWidth);
    }

    private void UpdateMousePullVisual()
    {
        if (!showMousePullVisual || !_isReeling || !_hasMousePullAnchor || !bob || !bob.activeInHierarchy)
        {
            HideMousePullVisual();
            return;
        }

        if (!TryGetWorldPositionFromScreen(_mousePullAnchorScreenPosition, bob.transform.position.z, out Vector2 anchorPosition))
        {
            HideMousePullVisual();
            return;
        }

        Vector2 pointerScreenPosition = GetClampedPullScreenPosition();
        if (!TryGetWorldPositionFromScreen(pointerScreenPosition, bob.transform.position.z, out Vector2 pointerPosition))
        {
            HideMousePullVisual();
            return;
        }

        SetMousePullAnchorCircle(anchorPosition);
        SetMousePullLine(anchorPosition, pointerPosition, GetMousePullLineColor());
    }

    private void UpdateCounterPullGuide()
    {
        if (!showCounterPullGuide || !_hookedFish || !_hookedFish.IsFighting || !bob || !_hasMousePullAnchor)
        {
            HideCounterPullGuide();
            return;
        }

        Vector2 perfectCounterDirection = -_hookedFish.FightDirection;
        if (perfectCounterDirection == Vector2.zero)
        {
            HideCounterPullGuide();
            return;
        }

        if (!TryGetWorldPositionFromScreen(_mousePullAnchorScreenPosition, bob.transform.position.z, out Vector2 guideOrigin))
        {
            HideCounterPullGuide();
            return;
        }

        float guideLength = GetWorldDistanceFromScreenPixels(maxMousePullPixels, bob.transform.position.z);
        MousePullInput mousePullInput = GetMousePullInput();
        Color pullColor = GetCounterPullColor(GetCounterPullStrength(mousePullInput.Direction));

        SetGuideLineEnabled(_perfectCounterGuide, false);
        SetGuideLine(_perfectCounterLeftGuide, guideOrigin, Rotate(perfectCounterDirection, counterPullAngle), perfectCounterGuideColor, guideLength);
        SetGuideLine(_perfectCounterRightGuide, guideOrigin, Rotate(perfectCounterDirection, -counterPullAngle), perfectCounterGuideColor, guideLength);
        SetGuideLine(_assistCounterLeftGuide, guideOrigin, Rotate(perfectCounterDirection, counterPullAssistAngle), assistCounterGuideColor, guideLength * 0.85f);
        SetGuideLine(_assistCounterRightGuide, guideOrigin, Rotate(perfectCounterDirection, -counterPullAssistAngle), assistCounterGuideColor, guideLength * 0.85f);

        if (_hasMousePullAnchor && showMousePullVisual)
        {
            SetGuideLineEnabled(_currentPullGuide, false);
        }
        else
        {
            SetGuideLine(_currentPullGuide, guideOrigin, mousePullInput.Direction, pullColor, guideLength * mousePullInput.Strength);
        }
    }

    private void SetGuideLine(LineRenderer line, Vector2 origin, Vector2 direction, Color color, float length, float width = -1f)
    {
        if (!line)
        {
            return;
        }

        bool enabled = direction != Vector2.zero && length > 0f;
        line.enabled = enabled;

        if (!enabled)
        {
            return;
        }

        float lineWidthValue = width > 0f ? width : counterPullGuideWidth;
        line.startWidth = lineWidthValue;
        line.endWidth = lineWidthValue;
        line.startColor = color;
        line.endColor = color;
        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction.normalized * length);
    }

    private void HideCounterPullGuide()
    {
        SetGuideLineEnabled(_perfectCounterGuide, false);
        SetGuideLineEnabled(_perfectCounterLeftGuide, false);
        SetGuideLineEnabled(_perfectCounterRightGuide, false);
        SetGuideLineEnabled(_assistCounterLeftGuide, false);
        SetGuideLineEnabled(_assistCounterRightGuide, false);
        SetGuideLineEnabled(_currentPullGuide, false);
    }

    private void HidePullForceIndicator()
    {
        SetGuideLineEnabled(_pullForceIndicator, false);
        _smoothedPullIndicatorDirection = Vector2.zero;
        _smoothedPullIndicatorLength = 0f;
    }

    private void HideMousePullVisual()
    {
        SetGuideLineEnabled(_mousePullAnchorCircle, false);
        SetGuideLineEnabled(_mousePullLine, false);
    }

    private void SetMousePullAnchorCircle(Vector2 center)
    {
        if (!_mousePullAnchorCircle)
        {
            return;
        }

        int pointCount = Mathf.Max(8, _mousePullAnchorCircle.positionCount);
        _mousePullAnchorCircle.enabled = true;
        _mousePullAnchorCircle.positionCount = pointCount;
        _mousePullAnchorCircle.startWidth = mousePullVisualWidth;
        _mousePullAnchorCircle.endWidth = mousePullVisualWidth;
        _mousePullAnchorCircle.startColor = mousePullAnchorColor;
        _mousePullAnchorCircle.endColor = mousePullAnchorColor;

        float radius = Mathf.Max(0f, mousePullAnchorRadius);
        for (int i = 0; i < pointCount; i++)
        {
            float angle = (float)i / pointCount * Mathf.PI * 2f;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            _mousePullAnchorCircle.SetPosition(i, center + offset);
        }
    }

    private void SetMousePullLine(Vector2 anchorPosition, Vector2 pointerPosition, Color lineColor)
    {
        if (!_mousePullLine)
        {
            return;
        }

        bool hasLength = Vector2.Distance(anchorPosition, pointerPosition) > 0.001f;
        _mousePullLine.enabled = hasLength;
        if (!hasLength)
        {
            return;
        }

        _mousePullLine.startWidth = mousePullVisualWidth;
        _mousePullLine.endWidth = mousePullVisualWidth;
        _mousePullLine.startColor = lineColor;
        _mousePullLine.endColor = lineColor;
        _mousePullLine.SetPosition(0, anchorPosition);
        _mousePullLine.SetPosition(1, pointerPosition);
    }

    private Color GetMousePullLineColor()
    {
        if (!_hookedFish || !_hookedFish.IsFighting || _currentPullDirection == Vector2.zero)
        {
            return mousePullLineColor;
        }

        return GetCounterPullColor(_counterPullStrength);
    }

    private Color GetCounterPullColor(float counterStrength)
    {
        counterStrength = Mathf.Clamp01(counterStrength);
        if (counterStrength < 0.5f)
        {
            return Color.Lerp(Color.red, assistCounterGuideColor, counterStrength * 2f);
        }

        return Color.Lerp(assistCounterGuideColor, perfectCounterGuideColor, (counterStrength - 0.5f) * 2f);
    }

    private void SetGuideLineEnabled(LineRenderer line, bool enabled)
    {
        if (line)
        {
            line.enabled = enabled;
        }
    }

    private Vector2 Rotate(Vector2 direction, float degrees)
    {
        return Quaternion.Euler(0f, 0f, degrees) * direction;
    }

    private Vector2 GetFishingPointDirection()
    {
        if (!bob || !fishingPoint)
        {
            return Vector2.zero;
        }
        Vector2 bobPos = bob.transform.position;
        Vector2 fishingPointPos = fishingPoint.transform.position;

        _fishingPointDir = (fishingPointPos - bobPos).normalized;
        return _fishingPointDir;
    }

    private bool CanStartReeling()
    {
        return _thrown && _bobHasLeftFishingPoint && Time.time >= _reelReadyTime;
    }

    private Vector2 GetReelForce()
    {
        Vector2 fishingPointDirection = GetFishingPointDirection();
        MousePullInput mousePullInput = GetMousePullInput();
        _currentPullDirection = mousePullInput.Direction;
        _currentPullStrength = mousePullInput.Strength;
        _counterPullStrength = GetCounterPullStrength(_currentPullDirection);
        _counterPullingThisFrame = _currentPullStrength > 0f && _counterPullStrength > 0f;

        float mouseForce = _hookedFish ? hookedMouseReelForce : emptyMouseReelForce;
        mouseForce *= _currentPullStrength;
        float reelForce = reelAcceleration * GetReelMultiplier();

        if (_counterPullingThisFrame)
        {
            float counterPullMultiplier = Mathf.Lerp(1f, counterPullForceMultiplier, _counterPullStrength);
            reelForce *= counterPullMultiplier;
            mouseForce *= counterPullMultiplier;
            _hookedFish.ApplyCounterPull(_currentPullStrength * _counterPullStrength, Time.fixedDeltaTime);
        }

        if (IsSlidingAlongSideBoundary())
        {
            reelForce *= sideBoundaryReelMultiplier;
            mouseForce *= sideBoundaryReelMultiplier;
        }

        return fishingPointDirection * reelForce + _currentPullDirection * mouseForce;
    }

    private float GetReelMultiplier()
    {
        if (!_hookedFish)
        {
            return 1f;
        }

        return _hookedFish.IsFighting ? fightingReelMultiplier : restingReelMultiplier;
    }

    private float GetMaxBobSpeed()
    {
        if (!_hookedFish)
        {
            return maxReelSpeed;
        }

        return Mathf.Lerp(hookedMaxReelSpeed, counterPullMaxReelSpeed, _counterPullStrength);
    }

    private MousePullInput GetMousePullInput()
    {
        if (!bob || !_hasMousePullAnchor)
        {
            return MousePullInput.None;
        }

        if (!TryGetPointerScreenPosition(out _))
        {
            return MousePullInput.None;
        }

        Vector2 pointerScreenPosition = GetClampedPullScreenPosition();
        Vector2 pullOffset = pointerScreenPosition - _mousePullAnchorScreenPosition;
        float pullDistance = pullOffset.magnitude;

        if (pullDistance <= 0f || maxMousePullPixels <= 0f)
        {
            return MousePullInput.None;
        }

        float minDistance = Mathf.Max(0f, minMousePullPixels);
        float maxDistance = Mathf.Max(minDistance + 1f, maxMousePullPixels);
        float pullStrength = Mathf.InverseLerp(minDistance, maxDistance, pullDistance);
        Vector2 pullDirection = GetWorldDirectionFromScreenDelta(pullOffset, bob.transform.position.z);

        return new MousePullInput(pullDirection, pullStrength);
    }

    private float GetCounterPullStrength(Vector2 pullDirection)
    {
        if (!_hookedFish || !_hookedFish.IsFighting || pullDirection == Vector2.zero)
        {
            return 0f;
        }

        Vector2 counterDirection = -_hookedFish.FightDirection;
        if (counterDirection == Vector2.zero)
        {
            return 0f;
        }

        float angle = Vector2.Angle(pullDirection, counterDirection);
        if (angle <= counterPullAngle)
        {
            return 1f;
        }

        if (angle >= counterPullAssistAngle)
        {
            return 0f;
        }

        return 1f - Mathf.InverseLerp(counterPullAngle, counterPullAssistAngle, angle);
    }

    private void UpdateLineTension(float deltaTime)
    {
        if (IsLineLocked)
        {
            return;
        }

        if (!_hookedFish)
        {
            RecoverLineTension(deltaTime);
            return;
        }

        if (!_isReeling)
        {
            UpdateBoundaryTension(deltaTime);
            RecoverLineTension(deltaTime);
            ClampTensionAndSnapIfNeeded();
            return;
        }

        if (!_hookedFish.IsFighting)
        {
            UpdateBoundaryTension(deltaTime);
            RecoverLineTension(deltaTime, GetGentlePullTensionRecoveryMultiplier());
            ClampTensionAndSnapIfNeeded();
            return;
        }

        float counterTensionMultiplier = Mathf.Lerp(1f, counterPullTensionMultiplier, _counterPullStrength);
        _lineTension += _currentPullStrength
                        * _hookedFish.FightIntensity
                        * tensionBuildSpeed
                        * counterTensionMultiplier
                        * GetHookedFishTensionMultiplier()
                        * deltaTime;
        UpdateBoundaryTension(deltaTime);
        RecoverLineTension(deltaTime, GetGentlePullTensionRecoveryMultiplier());
        ClampTensionAndSnapIfNeeded();
    }

    private void RecoverLineTension(float deltaTime)
    {
        RecoverLineTension(deltaTime, 1f);
    }

    private void RecoverLineTension(float deltaTime, float multiplier)
    {
        if (multiplier <= 0f)
        {
            return;
        }

        _lineTension = Mathf.Max(0f, _lineTension - tensionRecoverSpeed * multiplier * deltaTime);
    }

    private float GetGentlePullTensionRecoveryMultiplier()
    {
        if (gentlePullMaxStrength <= 0f)
        {
            return 0f;
        }

        float gentlePullPercent = 1f - Mathf.InverseLerp(0f, gentlePullMaxStrength, _currentPullStrength);
        return gentlePullPercent * gentlePullTensionRecoverMultiplier;
    }

    private void ClampTensionAndSnapIfNeeded()
    {
        _lineTension = Mathf.Clamp(_lineTension, 0f, lineSnapTension);

        if (_lineTension >= lineSnapTension)
        {
            SnapLine();
        }
    }

    private void UpdateBoundaryTension(float deltaTime)
    {
        if (!_hookedFish || !_isReeling || !_pinnedToSideBoundary || PullsAwayFromSideBoundary())
        {
            return;
        }

        float intensity = _hookedFish.IsFighting ? _hookedFish.FightIntensity : 0.5f;
        _lineTension += _currentPullStrength
                        * intensity
                        * sideBoundaryTensionBuildSpeed
                        * GetHookedFishTensionMultiplier()
                        * deltaTime;
    }

    private float GetHookedFishTensionMultiplier()
    {
        return _hookedFish ? _hookedFish.TensionDamageMultiplier : 1f;
    }

    private void SnapLine()
    {
        if (!_snapInProgress)
        {
            StartCoroutine(SnapLineSequence());
        }
    }

    private IEnumerator SnapLineSequence()
    {
        _snapInProgress = true;
        _thrown = false;
        _isReeling = false;
        _isMouseButtonHeld = false;
        _hasStartedReelingThisThrow = false;
        _bobHasLeftFishingPoint = false;
        _lineTension = 0f;
        ClearLineDistanceLock();
        ResetFishingAudioState();
        ClearMousePullAnchor();
        HideCounterPullGuide();
        HidePullForceIndicator();

        Fish escapingFish = _hookedFish;
        _hookedFish = null;

        if (escapingFish)
        {
            escapingFish.EscapeFromHook(fishEscapeFadeDuration, fishEscapeSinkSpeed);
        }

        if (_bobRb)
        {
            _bobRb.linearVelocity = Vector2.zero;
            _bobRb.angularVelocity = 0f;
        }

        if (bob)
        {
            bob.SetActive(false);
        }

        yield return new WaitForSeconds(bobRespawnDelay);

        ResetBobForThrow();

        if (bob)
        {
            bob.SetActive(true);
        }

        _snapInProgress = false;
    }

    private bool ShouldLockLineDistance()
    {
        return _thrown
               && _bobHasLeftFishingPoint
               && _hookedFish
               && bob
               && fishingPoint
               && (!_inputReady || (_hasStartedReelingThisThrow && !_isMouseButtonHeld));
    }

    private void LockLineDistance()
    {
        if (!_bobRb || !bob || !fishingPoint)
        {
            return;
        }

        Vector2 fishingPointPosition = fishingPoint.transform.position;
        Vector2 bobPosition = _bobRb.position;
        Vector2 fishingPointToBob = bobPosition - fishingPointPosition;
        float currentDistance = fishingPointToBob.magnitude;

        if (!_lineDistanceLocked)
        {
            _lockedLineDistance = currentDistance;
            _lineDistanceLocked = true;
        }

        if (currentDistance > 0.001f)
        {
            Vector2 lineDirection = fishingPointToBob.normalized;
            Vector2 sideDirection = new Vector2(-lineDirection.y, lineDirection.x);
            float sideSpeed = Vector2.Dot(_bobRb.linearVelocity, sideDirection);

            _bobRb.position = fishingPointPosition + lineDirection * _lockedLineDistance;
            _bobRb.linearVelocity = sideDirection * sideSpeed;
        }
        else
        {
            _bobRb.linearVelocity = Vector2.zero;
        }

        _bobRb.angularVelocity = 0f;
        _currentPullStrength = 0f;
        _currentPullDirection = Vector2.zero;
        _counterPullingThisFrame = false;
        _counterPullStrength = 0f;
        _lastAppliedBobForce = Vector2.zero;
    }

    private void ClearLineDistanceLock()
    {
        _lineDistanceLocked = false;
        _lockedLineDistance = 0f;
    }

    private void UpdateFishingLineVisuals()
    {
        float tensionPercent = lineSnapTension <= 0f ? 0f : Mathf.Clamp01(_lineTension / lineSnapTension);
        float width = Mathf.Lerp(lineWidth, highTensionLineWidth, tensionPercent);
        Color color = Color.Lerp(lineColor, highTensionLineColor, tensionPercent);

        fishingLine.startWidth = width;
        fishingLine.endWidth = width;
        fishingLine.startColor = color;
        fishingLine.endColor = color;
    }

    private void KeepBobInPlayableArea()
    {
        _pinnedToSideBoundary = false;
        _sideBoundaryReleaseDirection = Vector2.zero;

        if (!bob || (!_bobHasLeftFishingPoint && !_hookedFish))
        {
            return;
        }

        Vector2 bobPosition = _bobRb ? _bobRb.position : bob.transform.position;
        Vector2 clampedPosition = bobPosition;

        if (TryGetPlayableArea(out float minX, out float maxX, out float minY, out float maxY))
        {
            clampedPosition.x = Mathf.Clamp(
                clampedPosition.x,
                minX,
                maxX);

            if (!Mathf.Approximately(clampedPosition.x, bobPosition.x))
            {
                _pinnedToSideBoundary = true;
                _sideBoundaryReleaseDirection = bobPosition.x < minX ? Vector2.right : Vector2.left;
            }

            clampedPosition.y = Mathf.Clamp(
                clampedPosition.y,
                minY,
                maxY);
        }

        if (keepBobAboveFishingPoint && fishingPoint)
        {
            float fishingPointMinY = fishingPoint.transform.position.y + minFishingPointYOffset;
            clampedPosition.y = Mathf.Max(clampedPosition.y, fishingPointMinY);
        }

        if (clampedPosition == bobPosition)
        {
            return;
        }

        if (_bobRb)
        {
            Vector2 velocity = _bobRb.linearVelocity;
            bool hitBoundary = clampedPosition != bobPosition;

            if (!_hookedFish && hitBoundary)
            {
                velocity = Vector2.zero;
            }
            else
            {
                if (!Mathf.Approximately(clampedPosition.x, bobPosition.x))
                {
                    velocity.x = 0f;
                }

                if (!Mathf.Approximately(clampedPosition.y, bobPosition.y))
                {
                    velocity.y = 0f;
                }
            }

            _bobRb.position = clampedPosition;
            _bobRb.linearVelocity = velocity;

            if (_hookedFish && _pinnedToSideBoundary)
            {
                Vector2 sideBoundaryForce = _sideBoundaryReleaseDirection * sideBoundaryReleaseForce;
                _lastAppliedBobForce += sideBoundaryForce;
                _bobRb.AddForce(sideBoundaryForce, ForceMode2D.Force);
            }

            return;
        }

        bob.transform.position = clampedPosition;
    }

    private bool TryGetPlayableArea(out float minX, out float maxX, out float minY, out float maxY)
    {
        minX = maxX = minY = maxY = 0f;

        if (!keepBobInsideCamera)
        {
            return false;
        }

        if (!_camera)
        {
            ResolveCamera();
        }

        if (!_camera || !bob)
        {
            return false;
        }

        float distanceFromCamera = Mathf.Abs(_camera.transform.position.z - bob.transform.position.z);
        Vector3 bottomLeft = _camera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceFromCamera));
        Vector3 topRight = _camera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCamera));

        minX = bottomLeft.x + cameraBoundsPadding;
        maxX = topRight.x - cameraBoundsPadding;
        minY = bottomLeft.y + cameraBoundsPadding;
        maxY = topRight.y - cameraBoundsPadding;

        if (keepBobAboveFishingPoint && fishingPoint)
        {
            minY = Mathf.Max(minY, fishingPoint.transform.position.y + minFishingPointYOffset);
        }

        return minX <= maxX && minY <= maxY;
    }

    private static void AddEdgeEscape(
        float distanceToEdge,
        Vector2 awayFromEdge,
        float edgeDistance,
        ref Vector2 escapeDirection,
        ref float edgePressure)
    {
        float pressure = 1f - Mathf.Clamp01(distanceToEdge / edgeDistance);
        if (pressure <= 0f)
        {
            return;
        }

        escapeDirection += awayFromEdge * pressure;
        edgePressure = Mathf.Max(edgePressure, pressure);
    }

    private bool IsSlidingAlongSideBoundary()
    {
        return _hookedFish && _pinnedToSideBoundary && !PullsAwayFromSideBoundary();
    }

    private bool PullsAwayFromSideBoundary()
    {
        return _sideBoundaryReleaseDirection != Vector2.zero
               && _currentPullDirection != Vector2.zero
               && Vector2.Dot(_currentPullDirection, _sideBoundaryReleaseDirection) > 0.4f;
    }

    private readonly struct MousePullInput
    {
        public static readonly MousePullInput None = new MousePullInput(Vector2.zero, 0f);

        public readonly Vector2 Direction;
        public readonly float Strength;

        public MousePullInput(Vector2 direction, float strength)
        {
            Direction = direction;
            Strength = strength;
        }
    }
    
    private void GetThrowDirection()
    {
        if (!TryGetPointerWorldPosition(transform.position.z, out Vector2 mouseWorldPosition))
        {
            _throwDir = Vector2.zero;
            _throwForce = Vector2.zero;
            return;
        }

        Vector2 fishingPointPosition = fishingPoint ? fishingPoint.transform.position : transform.position;
        Vector2 throwTarget = mouseWorldPosition;

        if (TryGetPlayableArea(out float minX, out float maxX, out float minY, out float maxY))
        {
            throwTarget.x = Mathf.Clamp(throwTarget.x, minX, maxX);
            throwTarget.y = Mathf.Clamp(throwTarget.y, minY, maxY);
        }

        Vector2 throwVector = throwTarget - fishingPointPosition;

        _throwDir = throwVector.normalized;
        _throwForce = _throwDir * (throwVector.magnitude * throwForceMultiplier);
    }

    private bool IsBobAtFishingPoint()
    {
        if (!bob || !fishingPoint)
        {
            return false;
        }

        Vector2 bobPos = bob.transform.position;
        Vector2 fishingPointPos = fishingPoint.transform.position;

        return Vector2.Distance(bobPos, fishingPointPos) <= readyDistance;
    }

    private void ResolveCamera()
    {
        if (!fishingCamera)
        {
            fishingCamera = GetComponentInParent<FishingMiniGame>()?.GetComponentInChildren<Camera>(true);
        }

        _camera = fishingCamera ? fishingCamera : Camera.main;
    }

    private bool IsPointerInputAvailable()
    {
        if (!_inputReady)
        {
            return false;
        }

        if (_crtInputRelay)
        {
            return true;
        }

        if (!_camera)
        {
            ResolveCamera();
        }

        return _camera && Mouse.current != null;
    }

    private bool IsPointerHeld()
    {
        if (_crtInputRelay)
        {
            if (!_crtInputRelay.IsPointerActive)
            {
                _relayPointerHeld = false;
            }

            return _relayPointerHeld;
        }

        return Mouse.current != null && Mouse.current.leftButton.isPressed;
    }

    private bool WasPointerPressedThisFrame()
    {
        if (_crtInputRelay)
        {
            return _relayPointerPressedThisFrame;
        }

        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private void UpdateMousePullAnchor()
    {
        if (!TryGetPointerScreenPosition(out Vector2 pointerScreenPosition))
        {
            ClearMousePullAnchor();
            return;
        }

        if (!_hasMousePullAnchor)
        {
            _mousePullAnchorScreenPosition = pointerScreenPosition;
            _hasMousePullAnchor = true;
            return;
        }

        LockPointerToMaxPullDistance(pointerScreenPosition);
    }

    private void ClearMousePullAnchor()
    {
        _hasMousePullAnchor = false;
        _mousePullAnchorScreenPosition = Vector2.zero;
        HideMousePullVisual();
    }

    private bool TryGetPointerScreenPosition(out Vector2 screenPosition)
    {
        if (_crtInputRelay)
        {
            if (!_hasRelayPointerPosition)
            {
                screenPosition = default;
                return false;
            }

            if (!_camera)
            {
                ResolveCamera();
            }

            if (_camera)
            {
                screenPosition = _camera.WorldToScreenPoint(_relayPointerWorldPosition);
                return true;
            }

            screenPosition = _relayPointerWorldPosition;
            return true;
        }

        if (Mouse.current == null)
        {
            screenPosition = default;
            return false;
        }

        screenPosition = Mouse.current.position.ReadValue();
        return true;
    }

    private Vector2 GetClampedPullScreenPosition()
    {
        if (!_hasMousePullAnchor || !TryGetPointerScreenPosition(out Vector2 pointerScreenPosition))
        {
            return _mousePullAnchorScreenPosition;
        }

        Vector2 pullOffset = pointerScreenPosition - _mousePullAnchorScreenPosition;
        float maxDistance = Mathf.Max(1f, maxMousePullPixels);
        if (pullOffset.magnitude <= maxDistance)
        {
            return pointerScreenPosition;
        }

        return _mousePullAnchorScreenPosition + pullOffset.normalized * maxDistance;
    }

    private void LockPointerToMaxPullDistance(Vector2 pointerScreenPosition)
    {
        if (!_hasMousePullAnchor)
        {
            return;
        }

        Vector2 pullOffset = pointerScreenPosition - _mousePullAnchorScreenPosition;
        float maxDistance = Mathf.Max(1f, maxMousePullPixels);
        if (pullOffset.magnitude <= maxDistance)
        {
            return;
        }

        Vector2 lockedScreenPosition = _mousePullAnchorScreenPosition + pullOffset.normalized * maxDistance;
        if (_crtInputRelay)
        {
            float targetZ = bob ? bob.transform.position.z : transform.position.z;
            if (TryGetWorldPositionFromScreen(lockedScreenPosition, targetZ, out Vector2 lockedWorldPosition))
            {
                _relayPointerWorldPosition = lockedWorldPosition;
            }

            return;
        }

        Mouse.current?.WarpCursorPosition(lockedScreenPosition);
    }

    private bool TryGetWorldPositionFromScreen(Vector2 screenPosition, float targetZ, out Vector2 worldPosition)
    {
        if (!_camera)
        {
            ResolveCamera();
        }

        if (!_camera)
        {
            worldPosition = default;
            return false;
        }

        float cameraDistance = Mathf.Abs(_camera.transform.position.z - targetZ);
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, cameraDistance);
        worldPosition = _camera.ScreenToWorldPoint(screenPoint);
        return true;
    }

    private float GetWorldDistanceFromScreenPixels(float pixels, float targetZ)
    {
        if (!_hasMousePullAnchor)
        {
            return counterPullGuideLength;
        }

        if (!TryGetWorldPositionFromScreen(_mousePullAnchorScreenPosition, targetZ, out Vector2 startPosition)
            || !TryGetWorldPositionFromScreen(_mousePullAnchorScreenPosition + Vector2.right * pixels, targetZ, out Vector2 endPosition))
        {
            return counterPullGuideLength;
        }

        return Mathf.Max(0.01f, Vector2.Distance(startPosition, endPosition));
    }

    private Vector2 GetWorldDirectionFromScreenDelta(Vector2 screenDelta, float targetZ)
    {
        if (screenDelta == Vector2.zero)
        {
            return Vector2.zero;
        }

        if (!TryGetWorldPositionFromScreen(_mousePullAnchorScreenPosition, targetZ, out Vector2 anchorWorldPosition)
            || !TryGetWorldPositionFromScreen(_mousePullAnchorScreenPosition + screenDelta, targetZ, out Vector2 pointerWorldPosition))
        {
            return screenDelta.normalized;
        }

        Vector2 worldDelta = pointerWorldPosition - anchorWorldPosition;
        return worldDelta == Vector2.zero ? Vector2.zero : worldDelta.normalized;
    }

    private bool TryGetPointerWorldPosition(float targetZ, out Vector2 worldPosition)
    {
        if (_crtInputRelay)
        {
            worldPosition = _relayPointerWorldPosition;
            return _hasRelayPointerPosition;
        }

        if (!_camera || Mouse.current == null)
        {
            worldPosition = default;
            return false;
        }

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = Mathf.Abs(_camera.transform.position.z - targetZ);
        worldPosition = _camera.ScreenToWorldPoint(mousePosition);
        return true;
    }

    private void HandleRelayPointerDown(Vector3 worldPosition)
    {
        if (!_inputReady)
        {
            return;
        }

        _relayPointerWorldPosition = worldPosition;
        _hasRelayPointerPosition = true;
        _relayPointerHeld = true;
        _relayPointerPressedThisFrame = true;
    }

    private void HandleRelayPointerDrag(Vector3 worldPosition)
    {
        if (!_inputReady)
        {
            return;
        }

        _relayPointerWorldPosition = worldPosition;
        _hasRelayPointerPosition = true;
        _relayPointerHeld = true;
    }

    private void HandleRelayPointerUp(Vector3 worldPosition)
    {
        _relayPointerWorldPosition = worldPosition;
        _hasRelayPointerPosition = true;
        _relayPointerHeld = false;
    }

    private void ClearFrameInput()
    {
        _relayPointerPressedThisFrame = false;
    }

    private void UpdateInputFocusReadiness()
    {
        if (!_hasInputFocus)
        {
            _inputReady = false;
            return;
        }

        if (_inputReady)
        {
            return;
        }

        if (Time.unscaledTime < _inputUnlockTime)
        {
            return;
        }

        if (_waitingForPointerRelease)
        {
            if (IsAnyPointerHeld())
            {
                return;
            }

            _waitingForPointerRelease = false;
        }

        _inputReady = true;
    }

    private bool IsAnyPointerHeld()
    {
        return _relayPointerHeld
               || (_crtInputRelay && _crtInputRelay.IsPointerActive)
               || (Mouse.current != null && Mouse.current.leftButton.isPressed);
    }

    private void StopActiveInput()
    {
        _isMouseButtonHeld = false;
        _isReeling = false;
        ResetRelayPointerState();
        HidePullForceIndicator();
        _currentPullStrength = 0f;
        _currentPullDirection = Vector2.zero;
        _counterPullingThisFrame = false;
        _counterPullStrength = 0f;
        _lastAppliedBobForce = Vector2.zero;
        ClearMousePullAnchor();
    }

    private void PrepareInputForNextThrow()
    {
        bool pointerWasHeld = IsAnyPointerHeld();
        ResetRelayPointerState();

        if (!_hasInputFocus)
        {
            _inputReady = false;
            _waitingForPointerRelease = false;
            _inputUnlockTime = 0f;
            return;
        }

        _waitingForPointerRelease = pointerWasHeld;
        if (!_waitingForPointerRelease)
        {
            return;
        }

        _inputReady = false;
        _inputUnlockTime = Time.unscaledTime + focusInputDelay;
    }

    private void ResetRelayPointerState()
    {
        _relayPointerHeld = false;
        _relayPointerPressedThisFrame = false;
        _hasRelayPointerPosition = false;
        _relayPointerWorldPosition = Vector2.zero;
    }
}
