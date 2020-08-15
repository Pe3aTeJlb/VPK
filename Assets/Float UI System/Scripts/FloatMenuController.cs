using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class Annotation
{
    public GameObject uiElement;
    public GameObject targetPoint;
    [Range(0,3)]
    [Description("ClockWise. 0 - lower-left corner")]
    public int AncorCornernIndex;
}

public class FloatMenuController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public List<Annotation> annotations;

    [Space(10)]
    private GameObject root; // the root
    public Transform Model; // the mesh
    public GameObject gameMoldel;
    public GameObject terrain;

    private Camera cam;

    [Space(10)]
    public Canvas menu;
    public Text annotationButton; //annotation enable button/ needs to change text
    private LocalizationManager localizationManager;

    [Space(10)]
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

    private float firstClickTime; //double click
    public float timeBetweenClicks = 0.5f;
    private int clickCounter;

    private int currAnnotationIndex = 0;
    private int prevAnnotationIndex = 0;
    public GameObject annotationButtons;

    // Start is called before the first frame update
    void Start()
    {

        Application.targetFrameRate = 60;

        sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        localizationManager = GameObject.FindGameObjectWithTag("LocalizationManager").GetComponent<LocalizationManager>();

        cam = Camera.main;

        menu.enabled = false;

        root = transform.parent.gameObject;

        foreach (Annotation an in annotations)
        {
            an.uiElement.SetActive(false);
            an.targetPoint.SetActive(false);
        }
        annotationButtons.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickCounter++;
        firstClickTime = Time.time;
        StartCoroutine(DoubleClickDetection());
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

                    buffLine.SetPositions(new Vector3[] { buff[an.AncorCornernIndex], an.targetPoint.transform.position });
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

                    Model.localScale = new Vector3(scale, scale, scale);
                }

                if (Input.touchCount == 1)
                {
                   
                    rotation += transform.localEulerAngles.y + Input.GetTouch(0).deltaPosition.x * -screwSense / 20;
                    Model.transform.eulerAngles = new Vector3(0, rotation, 0);

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
                    Model.transform.localEulerAngles = new Vector3(0, rotation, 0);
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
            annotationButton.text = localizationManager.GetLocalizedValue("RemoveAnotation");

            annotationIsOpen = true;

            CloseFloatMenu();

            if (annotations.Count > 1)annotationButtons.SetActive(true);

             foreach (Annotation an in annotations)
             {

                an.uiElement.SetActive(true);
                an.targetPoint.SetActive(true);

                buffLine = an.uiElement.GetComponent<LineRenderer>();

                RectTransform ui = an.uiElement.transform as RectTransform;

                ui.GetWorldCorners(buff);

                buffLine.SetPositions(new Vector3[] { buff[an.AncorCornernIndex], an.targetPoint.transform.position});

                buffLine.startWidth = lineWidth;
                buffLine.endWidth = lineWidth;

                an.uiElement.SetActive(false);
                an.targetPoint.SetActive(false);

             }

            annotations[currAnnotationIndex].uiElement.SetActive(true);
            annotations[currAnnotationIndex].targetPoint.SetActive(true);

        }
        else
        {
            annotationButton.text = localizationManager.GetLocalizedValue("DrawAnotation");

            annotationIsOpen = false;
            annotationButtons.SetActive(false);

            currAnnotationIndex = 0;

            foreach (Annotation an in annotations)
            {
                an.uiElement.SetActive(false);
                an.targetPoint.SetActive(false);

            }

        }
    }

    public void NextAnnotation() 
    {
        prevAnnotationIndex = currAnnotationIndex;
        currAnnotationIndex++;

        if (currAnnotationIndex > annotations.Count - 1)
        {
            currAnnotationIndex = 0;    
        }

        annotations[currAnnotationIndex].uiElement.SetActive(true);
        annotations[currAnnotationIndex].targetPoint.SetActive(true);

        annotations[prevAnnotationIndex].uiElement.SetActive(false);
        annotations[prevAnnotationIndex].targetPoint.SetActive(false);
        
    }

    public void PrevAnnotation() 
    {
        prevAnnotationIndex = currAnnotationIndex;
        currAnnotationIndex--;

        if (currAnnotationIndex < 0) {
            currAnnotationIndex = annotations.Count - 1;
        }

        annotations[currAnnotationIndex].uiElement.SetActive(true);
        annotations[currAnnotationIndex].targetPoint.SetActive(true);

        annotations[prevAnnotationIndex].uiElement.SetActive(false);
        annotations[prevAnnotationIndex].targetPoint.SetActive(false);
        

    }

    public void TestDrive() 
    {
        menu.enabled = false;

        if (sceneController == null) 
        {
            Debug.Log("Somehow its null");
            sceneController = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SceneController>();
        }

        sceneController.TestDrive(gameMoldel, terrain);
    }

    public void RemoveModel()
    {
        sceneController.DeleteModel(root);
    }

    private IEnumerator DoubleClickDetection()
    {
        while (Time.time < firstClickTime + timeBetweenClicks)
        {
            if (clickCounter == 2)
            {
                Debug.Log("Double click on model");
                transformModel = !transformModel;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        clickCounter = 0;
        firstClickTime = 0f;
    }

}
