using UnityEngine;
using UnityEngine.UI;

public class Slider : MonoBehaviour
{
    private RectTransform hover;

    public float slideSpeed = 600;

    private Vector2 touchStartPos;
    private Vector2 targetPos;
    private float paneltHeight;
    private float hoverHeight;

    private RectTransform contentPanel;

    [Tooltip("How many pixel you have to swipe to trigger event")]
    public float operatingThreshold;

    void Start()
    {
        hover = this.transform as RectTransform;
        hoverHeight = hover.sizeDelta.y;

        contentPanel = GetComponentInChildren<ScrollRect>().transform as RectTransform;

        paneltHeight = contentPanel.sizeDelta.y + hoverHeight;
        targetPos = new Vector2(0, -hoverHeight);//hide menu
        ShowHover();
    }

    void Update()
    {

        //if (Input.GetMouseButtonDown(0)) { ShowContentPanel(); }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began: touchStartPos = touch.position; break;

                case TouchPhase.Moved:

                    //swipe horizontal?
                    if (RectTransformUtility.RectangleContainsScreenPoint(this.transform as RectTransform, Input.mousePosition))
                    {
                        Debug.Log("slider 43 " + (touch.position.y - touchStartPos.y));
                        if (touch.position.y - touchStartPos.y > operatingThreshold)
                        {
                            targetPos = new Vector2(0, paneltHeight - hoverHeight);//show menu
                        }

                        if (touch.position.y - touchStartPos.y < -operatingThreshold)
                        {
                            targetPos = new Vector2(0, 0);//hide menu
                        }
                    }
                    break;
            }
        }

        hover.anchoredPosition = Vector2.MoveTowards(hover.anchoredPosition, targetPos, slideSpeed * Time.deltaTime);

    }

    public void ShowHover()
    {
        targetPos = new Vector2(0, 0);
    }

    public void ShowContentPanel()
    {
        targetPos = new Vector2(0, paneltHeight - hoverHeight);
    }

}
