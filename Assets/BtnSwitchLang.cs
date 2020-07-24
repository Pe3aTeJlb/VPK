using UnityEngine;

public class BtnSwitchLang : MonoBehaviour
{
    [SerializeField]
    private LocalizationManager localizationManager;

    public string lang;
    public void OnButtonClick()
    {
        Debug.LogError(Application.persistentDataPath);
        localizationManager.LoadLocalizedText(lang);
    }
    
}