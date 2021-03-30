namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;

    /*
     * Zuständig für ARCore Session, ImageRecogntion, PlaneDetection.
     * Setzt bei erkannter Plane und Touch-Eingabe einen Anchor auf dem erkannten Bild.   
     */
    public class SceneController : MonoBehaviour
    {
      
        public MyAnchorController _myAnchor;
        public ClientControl _clientControl;

        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        private bool m_IsQuitting = false;

        private bool _touchDisabled = false;

        private GameObject cam;


        public AugmentedImageVisualizer AugmentedImageVisualizerPrefab;
        public GameObject FitToScanOverlay;

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
        private Anchor _imageAnchor;
        
    
        void Update()
        {
            _UpdateApplicationLifecycle();

            ProcessTouches();
        }

        public void Awake()
        {
            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;
        }


        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }

            // Get updated augmented images for this frame.
            Session.GetTrackables<AugmentedImage>(
                m_TempAugmentedImages, TrackableQueryFilter.Updated);

            // Create visualizers and anchors for updated augmented images that are tracking and do
            // not previously have a visualizer. Remove visualizers for stopped images.
            foreach (var image in m_TempAugmentedImages)
            {
                AugmentedImageVisualizer visualizer = null;
                m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                if (image.TrackingState == TrackingState.Tracking && visualizer == null)
                {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    Anchor anchor = image.CreateAnchor(image.CenterPose);
                    _imageAnchor = anchor;

                    _imageAnchor.transform.forward = anchor.transform.forward;

                    visualizer = (AugmentedImageVisualizer)Instantiate(
                        AugmentedImageVisualizerPrefab, anchor.transform);
                    visualizer.Image = image;
                    m_Visualizers.Add(image.DatabaseIndex, visualizer);
                }
                else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                {
                    m_Visualizers.Remove(image.DatabaseIndex);
                    GameObject.Destroy(visualizer.gameObject);
                }
            }

            // Show the fit-to-scan overlay if there are no images that are Tracking.
            foreach (var visualizer in m_Visualizers.Values)
            {
                if (visualizer.Image.TrackingState == TrackingState.Tracking)
                {
                    FitToScanOverlay.SetActive(false);
                    return;
                }
            }
            FitToScanOverlay.SetActive(true);
        }


        private void _DoQuit()
        {
            Application.Quit();
        }

        private void _ShowAndroidToastMessage(string message)
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


        //Performs the raycasting hit test and selects the plane that is tapped
        void ProcessTouches()
        {
            if (!_touchDisabled)
            {
                Touch touch;
                if (Input.touchCount != 1 ||
                    (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                {
                    return;
                }
                
                TrackableHit hit;
                TrackableHitFlags raycastFilter =
                    TrackableHitFlags.PlaneWithinBounds |
                    TrackableHitFlags.PlaneWithinPolygon;

                if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
                {
                    SetSelectedPlane(hit.Trackable as DetectedPlane);
                    _clientControl._sendTouch = true;
                    _touchDisabled = true;
                }
            }
        }

        //notifies all other controllers that a new plane has been selected
        void SetSelectedPlane(DetectedPlane selectedPlane)
        {
            if (_imageAnchor != null)
            {
                //Debug.Log("Selected plane centered at " + selectedPlane.CenterPose.position);
                _myAnchor.SetSelectedPlane(selectedPlane, _imageAnchor); //_myAnchor calls CreateAnchor() 
            } 
        }
    }
}