using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;

/*
 * Erstellt einen Anchor auf der erkannten Plane.
 * Ermittelt den Posenunterschied der ARCore-Kamera zu diesem Anchor.
 */
public class MyAnchorController : MonoBehaviour
{
    public Camera _firstPersonCamera;
    public bool _touchEnabled = false;
    public Text _log;
    public GameObject _device;
    public GameObject _myAnchorObject;
    public GameObject _z;
    public GameObject _x;
    public GameObject _y;

    private Anchor _anchor;
    private DetectedPlane _detectedPlane;
    private float _yOffset;

    private Vector3 _difference = new Vector3(100, 100, 100);
    private Vector3 _differenceRot = new Vector3(100,100,100);
   
    public bool _createdAnchor = false;


    void Start()
    {
        //Disables mesh renderers from children until they are placed
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }

    void Update()
    {
        if(Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        // If there is no plane, return
        if (_detectedPlane == null)
        {
            return;
        }

        while (_detectedPlane.SubsumedBy != null)
        {
            _detectedPlane = _detectedPlane.SubsumedBy;
        }

        transform.position = new Vector3(transform.position.x,
                    _detectedPlane.CenterPose.position.y + _yOffset, transform.position.z); 

        if(_createdAnchor)
        {
            ReadCamTransform();
        }
    }


    public void SetSelectedPlane(DetectedPlane detectedPlane, Anchor imageAnchor)
    {
        _detectedPlane = detectedPlane;
        CreateAnchor(imageAnchor);
    }


    void CreateAnchor(Anchor imageAnchor)
    {
        // WITH AUGMENTED IMAGE

        Vector3 anchorPosition = imageAnchor.transform.position;

        if (_anchor != null)
        {
            DestroyObject(_anchor);
        }
        _anchor = _detectedPlane.CreateAnchor(new Pose(anchorPosition, Quaternion.identity));

        //Attach the anchorobject to the anchor
        transform.position = anchorPosition; //lies on empty GameObject
        transform.forward = imageAnchor.transform.forward; //aligns both Anchor forward vectors
        transform.SetParent(_anchor.transform);

        // Record the y offset from the plane.
        _yOffset = transform.position.y - _detectedPlane.CenterPose.position.y;

        // Enable the renderers.
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }

        _createdAnchor = true;
        ReadCamTransform();

        DrawCoordinates();
    }


      void ReadCamTransform() 
      {
          _firstPersonCamera.transform.parent = this.transform;

          _difference = _firstPersonCamera.transform.localPosition;
          _differenceRot = _firstPersonCamera.transform.localRotation.eulerAngles;
      
          _firstPersonCamera.transform.parent = _device.transform;
      } 

  
    public Vector3 GetDistanceVector()
    {
         return _difference;
    }

    public Vector3 GetDistanceVectorRot()
    { 
        return _differenceRot;
    }

    
    //FOR DEBUGGING
    void DrawCoordinates()
    {
        _z.transform.position = _myAnchorObject.transform.position;
        _z.transform.forward = _myAnchorObject.transform.forward;

        _y.transform.position = _myAnchorObject.transform.position;
        _y.transform.forward = _myAnchorObject.transform.up;

        _x.transform.position = _myAnchorObject.transform.position;
        _x.transform.forward = _myAnchorObject.transform.right; 
    }
}
