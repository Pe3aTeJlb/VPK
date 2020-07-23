using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private SceneController sceneController;
    private RectTransform contentPanel;
    private Vector3 originPos;
    public GameObject prefab;

    public void Start()
    {
        sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        contentPanel = GameObject.FindGameObjectWithTag("ContentHolder").transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originPos = transform.localPosition;
        sceneController.SetModelByIndex(prefab);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        if (!RectTransformUtility.RectangleContainsScreenPoint(contentPanel, Input.mousePosition))
        {
            sceneController.InstantiateModel();
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localPosition = originPos;
        sceneController.SetAnchor();
    }

}
