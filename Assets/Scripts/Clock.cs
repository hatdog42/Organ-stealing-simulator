using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    [SerializeField]private float duration = 10f;
    [SerializeField]private Image clockImage;
    private float _remainingTime;
    void Start()
    {
        clockImage = GetComponent<Image>();
        
        _remainingTime = duration;
    }

    void Update()
    {
        if (_remainingTime > 0)
        {
            _remainingTime -= Time.deltaTime;
            clockImage.fillAmount = _remainingTime / duration;
        }
        else
        {
            TimerFinished();
        }
    }

    public void TimerFinished()
    {
        Debug.Log("TimerFinished"); //you loose
    }
}
