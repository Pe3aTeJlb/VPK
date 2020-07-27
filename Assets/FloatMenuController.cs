using System.Collections;
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

    private Transform Model;

    public Canvas menu;
    public LocalizedText annotationButton;
    private bool annotationIsOpen;
    private Vector3 originPos;


    private Camera cam;

    private bool pointerOverModel;
    private float startTime, currTime;
    public float boundaryValue = 0.85f;
    private bool longTapOver;

    private bool transformModel = false;

    private float scale = 1;
    public float scaleStep = 0.05f;

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

        Model = transform.parent.transform;

        foreach (Annotation an in annotations)
        {
            an.uiElement.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
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
            //menu.transform.rotation = Quaternion.Euler(menu.transform.rotation.x, -menu.transform.rotation.y, menu.transform.rotation.z);
        }

        if (annotationIsOpen) 
        {

            foreach (Annotation an in annotations)
            {
                originPos = an.targetPoint.transform.position;
                an.uiElement.transform.LookAt(cam.transform);
                an.uiElement.transform.Rotate(Vector3.up, 180);
                an.targetPoint.transform.position = originPos;

            }

        }

        if (!transformModel && pointerOverModel)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startTime = Time.time;
            }

            if (Input.GetMouseButton(0))
            {
                currTime = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                longTapOver = false;
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

#if UNITY_EDITOR

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                scale += scaleStep;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                scale -= scaleStep;
            }

            scale = Mathf.Clamp(scale, 0.1f, 1);

            Model.localScale = new Vector3(scale, scale, scale);

            if (Input.GetKey(KeyCode.Mouse0))
            {
                rotation = Model.localEulerAngles.y + Input.GetAxis("Mouse X") * -screwSense;
                Model.localEulerAngles = new Vector3(0, rotation, 0);
            }
#endif

            /// управление для сенсора
#if !UNITY_EDITOR && UNITY_ANDROID
/*
        if (Input.touchCount == 2) {
				Touch touchZero = Input.GetTouch (0);
				Touch touchOne = Input.GetTouch (1);

				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				float prevTouchDelMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float TouchDelMag = (touchZero.position - touchOne.position).magnitude;

				float deltaMagnitudeDiff = prevTouchDelMag - TouchDelMag;

				offset.z -= deltaMagnitudeDiff * sensor_zoomSpeed;
				offset.z = Mathf.Max (offset.z, -zoomMax);
                offset.z = Mathf.Clamp(offset.z, -Mathf.Abs(zoomMax), -Mathf.Abs(zoomMin));
        }
				
			if ((Input.touchCount == 1) && expo_mod_on == false && stop_rotating_i_touched == false) {
			
				X = transform.localEulerAngles.y + Input.GetTouch (0).deltaPosition.x * sensor_sensitivity;
				Y += Input.GetTouch (0).deltaPosition.y * sensor_sensitivity;
				Y = Mathf.Clamp (Y, -limit, limit);
				transform.localEulerAngles = new Vector3 (-Y, X, 0);

			}
            */
#endif

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

                LineRenderer line = an.uiElement.GetComponent<LineRenderer>();

                RectTransform ui = an.uiElement.transform as RectTransform;

                Vector3 bottomRightCorner = new Vector3(ui.offsetMax.x, ui.offsetMin.y, 0);

                line.SetPositions(new Vector3[] { bottomRightCorner, an.targetPoint.transform.localPosition });

                line.startWidth = 0.01f;
                line.endWidth = 0.01f;

            }
        }
        else
        {
            annotationButton.key = "DrawAnotation";

            annotationIsOpen = false;

            foreach (Annotation an in annotations)
            {
                an.uiElement.SetActive(false);

                LineRenderer line = an.uiElement.GetComponent<LineRenderer>();

                RectTransform ui = an.uiElement.transform as RectTransform;

                Vector3 bottomRightCorner = new Vector3(ui.offsetMax.x, ui.offsetMin.y, 0);

                line.SetPositions(new Vector3[] { bottomRightCorner, an.targetPoint.transform.localPosition });

                line.startWidth = 0.01f;
                line.endWidth = 0.01f;

            }

        }
    }



    public void RemoveModel()
    {
        //sceneController
    }

}
