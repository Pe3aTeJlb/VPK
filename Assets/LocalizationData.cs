using System;
using UnityEngine;

[Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;
}

[Serializable]
public class LocalizationItem 
{
    public string key;
    public string value;
}