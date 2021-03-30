using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.UI;

/*
 * Zuständig für Kommunikation mit Server.
 * Gibt Server Rückmeldung zu empfangenen Bilddaten.
 * Verarbeitet das empfangene Bild und zeigt es in der UI an. 
 * Sendet die Positionsdaten für Tracking und ggf. Interaktionsdaten.
 */
public class ClientControl : MonoBehaviour
{
    Socket socket;
    Socket socketAnswer;
    Socket socketTouch;

    int _byteLength;
    bool _complete = true;
    byte[] _file;
    int _bytesWritten;
    bool _shouldAssemble;
    bool _wait = false;
    bool _waiting = false;
    public bool _sendTouch = false;
    byte[] _savedImageFile = null;

    Texture2D _imageTex;
    Texture2D _recImage;

    public RawImage _displayImage;

    //Cam Distance 
    public MyAnchorController _myAnchor;
    public Vector3 _distance = new Vector3(100, 100, 100);
    private bool _mayDisplay = false;
    public Camera _firstPersonCamera;
    public Vector3 _distanceRot = new Vector3(100,100,100);

    //Touch
    /*public int rayLength;
    public LayerMask layermask; */


    void Start()
    {
        _imageTex = new Texture2D(1920, 1080);
        _displayImage.enabled = false;
    }

    void Update()
    {
        if (_shouldAssemble)
        {
            _shouldAssemble = false;

            _imageTex.LoadImage(_savedImageFile);
            UseAssembledTexture(_imageTex);
        }

        if (_wait)
        {
            if (!_waiting)
            {
                StartCoroutine(waitAndRestart(0.1f));
                _waiting = true;
            }
        }

        SendCamDistance();
        // SendTouchEvent(); 
    }


    // Startet socketRead mit Onreceive
    public void OnClientStarted(Socket socket)
    {
      //  Debug.Log("Client started");
        this.socket = socket;
        SocketRead.Begin(socket, OnReceive, OnError);
    }

    public void OnClientStartedAnswer(Socket socket)
    {
      //  Debug.Log("CLient Answer started");
        this.socketAnswer = socket;
        SocketRead.Begin(socket, OnReceive, OnError);
    }

    public void OnClientStartedTouch(Socket socket)
    {
      //  Debug.Log("CLient Touch started");
        this.socketTouch = socket;
        SocketRead.Begin(socket, OnReceive, OnError);
    }


 
    void UseAssembledTexture(Texture2D recImageTex)
    {
        if (!ValidTexture(recImageTex))
            _recImage = null; 
        else
            _recImage = recImageTex;

        DisplayOnScreen();
    }

    private bool ValidTexture(Texture2D tex)
    {
        if (tex == null)
            return false;

        // Valid if not 8x8 Textture
        else if (tex.width == 8 || tex.height == 8)
            return false;

        return true;
    }


    void OnReceive(SocketRead read, byte[] data)
    {
       // Debug.Log("DataLength Array: " + data.Length);
        if (_complete && data.Length == 4) // laenge der msg wird in 4 bytes geschickt
        {
            _complete = false;
            _bytesWritten = 0;
            _byteLength = System.BitConverter.ToInt32(data, 0);
            if (_byteLength > 0)
                _file = new byte[_byteLength];

            ReportLengthDone();
        }
        else
        {
            if (!_wait)
            {
                try
                {
                    System.Array.Copy(data, 0, _file, _bytesWritten, data.Length);
                    _bytesWritten += data.Length;
                   // Debug.Log("bytesWritten: " + _bytesWritten + "bytesLength: " + _byteLength);
                }
                catch
                {
                    print("D Error: too many bytes for file received. Waiting for new beginning");
                    _wait = true;
                }
            }
        }
        if (!_wait)
        {
            if (_bytesWritten == _byteLength)
            {
                _savedImageFile = _file; // saves in new File avoiding red error questionmark, stays in previous texture
                _complete = true;
                _shouldAssemble = true;
                ReportDone();
            }
        }
    }


    void OnError(SocketRead read, System.Exception exception)
    {
        Debug.LogError("Client Receive error: " + exception);
    }


    IEnumerator waitAndRestart(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("D Resuming after wait");
        _complete = true;
        _wait = false;
        _waiting = false;
        ReportDone();
    }

    
    // Send Report String
    void ReportDone()
    {
        string messageToSend = "Done";
        socket.Send(Encoding.ASCII.GetBytes(messageToSend)); 
    }

    void ReportLengthDone()
    {
        string messageToSend = "Length Done";
        socket.Send(Encoding.ASCII.GetBytes(messageToSend));
    } 

 
    public void SendCamDistance()
    {
        if (_myAnchor.GetDistanceVector() != null && _myAnchor.GetDistanceVector() != _distance && _myAnchor.GetDistanceVectorRot() != null) 
        {
            _distance = _myAnchor.GetDistanceVector();
            _distanceRot = _myAnchor.GetDistanceVectorRot();

            var _data = new CamTrackingData(_distance, _distanceRot); 
            byte[] distanceMsg =_data.ToArray();

                System.Buffer.BlockCopy(BitConverter.GetBytes(_distance.x), 0, distanceMsg, 0, 4);
                System.Buffer.BlockCopy(BitConverter.GetBytes(_distance.y), 0, distanceMsg, 4, 4);
                System.Buffer.BlockCopy(BitConverter.GetBytes(_distance.z), 0, distanceMsg, 8, 4);

                socketAnswer.Send(distanceMsg);
        }
    }


    void DisplayOnScreen() 
    {
        if(_myAnchor._createdAnchor)
        {
            _firstPersonCamera.enabled = false;
            _displayImage.enabled = true;

            if (_imageTex != null)
                {
                    _displayImage.texture = _imageTex;
                    _displayImage.transform.SetAsFirstSibling();
                }
         } 
    }


    //Touch
    /*
    void SendTouchEvent()
    {
        if (_sendTouch)
        {
            Touch touch;
            if (Input.touchCount != 1 ||
                (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }
            socketTouch.Send(Encoding.ASCII.GetBytes("ScreenTouched")); 
        }
    } */
}
