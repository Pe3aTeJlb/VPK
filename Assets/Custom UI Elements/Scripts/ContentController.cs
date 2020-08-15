using System;
using System.Collections.Generic;
using UnityEngine;
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

    private int numberToCreate; // number of objects to create. Exposed in inspector
    private GridLayoutGroup GLP;

    //Snap Scrolling
    private List<RectTransform> icons = new List<RectTransform>();
    private Vector3[] iconsScale;

    private RectTransform contentRect;
    private int selectedIconID;
    private bool isScrolling;

    private Vector2 contentVector;
    public float snapSpeed;
    public float iconOffset;

    public float scaleOffset;
    public float scaleSpeed;

    private ScrollRect scrollRect;


    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
        contentRect = GetComponent<RectTransform>();

        numberToCreate = models.Count;
        GLP = GetComponent<GridLayoutGroup>();
        GLP.constraintCount = numberToCreate;

        iconsScale = new Vector3[models.Count];

        PopulateGrid();
    }

    void PopulateGrid()
    {
        GameObject newObj;

        for (int i = 0; i < numberToCreate; i++)
        {
            int j = i; // замыкание
            // Create new instances of our prefab until we've created as many as specified
            newObj = Instantiate(iconPrefab, transform);
            newObj.GetComponent<Image>().sprite = models[i].icon;
            newObj.GetComponent<ItemDragHandler>().prefab = models[j].modelPrefab;
            
            icons.Add(newObj.transform as RectTransform);
        }
        
    }

    public void Update()
    {
        if (contentRect.anchoredPosition.x >= icons[0].transform.localPosition.x && !isScrolling ||
            contentRect.anchoredPosition.x <= icons[icons.Count - 1].transform.localPosition.x && !isScrolling) 
        {

            isScrolling = false;
            scrollRect.inertia = false;
        }

        //float nearestPos = float.MaxValue;

        for (int i = 0; i < icons.Count; i++) 
        {
            float distanse = Mathf.Abs(contentRect.anchoredPosition.x - icons[i].transform.localPosition.x);
           
            if (distanse < 300) 
            {
               // nearestPos = distanse;
                selectedIconID = i;
            }

            float scale = Mathf.Clamp(1/(distanse/ GLP.spacing.x) *scaleOffset, 0.5f, 1f);
            iconsScale[icons.Count - 1 - i].x = Mathf.SmoothStep(icons[icons.Count - 1 - i].transform.localScale.x, scale, scaleSpeed * Time.deltaTime);
            iconsScale[icons.Count - 1 - i].y = Mathf.SmoothStep(icons[icons.Count - 1 - i].transform.localScale.y, scale, scaleSpeed * Time.deltaTime);
            iconsScale[icons.Count - 1 - i].z = 1;
            icons[icons.Count - 1 - i].transform.localScale = iconsScale[icons.Count - 1 - i];

        }

        float scrollVelocity = Mathf.Abs(scrollRect.velocity.x);

        if (scrollVelocity < 400 && !isScrolling) 
        {
            scrollRect.inertia = false;
        }

        if (!isScrolling && scrollVelocity < 400)
        {
            contentVector.x = Mathf.SmoothStep(contentRect.anchoredPosition.x, icons[selectedIconID].transform.localPosition.x, snapSpeed * Time.deltaTime);
            contentRect.anchoredPosition = contentVector;
            icons[icons.Count - 1 - selectedIconID].gameObject.GetComponent<ItemDragHandler>().enabled = true;
        }
        else 
        {
            icons[icons.Count - 1 - selectedIconID].gameObject.GetComponent<ItemDragHandler>().enabled = false;
        }
    }


    public void Scrolling(bool scroll)
    {
        isScrolling = scroll;
        if (scroll) scrollRect.inertia = true;
    }


}