using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slider : MonoBehaviour
{
    public float slideSpeed = 600;
    public Camera cam;

    private Vector2 touchStartPos;
    private Vector2 targetPos;
    public int slideWidth;
    public RectTransform contentPanel, handle;
    private bool b = false;

    void Start()
    {
        targetPos = new Vector2(0, slideWidth);//hide menu
        contentPanel = transform as RectTransform;
    }

    void Update()
    {

        if (Input.touchCount > 0)
        {

            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began: touchStartPos = touch.position; break;

                case TouchPhase.Moved:
                    
                    //swipe horizontal?
                    if (RectTransformUtility.RectangleContainsScreenPoint(contentPanel, Input.mousePosition, cam))
                    {

                        if (touch.position.y - touchStartPos.y > 20)
                        {
                            Debug.Log("(((((");
                            targetPos = new Vector2(0, 0);//show menu
                        }
                    
                        if (touch.position.y - touchStartPos.y < -20)
                        {
                            Debug.Log("&");
                            targetPos = new Vector2(0, slideWidth);//hide menu
                        }
                    }
                    break;
            }
        }

        contentPanel.anchoredPosition = Vector2.MoveTowards(contentPanel.anchoredPosition, targetPos, slideSpeed * Time.deltaTime);

    }
}
