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
        var healthBars = HealthBars.Instance;
        
        if (healthBars.CurrentFamilyState() == HealthBars.FamilyState.Broken)
        {
            SceneController.Instance.LoadScene("DevorceEnding");
        }
        else
        {
            SceneController.Instance.LoadScene("ChosePatient");
        }
    }
}
