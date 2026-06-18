using MiniGames.Base;
using UnityEngine;

public class FishingMiniGame : MiniGameBase
{
    [Header("Catch Goal")]
    [SerializeField] private int _fishQuota = 5;

    [Header("Fishing Controls")]
    [SerializeField] private FishingRodController _rodController;

    [Header("Petri Dish")]
    [SerializeField] private Bacteria _bacteriaPrefab;
    [SerializeField] private Transform _petriDish;
    [SerializeField] private Collider2D _petriDishBounds;
    [SerializeField] private float _fallbackDishRadius = 1f;
    [SerializeField] private float _bacteriaBoundsPadding = 0.1f;

    [Header("Bacteria Spawn")]
    [SerializeField] private float _bacteriaScale = 0.15f;

    private int _fishCaught;

    public int FishCaught => _fishCaught;
    public int FishQuota => _fishQuota;

    private void Start()
    {
        EnsureRodController();
    }

    public override void OnFocusGained(TVInputRelay relay)
    {
        base.OnFocusGained(relay);
        EnsureRodController();

        if (_rodController)
        {
            _rodController.SetCrtInput(relay);
            _rodController.SetInputFocus(true);
        }
    }

    public override void OnFocusLost()
    {
        if (_rodController)
        {
            _rodController.SetInputFocus(false);
            _rodController.SetCrtInput(null);
            _rodController.StopFishingAudio();
        }

        StopFishAudio();
        base.OnFocusLost();
    }

    public void RegisterCaughtFish()
    {
        _fishCaught++;
        SpawnBacteria();

        if (_fishCaught >= _fishQuota)
        {
            GameWin();
        }
    }

    private void SpawnBacteria()
    {
        if (!_bacteriaPrefab || !_petriDish)
        {
            return;
        }

        Vector2 spawnPosition = GetRandomDishPosition();
        Bacteria bacteria = Instantiate(
            _bacteriaPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)),
            _petriDish);

        bacteria.transform.localScale = Vector3.one * _bacteriaScale;
        bacteria.Setup(_petriDish, _petriDishBounds, _fallbackDishRadius, _bacteriaBoundsPadding);
    }

    private Vector2 GetRandomDishPosition()
    {
        if (_petriDishBounds is CircleCollider2D circle)
        {
            Transform circleTransform = circle.transform;
            Vector2 center = circleTransform.TransformPoint(circle.offset);
            float scale = Mathf.Max(Mathf.Abs(circleTransform.lossyScale.x), Mathf.Abs(circleTransform.lossyScale.y));
            float radius = Mathf.Max(0.01f, circle.radius * scale - _bacteriaBoundsPadding);

            return center + Random.insideUnitCircle * radius;
        }

        if (_petriDishBounds)
        {
            Bounds bounds = _petriDishBounds.bounds;

            for (int i = 0; i < 20; i++)
            {
                Vector2 position = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y));

                if (_petriDishBounds.OverlapPoint(position))
                {
                    return position;
                }
            }

            return _petriDishBounds.ClosestPoint(_petriDish.position);
        }

        float fallbackRadius = Mathf.Max(0.01f, _fallbackDishRadius - _bacteriaBoundsPadding);
        Vector2 randomPoint = Random.insideUnitCircle * fallbackRadius;
        return (Vector2)_petriDish.position + randomPoint;
    }

    private void EnsureRodController()
    {
        if (!_rodController)
        {
            _rodController = GetComponentInChildren<FishingRodController>(true);
        }
    }

    private void StopFishAudio()
    {
        foreach (Fish fish in GetComponentsInChildren<Fish>(true))
        {
            fish.StopAudio();
        }
    }
}
