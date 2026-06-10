using UnityEngine;
using UnityEngine.UI;

public class BedButon : MonoBehaviour
{
    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
        
        _button.onClick.AddListener(HandleButtonClick);
    }
    private void HandleButtonClick()
    {
        if (HealthBars.Instance.CurrentFamilyState() == HealthBars.FamilyState.Broken)
        {
            SceneController.Instance.LoadScene("DevorceEnding");
            return;
        }

        SceneController.Instance.LoadScene("ChosePatient");
    }
}
