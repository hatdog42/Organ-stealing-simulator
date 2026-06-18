using UnityEngine;

public class FishManager : MonoBehaviour
{
    [SerializeField] private FishPool _fishPool;

    [Header("Spawn Area")]
    [Tooltip("Local offset from this FishManager transform.")]
    [SerializeField] private Vector2 _spawnAreaCenter = Vector2.zero;
    [SerializeField] private Vector2 _spawnAreaSize = new Vector2(10f, 5f);

    [Header("Spawn Timing")]
    [SerializeField] private float _minSpawnInterval = 1f;
    [SerializeField] private float _maxSpawnInterval = 5f;

    [Header("Fish Stats")]
    [SerializeField] private float _minSpeed = 0.7f;
    [SerializeField] private float _maxSpeed = 1.5f;
    
    private float _spawnTimer;
    
    private void Update()
    {
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnFish();
            _spawnTimer = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        }
    }

    private void SpawnFish()
    {
        Fish fish = _fishPool.GetFish();
        
        fish.transform.position = GetRandomPosition();
        fish.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        
        float randomSpeed = Random.Range(_minSpeed, _maxSpeed);
        fish.Setup(randomSpeed);
    }
    private Vector2 GetRandomPosition()
    {
        Vector2 halfSize = _spawnAreaSize / 2f;

        float x = Random.Range(_spawnAreaCenter.x - halfSize.x, _spawnAreaCenter.x + halfSize.x);
        float y = Random.Range(_spawnAreaCenter.y - halfSize.y, _spawnAreaCenter.y + halfSize.y);

        return transform.TransformPoint(new Vector3(x, y, 0f));
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_spawnAreaCenter, _spawnAreaSize);
        Gizmos.matrix = previousMatrix;
    }
}
