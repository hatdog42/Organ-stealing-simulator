using System;
using System.Collections;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(10f);
        SceneController.Instance.LoadScene("MainMenue");
    }
}
