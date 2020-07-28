using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    private SceneController sceneController;

    private RectTransform contentPanel;
    private Vector3 originPos;
    private Vector2 touchStartPos, currTouchPos;

    public GameObject prefab;

    private bool isPointerOver;

    private Image thisImage;

    public void Start()
    {
        sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        contentPanel = GameObject.FindGameObjectWithTag("ContentHolder").transform as RectTransform;
        thisImage = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // хотя сам метод и предполагает наличие нажатий, данная проверка предотвращает ошибки редактора
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            touchStartPos = touch.position;
        }
        else if (Input.GetMouseButtonDown(0)) 
        {
            touchStartPos = Input.mousePosition;
        }

        originPos = transform.localPosition;
        sceneController.SetModelByIndex(prefab);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            currTouchPos = touch.position;
        }
        else if (Input.GetMouseButton(0))
        {
            currTouchPos = Input.mousePosition;
        }

        if (currTouchPos.y - touchStartPos.y > 50)
        {
            //thisImage.raycastTarget = true;
            transform.position = Input.mousePosition;
        }
        else {
            //thisImage.raycastTarget = false;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(contentPanel, Input.mousePosition))
        {
            sceneController.InstantiateModel();
            isPointerOver = false;
        }
        else 
        {
            sceneController.HideModel();
            isPointerOver = true;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localPosition = originPos;

        if (isPointerOver) {
            sceneController.HideModel();
            sceneController.DeleteModelOnContentEnter();
        }
        else
        {
            sceneController.SetAnchor();
        }
        
    }

}
