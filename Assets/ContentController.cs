using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class UIModelImplemention
{
    public Sprite icon;
    public GameObject modelPrefab;
}

public class ContentController : MonoBehaviour
{

    public List<UIModelImplemention> models;

    public GameObject iconPrefab; // This is our prefab object that will be exposed in the inspector

    public SceneController sceneController;

    private int numberToCreate; // number of objects to create. Exposed in inspector
    private GridLayoutGroup GLP;

    // Start is called before the first frame update
    void Start()
    {
        GLP = GetComponent<GridLayoutGroup>();
        numberToCreate = models.Count;
        GLP.constraintCount = numberToCreate;
        PopulateGrid();

    }

    void PopulateGrid()
    {
        GameObject newObj;

        for (int i = 0; i < numberToCreate; i++)
        {
            int j = i; // замыкание
            // Create new instances of our prefab until we've created as many as specified
            newObj = (GameObject)Instantiate(iconPrefab, transform);
            newObj.GetComponent<Image>().sprite = models[i].icon;
            newObj.GetComponent<ItemDragHandler>().prefab = models[j].modelPrefab;
         
        }

    }

}