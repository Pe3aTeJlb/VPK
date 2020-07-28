using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LocalizationManager : MonoBehaviour
{
    public LocalizationManager instance;

    private string currentLanguage;
    private Dictionary<string, string> localizedText;
    public static bool isReady = false;

    public delegate void ChangeLangText();
    public event ChangeLangText OnLanguageChanged;

    void Awake()
    {

        DontDestroyOnLoad(gameObject);

        if (!PlayerPrefs.HasKey("Language"))
        {
            if (Application.systemLanguage == SystemLanguage.Russian || 
                Application.systemLanguage == SystemLanguage.Ukrainian || 
                Application.systemLanguage == SystemLanguage.Belarusian)
            {
                PlayerPrefs.SetString("Language", "ru_RU");
            }
            else
            {
                PlayerPrefs.SetString("Language", "en_EN");
            }
        }

        currentLanguage = PlayerPrefs.GetString("Language");

        if (Application.platform == RuntimePlatform.Android)
        {
            StartCoroutine(LoadLocalizedTextOnAndroid(currentLanguage));
        }
        else
        {
            LoadLocalizedText(currentLanguage);
        }

    }

    public void ChangeLanguage(string newLang) {

        if (!newLang.Equals(currentLanguage))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                localizedText.Clear();
                StartCoroutine(LoadLocalizedTextOnAndroid(newLang));
            }
            else
            {
                LoadLocalizedText(newLang);
            }
        }

    }

    public void LoadLocalizedText(string langName)
    {
        string fileName = langName + ".json";
        //string path = Application.streamingAssetsPath +"/"+ langName + ".json";
        string path = Path.Combine(Application.streamingAssetsPath + "/", fileName);

        string dataAsJson;

        dataAsJson = File.ReadAllText(path);
        LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);
        
        localizedText = new Dictionary<string, string>();

        for (int i = 0; i < loadedData.items.Length; i++)
        {
            localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            //Debug.LogWarning("" +loadedData.items[i].key + "  " + loadedData.items[i].value);
        }

        PlayerPrefs.SetString("Language", langName);
        currentLanguage = PlayerPrefs.GetString("Language");
        isReady = true;

        OnLanguageChanged?.Invoke();
    }

    IEnumerator LoadLocalizedTextOnAndroid(string langName)
    {
        string fileName = langName + ".json";

        localizedText = new Dictionary<string, string>();

        string filePath;
        filePath = Path.Combine(Application.streamingAssetsPath + "/", fileName);
        
        string dataAsJson;

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {

            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            dataAsJson = www.downloadHandler.text;
            
        }
        else
        {
            dataAsJson = File.ReadAllText(filePath);
        }
        LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

        Debug.LogWarning(loadedData.items.Length);
        int n = loadedData.items.Length;

        for (int i = 0; i < n; i++)
        {
            if (!localizedText.ContainsKey(loadedData.items[i].key))
            {
                localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }
            Debug.Log("KEYS:" + loadedData.items[i].key);
        }

        PlayerPrefs.SetString("Language", langName);
        currentLanguage = PlayerPrefs.GetString("Language");
        isReady = true;

        OnLanguageChanged?.Invoke();
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText.ContainsKey(key))
        {
            string result = localizedText[key];
            return result;
        }
        else
        {
            throw new Exception("Localized text with key \"" + key + "\" not found");
        }
    }

    public string CurrentLanguage
    {
        get
        {
            return currentLanguage;
        }
        set
        {
            PlayerPrefs.SetString("Language", value);
            currentLanguage = PlayerPrefs.GetString("Language");
        }
    }
    
    public bool IsReady
    {
        get
        {
            return isReady;
        }
    }

}