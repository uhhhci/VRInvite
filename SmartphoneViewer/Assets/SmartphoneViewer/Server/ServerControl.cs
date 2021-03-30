using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System;

/*
 * Zuständig für die Kommunikation mit den Clients.
 * Erstellt und sendet die Bilder.
 * Empfängt Positions- und ggf. Interaktionsdaten.
 */
public class ServerControl : MonoBehaviour
{
    List<Socket> clients = new List<Socket>();
    List<Socket> clientsAnswer = new List<Socket>();
    List<Socket> clientsTouch = new List<Socket>();
    Socket socket;

    public Camera smartCam3D;

    bool _maySend = true;
    bool _maySendFile = false;
    byte[] _imageData;
    byte[] _distanceData = null;
    byte[] _touchData = null;

    private Texture2D _image = null;
    private RenderTexture _currentRT;
    public PositionWithAnchor _positionWithAnchor;

    //Touch
    /*
    public float rayLength;
    public LayerMask colorLayermask;
    public GameObject smartphoneRay;
    private bool waitingForRay = false;

    private GameObject previousCardHint;
    private GameObject previousCardBack;
    private Color originalColorHint;
    private Color originalColorBack;
    private GameObject hintChild;
    private GameObject backChild; */
    
    
    public void OnServerStarted()
    {
        Debug.Log("Server started");
    }

    public void OnClientConnected(Socket client)
    {
        Debug.Log("Client connected");
        clients.Add(client);
        SocketRead.Begin(client, OnReceive, OnReceiveError);
    }

    public void OnClientConnectedAnswer(Socket client)
    {
        Debug.Log("ClientAnswer connected");
        clientsAnswer.Add(client);
        SocketRead.Begin(client, OnReceiveAnswer, OnReceiveError);
    }

    public void OnClientConnectedTouch(Socket client)
    {
        Debug.Log("ClientTouch connected");
        clientsTouch.Add(client);
        SocketRead.Begin(client, OnReceiveTouch, OnReceiveError);
    }

    void Update() 
    {
        if (NetworkControl.Instance.IsServer() && _maySend) // checkbox Server
        {
            if (_maySendFile)
                SendImage();
            else
                SendLength();
        }

        if (_distanceData != null)
            HandleCamDistance(_distanceData);

        //Touch 
        /* if (_touchData != null)
          HandleTouchMsg(_touchData); */
    }

    void OnReceive(SocketRead read, byte[] data)
    {
        string receivedMsg = Encoding.ASCII.GetString(data, 0, data.Length);

        if (receivedMsg == "Done")
        {
            _maySend = true;
        }

        if (receivedMsg == "Length Done")
        {
            _maySend = true;
            _maySendFile = true;
        }
    }


    void OnReceiveAnswer(SocketRead read, byte[] data)
    {
        _distanceData = data;
    }

    void OnReceiveTouch(SocketRead read, byte[] data)
    {
        _touchData = data;
    }



    // Get Texture and send length Image
    void SendLength()
    {
        if (!_maySend || clients.Count == 0)
            return;

        _maySend = false;

        _imageData = GetTexture(smartCam3D).EncodeToJPG(15);
        byte[] length = System.BitConverter.GetBytes(_imageData.Length);

        foreach (Socket client in clients)
        {
            if (client == null)
                return;

            client.Send(length);
          //  Debug.Log("Sent length " + _imageData.Length);
        }
    }


    // Image
    void SendImage()
    {
        if (!_maySend || clients.Count == 0 || !_maySendFile)
            return;

        _maySend = false;
        _maySendFile = false;

        foreach (Socket client in clients)
        {
            if (client == null) 
            {
                return;
            }
            client.Send(_imageData);
           // Debug.Log("Server sent new image file of " + _imageData.Length + " bytes");
        }
    }


    private Texture2D GetTexture(Camera camera)
    {
        _currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        camera.Render();

        if (_image == null)
        {
            _image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.RGBA32, false);
        }

        _image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        _image.Apply();

        // Raplace the original active Render Texture
        RenderTexture.active = _currentRT;

        return _image;
    }


    void OnReceiveError(SocketRead read, System.Exception exception)
    {
        Debug.LogError("Receive error: " + exception);
    }


    void HandleCamDistance(byte[] data)
    {
        var _receivedData = CamTrackingData.FromArray(data);
        //Debug.Log("Received Position: " + _receivedData._position + " Rotation: " + _receivedData._rotation);

        _positionWithAnchor._distanceToAnchor = _receivedData._position;
        _positionWithAnchor._distanceToAnchorRot = _receivedData._rotation;

        _distanceData = null;
    }


    //Touch example (memory vr game)
    /* void HandleTouchMsg(byte[] data)
    {
        string receivedTouchMsg = Encoding.ASCII.GetString(data, 0, data.Length);
        if (receivedTouchMsg.Equals("ScreenTouched"))
        {
            smartphoneRay.SetActive(true);
            StartCoroutine(waitForRay());

            Debug.Log("Screen Touched");

            if (previousCardHint != null)
            {
                previousCardHint.GetComponent<Renderer>().material.color = originalColorHint;
            }

            if (previousCardBack != null)
            {
                previousCardBack.GetComponent<Renderer>().material.color = originalColorBack;
            }

            RaycastHit hit;
            Ray ray = smartCam3D.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); 

            if (Physics.Raycast(ray, out hit, rayLength, colorLayermask))
            {
                if (hit.collider.GetComponent<Renderer>() != null)
                {
                    Debug.Log("Hit object!");

                    var hitParent = hit.transform.parent;
                    hintChild = hitParent.transform.Find("test").gameObject;
                    backChild = hitParent.transform.Find("Back").gameObject;


                    //Checks HINT TEST
                    if (hintChild.activeInHierarchy)
                    {
                        previousCardHint = hintChild;
                        originalColorHint = hintChild.GetComponent<Renderer>().material.color;

                        previousCardBack = backChild;
                        originalColorBack = backChild.GetComponent<Renderer>().material.color;

                        hintChild.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
                        backChild.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan * 3);
                    }
                    else
                    {
                            Debug.Log("Hit object!");
                            previousCardBack = hit.collider.transform.gameObject; //backChild

                            var pointerRenderer = hit.collider.GetComponent<Renderer>();
                            originalColorBack = pointerRenderer.material.color;
                            pointerRenderer.material.SetColor("_Color", Color.cyan * 3);
                    }
                }
           }
        } 

        _touchData = null;
    } 

    
    IEnumerator waitForRay()
    {
        waitingForRay = true;
        yield return new WaitForSeconds(1.0f);

        waitingForRay = false;
        smartphoneRay.SetActive(false);
    } */
}

