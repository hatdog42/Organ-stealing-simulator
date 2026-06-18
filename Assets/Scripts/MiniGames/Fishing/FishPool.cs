using System.Collections.Generic;
using UnityEngine;

public class FishPool : MonoBehaviour
{
    [SerializeField] private Fish _fishPrefab;
    [SerializeField] private int _startAmount = 5;
    
    private readonly Queue<Fish> _pool = new Queue<Fish>();
    
    private void Awake()
    {
        for (int i = 0; i < _startAmount; i++)
        {
            CreateFish();
        }
    }
    private Fish CreateFish()
    {
        Fish fish = Instantiate(_fishPrefab, transform);
        fish.gameObject.SetActive(false);
        fish.SetPool(this);

        _pool.Enqueue(fish);
        return fish;
    }

    public Fish GetFish()
    {
        if (_pool.Count == 0)
        {
            CreateFish();
        }

        Fish fish = _pool.Dequeue();
        fish.gameObject.SetActive(true);

        return fish;
    }

    public void ReturnFish(Fish fish)
    {
        fish.gameObject.SetActive(false);
        fish.transform.SetParent(transform);
        _pool.Enqueue(fish);
    }
}
