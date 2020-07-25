using UnityEngine;

public class BtnSwitchLang : MonoBehaviour
{
    [SerializeField]
    private LocalizationManager localizationManager;

    public string lang;
    public void OnButtonClick()
    {
        localizationManager.ChangeLanguage(lang);
    }
    
}