using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

/*
 * Zuständig für Verbindungsaufbau zwischen Server und Clients.
 * Handelt es sich um den Server, versucht dieser die Sockets zu öffnen und wartet auf eine eingehende Verbindung. 
 * Handelt es sich um einen Client, versucht dieser sich mit dem bestehenen Socket des Servers zu verbinden. 
 */
public class NetworkControl : MonoBehaviour
{
    public string _serverIP = "192.168.2.116"; 

    public const string kServerArgument = "-server";
    public bool _isServerOption;

    private bool _isServer;

    public ServerControl _serverControl;
    public ClientControl _clientControl;
    public ClientControl _clientControlAnswer;
    public ClientControl _clientControlTouch;

    private int _connect_port_image = 8888;
    private int _port_1_image = 8888;
    private int _port_answer = 8887;
    private int _port_touch = 8889;

    const int kHostConnectionBacklog = 10; 

    static NetworkControl instance;

    string message = "Awake";
    Socket socketImage;
    Socket _socketAnswer;
    Socket _socketTouch;
    IPAddress ip; 


    public static NetworkControl Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (NetworkControl)FindObjectOfType(typeof(NetworkControl));
            }

            return instance;
        }
    }


    public static Socket SocketImage
    {
        get
        {
            return Instance.socketImage;
        }
    }



    void Start()
    {
        Screen.fullScreen = false;

        _connect_port_image = _port_1_image;
        Application.RegisterLogCallbackThreaded(OnLog);

        IPAddress _ip = IPAddress.Parse(_serverIP);

        _isServer = false;


        if (_isServerOption)
        {
            _isServer = true;
        }

        if (_isServer)
        {
            _clientControl.gameObject.SetActive(false);

            // if host
            if (Host(_port_1_image) && HostAnswer(_port_answer) && HostTouch(_port_touch))
            {
                _serverControl.enabled = true;
                _serverControl.OnServerStarted();
            }

            _clientControlAnswer.gameObject.SetActive(false);
            _clientControlTouch.gameObject.SetActive(false);

        }
        else
        {
            //if client
            if (Connect(_ip, _connect_port_image))
            {
                _clientControl.enabled = true;
                _clientControl.OnClientStarted(socketImage);
            }

            if (ConnectAnswer(_ip, _port_answer))
            {
                _clientControlAnswer.enabled = true;
                _clientControlAnswer.OnClientStartedAnswer(_socketAnswer);
            }

            if (ConnectTouch(_ip, _port_touch))
            {
                _clientControlTouch.enabled = true;
                _clientControlTouch.OnClientStartedTouch(_socketTouch);
            }
        }
    }



    void OnApplicationQuit()
    {
        Disconnect();
    }


    public bool Host(int port)
    {
        Debug.Log("ImagePort Hosting on port " + port);

        socketImage = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socketImage.Bind(new IPEndPoint(IP, port));
            socketImage.Listen(kHostConnectionBacklog);
            socketImage.BeginAccept(new System.AsyncCallback(OnClientConnect), socketImage);
            Debug.Log("Host tried successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("ImagePort Exception when attempting to host (" + port + "): " + e);

            socketImage = null;

            return false;
        }

        return true;
    }



    public bool HostAnswer(int port)
    {
        Debug.Log("AnswerPorst Hosting on port " + port);

        _socketAnswer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _socketAnswer.Bind(new IPEndPoint(IP, port));
            _socketAnswer.Listen(kHostConnectionBacklog);
            _socketAnswer.BeginAccept(new System.AsyncCallback(OnClientConnectAnswer), _socketAnswer); 
            Debug.Log("HostAnswer tried successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("AnswerPort Exception when attempting to host (" + port + "): " + e);

            _socketAnswer = null;

            return false;
        }

        return true;
    }

    public bool HostTouch(int port)
    {
        Debug.Log("AnswerTouch Hosting on port " + port);

        _socketTouch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _socketTouch.Bind(new IPEndPoint(IP, port));
            _socketTouch.Listen(kHostConnectionBacklog);
            _socketTouch.BeginAccept(new System.AsyncCallback(OnClientConnectTouch), _socketTouch);
            Debug.Log("HostTouch tried successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("TouchPort Exception when attempting to host (" + port + "): " + e);

            _socketTouch = null;

            return false;
        }

        return true;
    }



    public bool Connect(IPAddress ip, int port)
    {
        Debug.Log("ImagePort Connecting to " + ip + " on port " + port);

        socketImage = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socketImage.Connect(new IPEndPoint(ip, port));

        if (!socketImage.Connected)
        {
            Debug.LogError("ImagePort Failed to connect to " + ip + " on port " + port);

            socketImage = null;
            return false;
        }

        return true;
    }



    public bool ConnectAnswer(IPAddress ip, int port)
    {
        Debug.Log("ImagePort Connecting to " + ip + " on port " + port);

        _socketAnswer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socketAnswer.Connect(new IPEndPoint(ip, port));

        if (!_socketAnswer.Connected)
        {
            Debug.LogError("ImagePort Failed to connect to " + ip + " on port " + port);

            _socketAnswer = null;
            return false;
        }

        return true;
    }

    public bool ConnectTouch(IPAddress ip, int port)
    {
        Debug.Log("TouchPort Connecting to " + ip + " on port " + port);

        _socketTouch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socketTouch.Connect(new IPEndPoint(ip, port));

        if (!_socketTouch.Connected)
        {
            Debug.LogError("ImagePort Failed to connect to " + ip + " on port " + port);

            _socketTouch = null;
            return false;
        }

        return true;
    }



    void Disconnect()
    {
        if (socketImage != null)
        {
            socketImage.BeginDisconnect(false, new System.AsyncCallback(OnEndHostComplete), socketImage);
            socketImage.Shutdown(SocketShutdown.Both);
            //socketImage.Close();
        }

        if (_socketAnswer != null)
        {
            _socketAnswer.BeginDisconnect(false, new System.AsyncCallback(OnEndHostComplete), _socketAnswer);
            _socketAnswer.Shutdown(SocketShutdown.Both);
        }

        if (_socketTouch != null)
        {
            _socketTouch.BeginDisconnect(false, new System.AsyncCallback(OnEndHostComplete), _socketTouch);
            _socketTouch.Shutdown(SocketShutdown.Both);
        }
    }


    // wird in Host aufgerufen
    void OnClientConnect(System.IAsyncResult result)
    {
        Debug.Log("ImagePort Handling client connecting");

        try
        {
            _serverControl.OnClientConnected(socketImage.EndAccept(result));
        }
        catch (System.Exception e)
        {
            Debug.LogError("ImagePort Exception when accepting incoming connection: " + e);
        }

        try
        {
            socketImage.BeginAccept(new System.AsyncCallback(OnClientConnect), socketImage);
        }
        catch (System.Exception e)
        {
            Debug.LogError("ImagePort Exception when starting new accept process: " + e);
        }
    }


    

    void OnClientConnectAnswer(System.IAsyncResult result)
    {
        Debug.Log("AnswerPort Handling client connecting");

        try
        {
            _serverControl.OnClientConnectedAnswer(_socketAnswer.EndAccept(result));
        }
        catch (System.Exception e)
        {
            Debug.LogError("AnswerPort Exception when accepting incoming connection: " + e);
        }

        try
        {
            _socketAnswer.BeginAccept(new System.AsyncCallback(OnClientConnectAnswer), _socketAnswer);
        }
        catch (System.Exception e)
        {
            Debug.LogError("AnswerPort Exception when starting new accept process: " + e);
        }
    }

    void OnClientConnectTouch(System.IAsyncResult result)
    {
        Debug.Log("TouchPort Handling client connecting");

        try
        {
            _serverControl.OnClientConnectedTouch(_socketTouch.EndAccept(result));
        }
        catch (System.Exception e)
        {
            Debug.LogError("TouchPort Exception when accepting incoming connection: " + e);
        }

        try
        {
            _socketTouch.BeginAccept(new System.AsyncCallback(OnClientConnectTouch), _socketTouch);
        }
        catch (System.Exception e)
        {
            Debug.LogError("TouchPort Exception when starting new accept process: " + e);
        }
    }



    // For Disconnect
    void OnEndHostComplete(System.IAsyncResult result)
    {
        socketImage = null;
    }


    public IPAddress IP
    {
        get
        {
            if (ip == null)
            {
                ip = (
                    from entry in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    where entry.AddressFamily == AddressFamily.InterNetwork
                    select entry
                ).FirstOrDefault();
            }
            return ip;
        }
    }


    void OnLog(string message, string callStack, LogType type)
    {
        //this.message = message;
        this.message = message + "\n" + this.message;
        if (this.message.Length > 1999)
        {
            this.message = this.message.Substring(0, 2000);
        }

    }

    public bool IsServer()
    {
        return _isServer;
    }
}
