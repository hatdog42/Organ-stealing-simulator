using UnityEngine;

public class FishAnimationEventRelay : MonoBehaviour
{
    private Fish _fish;

    public void SetFish(Fish fish)
    {
        _fish = fish;
    }

    private void Awake()
    {
        ResolveFish();
    }

    public void PlaySloshBlobSfx()
    {
        ResolveFish();
        _fish?.PlaySloshBlobSfx();
    }

    private void ResolveFish()
    {
        if (!_fish)
        {
            _fish = GetComponentInParent<Fish>();
        }
    }
}
