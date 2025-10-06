using System;
using System.Collections;
using UnityEngine;

public class Ending : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(5f);
        SceneController.Instance.LoadScene("Credits");
    }
}
