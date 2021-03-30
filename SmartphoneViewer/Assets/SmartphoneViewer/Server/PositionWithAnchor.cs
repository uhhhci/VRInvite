using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Ermittelt die Position des virtuellen Ankers und der virtuellen Smartphone-Kamera.
 * 
 * Siehe ExampleMemory für eine Umsetzung mit passiv oder TV. 
 */
public class PositionWithAnchor : MonoBehaviour // SERVER
{
    public Camera vrCam;

    public Vector3 _distanceToAnchor = new Vector3(0,0,0);
    public Vector3 _distanceToAnchorRot = new Vector3(100,100,100);
    private Vector3 _oldDistanceToAnchor = new Vector3(0, 0, 0);


    void Update()
    {
            if (_oldDistanceToAnchor != _distanceToAnchor && _distanceToAnchorRot != null) 
            {
                transform.localPosition = _distanceToAnchor;
                transform.localRotation = Quaternion.Euler(_distanceToAnchorRot);

                _oldDistanceToAnchor = _distanceToAnchor;

                //Debug.Log("RenderCamPosition: " + transform.position + " _distanceToAnchor: " + _distanceToAnchor + " Rotation: " + _distanceToAnchorRot);            
                } 
             }
  }
