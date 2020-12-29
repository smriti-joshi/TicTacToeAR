using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Net;


public struct Packet
{
    public int id;
    public string message;
    public int row;
    public int column;

    public Packet (string message)
    {
        id = 0;
        this.message = message;
        row = -1;
        column = -1;
    }
}

class Network
{
    private Socket clientSocket;
    Packet message;
    private volatile bool hasUpdate = false;
    private volatile bool isHost = false;
    private volatile int messageCode = -1;

    public bool HasUpdate { get => hasUpdate; set => hasUpdate = value; }
    public bool IsHost { get => isHost; set => isHost = value; }
    public int MessageCode { get => messageCode; set => messageCode = value; }

    public string StartHost ()
    {
        // create socket to listen
        Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        EndPoint endPoint=new IPEndPoint(IPAddress.Any, 9011);
        socket.Bind (endPoint);
        socket.Listen (5);

        Thread thread = new Thread (() =>
        {
            // wait for the client to connect and start receive packages once it's connected
            clientSocket = socket.Accept ();
            socket.Close ();
            Run ();
        });

        thread.Start ();
        isHost = true;
        return GetLocalIPAddress ();
    }

    public bool StartClient (string ipAddress)
    {
        clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect (ipAddress, 9011);

        // if player connected, send signal messsage to host
        Packet toSend = new Packet
        {
            id = 0,
            message = "connected"
        };
        Send (toSend);

        return clientSocket.Connected;
    }

    public void Send (Packet message)
    {
        string jsonData = JsonConvert.SerializeObject(message);
        byte[] dataBytes = Encoding.Default.GetBytes(jsonData);
        clientSocket.SendBufferSize = dataBytes.Length;
        clientSocket.Send (dataBytes, dataBytes.Length, 0);
    }

    public Packet Receive ()
    {
        byte[] buffer=new byte[16];
        int readBytes;

        readBytes = clientSocket.Receive (buffer);
        MemoryStream memoryStream = new MemoryStream();

        while (readBytes > 0)
        {
            memoryStream.Write (buffer, 0, readBytes);
            if (clientSocket.Available > 0)
            {
                readBytes = clientSocket.Receive (buffer);
            }
            else
            {
                break;
            }
        }

        byte[] totalBytes = memoryStream.ToArray();
        memoryStream.Close ();
        string readData = Encoding.Default.GetString(totalBytes);
        Packet response = JsonConvert.DeserializeObject<Packet>(readData);

        return response;
    }

    public void Run ()
    {
        Thread thread = new Thread (() =>
        {
            while (true)
            {
                if (hasUpdate)
                    continue;

                ProcessMessage (Receive ());
            }
        });

        thread.Start ();
    }

    public Packet GetMessage ()
    {
        hasUpdate = false;
        messageCode = -1;
        return message;
    }

    public void Disconnect ()
    {
        clientSocket.Close ();
    }

    private void ProcessMessage (Packet packet)
    {
        // id = 0 -> player connected
        // id = 1 -> player made a move
        // id = 2 -> player finalized the grid
        hasUpdate = true;
        messageCode = packet.id;
        message = packet;
    }

    // helper function to get the local ip address
    private string GetLocalIPAddress ()
    {
        string localIP;
        using (Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect ("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString ();
        }

        return localIP;
    }
}

