using System.Collections;
using UnityEngine;

public class Ending : MonoBehaviour
{
    [SerializeField, Min(0f)] private float secondsBeforeCredits = 12f;

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(secondsBeforeCredits);
        SceneController.Instance.LoadScene("Credits");
    }
}
