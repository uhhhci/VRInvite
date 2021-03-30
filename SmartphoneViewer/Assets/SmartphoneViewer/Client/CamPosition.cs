using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CamPosition : MonoBehaviour // CLIENT
{
    public Camera _ARcam;
    private Vector3 _ARcamPosition = new Vector3(0,0,0);

    Vector3 distance = new Vector3(0, 0, 0);
    
    void Update()
    {
        distance = _ARcam.transform.position;   
    }


    public Vector3 getCamDistance()
    {
        return distance;
    }
}
