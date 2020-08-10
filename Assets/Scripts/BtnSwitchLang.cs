using UnityEngine;

public class BtnSwitchLang : MonoBehaviour
{
    private LocalizationManager localizationManager;

    public string lang;

    public void Start()
    {
        localizationManager = GameObject.FindGameObjectWithTag("LocalizationManager").GetComponent<LocalizationManager>();
    }

    public void OnButtonClick()
    {
        localizationManager.ChangeLanguage(lang);
    }
    
}