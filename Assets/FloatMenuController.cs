using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Annotation
{
    public GameObject uiElement;
    public GameObject targetPoint;
}

public class FloatMenuController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public List<Annotation> annotations;
    
    private GameObject root; // the root
    public Transform Model; // the mesh

    private Camera cam;

    public Canvas menu;
    public LocalizedText annotationButton; //annotation enable button/ needs to change text 

    private bool annotationIsOpen;
    private LineRenderer buffLine;
    private Vector3[] buff = new Vector3[4];
    public float lineWidth;

    //LongTap
    private bool pointerOverModel;
    private float startTime, currTime;
    public float boundaryValue = 0.85f;
    private bool longTapOver;

    private bool transformModel = false;

    //transform model
    private float scale = 1;
    public float scaleStep = 0.05f;
    public float minScale;

    public float screwSense;
    private float rotation;

    private SceneController sceneController;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();

        cam = Camera.main;

        menu.enabled = false;

        root = transform.parent.gameObject;

        foreach (Annotation an in annotations)
        {
            an.uiElement.SetActive(false);
            an.targetPoint.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int taps = eventData.clickCount;
        Debug.Log(taps);
        if (taps == 2)
        {
            Debug.Log("Double click on model");
            transformModel = !transformModel;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerOverModel = true;   
    }

    private void Update()
    {

        if (menu.isActiveAndEnabled)
        {
            menu.transform.LookAt(cam.transform);
            menu.transform.Rotate(Vector3.up, 180);
        }

        if (annotationIsOpen) 
        {
            foreach (Annotation an in annotations)
            {
                //force ui look at camera
                    an.uiElement.transform.LookAt(cam.transform);
                    an.uiElement.transform.Rotate(Vector3.up, 180);

                    an.targetPoint.transform.LookAt(cam.transform);

                    buffLine = an.uiElement.GetComponent<LineRenderer>();

                    RectTransform ui = an.uiElement.transform as RectTransform;

                    ui.GetWorldCorners(buff);

                    buffLine.SetPositions(new Vector3[] { buff[3], an.targetPoint.transform.position });
            }

        }

        if (!transformModel && pointerOverModel)
        {
            if (Input.GetMouseButtonDown(0))
            {
                longTapOver = false;
                startTime = Time.time;
            }

            if (Input.GetMouseButton(0))
            {
                currTime = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                //longTapOver = false;
            }

            if (currTime - startTime > boundaryValue && !longTapOver)
            {
                Debug.Log("Loong tap");
                longTapOver = true;
                startTime = 0;
                currTime = 0;
                pointerOverModel = false;
                OpenFloatMenu();

            }
        }

        if (transformModel)
        {

            if (Application.platform == RuntimePlatform.Android)
            {

                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevTouchDelMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float TouchDelMag = (touchZero.position - touchOne.position).magnitude;

                    float deltaMagnitudeDiff = prevTouchDelMag - TouchDelMag;

                    scale += deltaMagnitudeDiff * scaleStep;

                    scale = Mathf.Clamp(scale, minScale, 1);
                }

                if (Input.touchCount == 1)
                {

                    rotation = transform.localEulerAngles.y + Input.GetTouch(0).deltaPosition.x * screwSense;
                    transform.localEulerAngles = new Vector3(0, rotation, 0);

                }

            }
            else
            {

                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    scale += scaleStep;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    scale -= scaleStep;
                }

                scale = Mathf.Clamp(scale, minScale, 1);

                Model.localScale = new Vector3(scale, scale, scale);

                if (Input.GetKey(KeyCode.Mouse0))
                {
                    rotation = Model.localEulerAngles.y + Input.GetAxis("Mouse X") * -screwSense;
                    Model.localEulerAngles = new Vector3(0, rotation, 0);
                }

            }

        }

    }

    void OpenFloatMenu()
    {
        menu.enabled = true;
    }

    public void CloseFloatMenu()
    {
        menu.enabled = false;
    }

    public void DrawAnnotation()
    {
        if (!annotationIsOpen)
        {
            annotationButton.key = "RemoveAnotation";

            annotationIsOpen = true;

            CloseFloatMenu();

            foreach (Annotation an in annotations)
            {

                an.uiElement.SetActive(true);
                an.targetPoint.SetActive(true);

                buffLine = an.uiElement.GetComponent<LineRenderer>();

                RectTransform ui = an.uiElement.transform as RectTransform;

                ui.GetWorldCorners(buff);

                buffLine.SetPositions(new Vector3[] { buff[3], an.targetPoint.transform.position});

                buffLine.startWidth = lineWidth;
                buffLine.endWidth = lineWidth;

            }
        }
        else
        {
            annotationButton.key = "DrawAnotation";

            annotationIsOpen = false;

            foreach (Annotation an in annotations)
            {
                an.uiElement.SetActive(false);
                an.targetPoint.SetActive(false);

            }

        }
    }

    public void TestDrive() 
    { 
    
    }

    public void RemoveModel()
    {
        sceneController.DeleteModel(root);
    }

}
