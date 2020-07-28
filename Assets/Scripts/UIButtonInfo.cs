using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool isDown { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
    }

}
