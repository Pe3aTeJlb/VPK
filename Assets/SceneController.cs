using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleARCore.Examples.Common;

public class SceneController : MonoBehaviour
{
    public Camera firstPersonCamera;
    public GameObject cam;

    private Anchor lastAnchor;

    private TrackableHit lastHit;

    private GameObject Model;
    private GameObject Car;
    public GameObject terrain;
    private bool alreadyInstantiated = false;
    private bool tracking;

    public DepthMenu DepthMenu;
    private int currModelsCount = 0;
    private readonly int maxModelsCount = 1; // set max amount of models in the scene

    public GameObject carControl;
    public GameObject contentPanel;
    public GameObject gameButton;

    public DetectedPlaneGenerator planeGenerator;

    public Text fps;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QuitOnConnectionErrors();
        Car = null;

        contentPanel.SetActive(false);
        gameButton.SetActive(false);
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
            Car = Instantiate(Model, Vector3.zero, Quaternion.identity);
            
            Car.GetComponent<Rigidbody>().useGravity = false;

            //Face model to the camera
            Vector3 targetDirection = cam.transform.position - Car.transform.position;
            float step = 10000 * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(Car.transform.forward, targetDirection, step, 0.0F);
            Car.transform.rotation = Quaternion.LookRotation(newDirection);
            Car.transform.rotation = Quaternion.Euler(0, Car.transform.rotation.y, 0);

            Car.GetComponent<SimpleCarController>().enabled = false;
            Car.GetComponent<SteeringWheel>().enabled = false;
            
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

            alreadyInstantiated = false;
            
            Car = new GameObject();

            gameButton.SetActive(true);

            currModelsCount += 1;

            planeGenerator.hideNewPlanes = true;
            planeGenerator.HideAllPlanes();
            planeGenerator.enabled = false;
        }
    }

    public void EnableContentPanel() 
    {
        contentPanel.SetActive(true);
    }

    //Prepare game mode 
    public void Game() {
        

        if (currModelsCount == 1)
        {
            Screen.orientation = ScreenOrientation.Landscape;

            carControl.SetActive(true);

            var gO = Instantiate(terrain, Car.transform.position, Quaternion.identity);
            gO.transform.parent = Car.transform.parent;

            Car.GetComponent<Rigidbody>().useGravity = true;
            Car.GetComponent<SimpleCarController>().enabled = true;
            Car.GetComponent<SteeringWheel>().enabled = true;
            Car.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    public void HideModel()
    {
        if (Car != null) Car.SetActive(false);
    }

    public void DeleteModel() {
        if (Car != null) {
            Destroy(Car);
            Debug.Log("Deleted");
            alreadyInstantiated = false;
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
