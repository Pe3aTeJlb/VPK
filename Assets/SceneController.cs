using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleARCore.Examples.Common;
using System.Collections.Generic;

public class SceneController : MonoBehaviour
{
    public Camera firstPersonCamera;
    public GameObject cam;
    public PhysicsRaycaster rayCaster;

    private Anchor lastAnchor;
    private TrackableHit lastHit;

    private GameObject Model;
    private GameObject Car;
    private GameObject gameCar;
    private bool alreadyInstantiated = false;
    private bool tracking;

    public GameObject terrainPrefab;
    private GameObject terrain;

    private int currModelsCount = 0;
    private readonly int maxModelsCount = 10; // set max amount of models in the scene

    public DepthMenu DepthMenu;

    public Canvas carControl;
    public GameObject contentPanel;

    public DetectedPlaneGenerator planeGenerator;

    public bool kostil;
    public Text fps;

    private List<GameObject> models = new List<GameObject>();

    public GameObject backToExposition;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        Screen.orientation = ScreenOrientation.Landscape;
        QuitOnConnectionErrors();
        Car = null;

        contentPanel.SetActive(false);
        backToExposition.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        fps.text = "" + 1 / Time.deltaTime;

        if (DepthMenu != null && !DepthMenu.CanPlaceAsset())
        {
            return;
        }

        // The session status must be Tracking in order to access the Frame.
        if (Session.Status != SessionStatus.Tracking)
        {
            int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            return;
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        ProcessTouches();


        //я не знаю как лучше развернуть моедль в направлении камеры
        if (kostil) 
        {
            Car.transform.LookAt(new Vector3(cam.transform.position.x, Car.transform.position.y, cam.transform.position.z));
            kostil = false;

        }

    }
   
    public void SetModelByIndex(GameObject newModel)
    {
        Model = newModel;
    }

    public void InstantiateModel()
    {

        if (!alreadyInstantiated && currModelsCount < maxModelsCount)
        {
            tracking = true;
            alreadyInstantiated = true;

            rayCaster.enabled = false;

            Car = Instantiate(Model, Vector3.zero, Quaternion.identity);

            kostil = true;
            
            Car.SetActive(false);

            planeGenerator.enabled = true;
            planeGenerator.hideNewPlanes = false;
            planeGenerator.ShowAllPlanes();
        }
    }

    void ProcessTouches()
    {
        Touch touch;

        //if (Input.touchCount != 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        if (Input.touchCount != 1)
        {
            return;
        }
        else 
        {
            touch = Input.GetTouch(0);
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds |
            TrackableHitFlags.PlaneWithinPolygon;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(firstPersonCamera.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                lastHit = hit;

                if (DepthMenu != null)
                {
                    // Show depth card window if necessary.
                    DepthMenu.ConfigureDepthBeforePlacingFirstAsset();
                }

                if (hit.Trackable is DetectedPlane)
                {

                    DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;

                    if (detectedPlane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing &&
                        currModelsCount <= maxModelsCount &&
                        tracking && Car.gameObject != null
                        )
                    {
                        Car.transform.position = hit.Pose.position;
                        Car.SetActive(true);
                    }

                }
            }
        }
        else {
            HideModel();
        }
    }

    //called from ItemDragHandler OnEndDrag cause processtouches runs from update which will make problems
    public void SetAnchor()
    {
        if (tracking)
        {
            tracking = false;

            lastAnchor = lastHit.Trackable.CreateAnchor(lastHit.Pose);

            Car.transform.parent = lastAnchor.transform;
            Car.SetActive(true);

            //Car.GetComponentInChildren<FloatMenuController>().enabled = true;

            alreadyInstantiated = false;

            models.Add(Car);
            
            Car = new GameObject();

            currModelsCount += 1;

            rayCaster.enabled = true;

            planeGenerator.hideNewPlanes = true;
            planeGenerator.HideAllPlanes();
            planeGenerator.enabled = false;
        }
    }

    public void EnableContentPanel() 
    {
        contentPanel.SetActive(true);
    }

    public void Exposition() 
    {
        backToExposition.SetActive(false);

        Destroy(gameCar);
        Destroy(terrain);

        foreach (GameObject model in models)
        {
            model.SetActive(true);
        }

        contentPanel.SetActive(true);
        carControl.enabled = false;

    }

    //Prepare game mode 
    public void TestDrive(GameObject gameModel) {


        backToExposition.SetActive(true);
        contentPanel.SetActive(false);

        Debug.LogError(gameModel);
        Model = gameModel;

        foreach (GameObject model in models) 
        {
            model.SetActive(false);
        }

        carControl.enabled = true;
      
        gameCar = Instantiate(gameModel, lastAnchor.transform.position, Quaternion.identity);
        gameCar.transform.parent = lastAnchor.transform;
        gameCar.transform.Rotate(Vector3.up, 180);

        terrain = Instantiate(terrainPrefab, lastAnchor.transform.position, Quaternion.identity);
        terrain.transform.parent = lastAnchor.transform;

    }

    public void HideModel()
    {
        if (Car != null) Car.SetActive(false);
    }

    public void DeleteModelOnContentEnter() {
        if (Car != null) {
            Destroy(Car);
            Debug.Log("Deleted");
            alreadyInstantiated = false;
            currModelsCount--;
        }
    }

    public void DeleteModel(GameObject Car) 
    {

        if (Car != null)
        {
            models.Remove(Car);
            Destroy(Car);
            Debug.Log("Deleted");
            currModelsCount--;
        }

    }

    void QuitOnConnectionErrors()
    {
        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            //ShowAndroidToastMessage("Camera permission is needed to run this application.");
            ShowAndroidToastMessage("Необходимо разрешение доступ к камере");
            Invoke("Quit", 0.5f);
        }
        else if (Session.Status == SessionStatus.FatalError)
        {
            //ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            ShowAndroidToastMessage("ARCore столкнулся с проблемой подключения. Пожалуйста, перезапустите приложение.");
            Invoke("Quit", 0.5f);
        }
    }

    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    private void Quit()
    {
        Application.Quit();
    }

}
