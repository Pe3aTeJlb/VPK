using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleARCore.Examples.Common;

public class SceneController : MonoBehaviour
{
    public Camera firstPersonCamera;
    private Anchor anchor;
    private DetectedPlane detectedPlane;

    public GameObject Strela;
    public GameObject Car;
    public GameObject terrain;

    public DepthMenu DepthMenu;
    private int currModelsCount = 0;
    private int totelModelsCount = 1;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QuitOnConnectionErrors();
    }

    // Update is called once per frame
    void Update()
    {

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

    void ProcessTouches()
    {
        Touch touch;
        if (Input.touchCount != 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
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
                
                if (DepthMenu != null)
                {
                    // Show depth card window if necessary.
                    DepthMenu.ConfigureDepthBeforePlacingFirstAsset();
                }
                

                // Choose the prefab based on the Trackable that got hit.
                GameObject prefab = null; ;

                if (hit.Trackable is DetectedPlane)
                {

                    DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                    if (detectedPlane.PlaneType == DetectedPlaneType.Vertical){}
                    else
                    {
                        prefab = Strela;
                    }

                }
                else {}

                if (currModelsCount < totelModelsCount)
                {
                    
                    // Instantiate prefab at the hit pose.
                    Car = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    Car.GetComponent<Rigidbody>().useGravity = false;
                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    Car.transform.Rotate(0, 180, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    // Make game object a child of the anchor.
                    Car.transform.parent = anchor.transform;
                    Car.GetComponent<SimpleCarController>().enabled = false;

                    currModelsCount++;
                }
            }
        }
    }

    public void Game() {

        if (currModelsCount == 1)
        {
            var gO = Instantiate(terrain, Car.transform.position, Quaternion.identity);
            gO.transform.parent = Car.transform.parent;

            Car.GetComponent<Rigidbody>().useGravity = true;
            Car.GetComponent<SimpleCarController>().enabled = true;
            Car.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    void QuitOnConnectionErrors()
    {
        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("Camera permission is needed to run this application.");
            Invoke("Quit", 0.5f);
        }
        else if (Session.Status == SessionStatus.FatalError)
        {
            ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
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
