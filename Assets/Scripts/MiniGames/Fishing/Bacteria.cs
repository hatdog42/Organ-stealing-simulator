using UnityEngine;

public class Bacteria : MonoBehaviour
{
    [Header("Floating Movement")]
    [SerializeField] private Vector2 _localForwardDirection = new Vector2(-1f, 1f);
    [SerializeField] private Vector2 _directionChangeTime = new Vector2(0.7f, 1.8f);
    [SerializeField] private Vector2 _floatSpeed = new Vector2(0.03f, 0.08f);
    [SerializeField] private float _directionSmoothness = 4f;
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private float _wobbleStrength = 0.02f;
    [SerializeField] private float _wobbleFrequency = 1.5f;
    [SerializeField] private float _edgeTurnInStrength = 0.7f;

    [Header("Dish Bounds")]
    [SerializeField] private float _fallbackDishRadius = 1f;
    [SerializeField] private float _boundsPadding = 0.1f;

    private Transform _dish;
    private Collider2D _dishBounds;
    private Rigidbody2D _rigidbody;
    private Vector2 _currentDirection;
    private Vector2 _targetDirection;
    private float _directionTimer;
    private float _currentSpeed;
    private float _wobbleOffset;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        ConfigureRigidbody();
    }

    public void Setup(Transform dish, Collider2D dishBounds, float fallbackDishRadius, float boundsPadding)
    {
        _dish = dish;
        _dishBounds = dishBounds;
        _fallbackDishRadius = fallbackDishRadius;
        _boundsPadding = boundsPadding;
        ConfigureRigidbody();
        transform.position = KeepInsideDish(transform.position);
        StartFloating();
    }

    private void OnEnable()
    {
        ConfigureRigidbody();
        StartFloating();
    }

    private void Update()
    {
        UpdateFloating();
    }

    private void StartFloating()
    {
        _wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        PickNewDirection();
        _currentDirection = _targetDirection;
    }

    private void PickNewDirection()
    {
        _targetDirection = Random.insideUnitCircle.normalized;
        if (_targetDirection == Vector2.zero)
        {
            _targetDirection = Vector2.up;
        }

        _currentSpeed = RandomFromRange(_floatSpeed);
        _directionTimer = RandomFromRange(_directionChangeTime);
    }

    private void UpdateFloating()
    {
        _directionTimer -= Time.deltaTime;
        if (_directionTimer <= 0f)
        {
            PickNewDirection();
        }

        float directionBlend = 1f - Mathf.Exp(-_directionSmoothness * Time.deltaTime);
        _currentDirection = Vector2.Lerp(_currentDirection, _targetDirection, directionBlend).normalized;

        Vector2 sideways = new Vector2(-_currentDirection.y, _currentDirection.x);
        Vector2 wobble = sideways * (Mathf.Sin(Time.time * _wobbleFrequency + _wobbleOffset) * _wobbleStrength);
        Vector2 nextPosition = (Vector2)transform.position + (_currentDirection * _currentSpeed + wobble) * Time.deltaTime;
        Vector2 clampedPosition = KeepInsideDish(nextPosition);

        if ((clampedPosition - nextPosition).sqrMagnitude > 0.000001f)
        {
            Vector2 towardCenter = (GetDishCenter() - clampedPosition).normalized;
            _targetDirection = Vector2.Lerp(_targetDirection, towardCenter, _edgeTurnInStrength).normalized;
        }

        transform.position = clampedPosition;
        RotateTowardDirection(_currentDirection);
    }

    private void RotateTowardDirection(Vector2 direction)
    {
        if (direction == Vector2.zero || _localForwardDirection == Vector2.zero)
        {
            return;
        }

        float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float forwardAngle = Mathf.Atan2(_localForwardDirection.y, _localForwardDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, directionAngle - forwardAngle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private Vector2 KeepInsideDish(Vector2 position)
    {
        if (_dishBounds is CircleCollider2D circle)
        {
            Transform circleTransform = circle.transform;
            Vector2 circleCenter = circleTransform.TransformPoint(circle.offset);
            float scale = Mathf.Max(Mathf.Abs(circleTransform.lossyScale.x), Mathf.Abs(circleTransform.lossyScale.y));
            float circleRadius = Mathf.Max(0.01f, circle.radius * scale - _boundsPadding);
            Vector2 circleOffset = position - circleCenter;

            return circleOffset.magnitude <= circleRadius ? position : circleCenter + circleOffset.normalized * circleRadius;
        }

        if (_dishBounds && _dishBounds.OverlapPoint(position))
        {
            return position;
        }

        if (_dishBounds)
        {
            Vector2 closestPoint = _dishBounds.ClosestPoint(position);
            Vector2 boundsCenter = _dishBounds.bounds.center;
            Vector2 towardCenter = boundsCenter - closestPoint;

            return closestPoint + (towardCenter == Vector2.zero ? Vector2.zero : towardCenter.normalized * _boundsPadding);
        }

        if (!_dish)
        {
            return position;
        }

        Vector2 dishCenter = _dish.position;
        Vector2 dishOffset = position - dishCenter;
        float fallbackRadius = Mathf.Max(0.01f, _fallbackDishRadius - _boundsPadding);

        return dishOffset.magnitude <= fallbackRadius ? position : dishCenter + dishOffset.normalized * fallbackRadius;
    }

    private void ConfigureRigidbody()
    {
        if (!_rigidbody)
        {
            return;
        }

        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.gravityScale = 0f;
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
    }

    private Vector2 GetDishCenter()
    {
        if (_dishBounds is CircleCollider2D circle)
        {
            return circle.transform.TransformPoint(circle.offset);
        }

        if (_dishBounds)
        {
            return _dishBounds.bounds.center;
        }

        return _dish ? _dish.position : transform.position;
    }

    private float RandomFromRange(Vector2 range)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);

        return Mathf.Approximately(min, max) ? min : Random.Range(min, max);
    }
}
