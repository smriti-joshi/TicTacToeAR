using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    private volatile bool hasUpdate;
    Packet message;
    private volatile bool isHost = false;

    public bool HasUpdate { get => hasUpdate; set => hasUpdate = value; }
    public bool IsHost { get => isHost; set => isHost = value; }

    public void StartHost ()
    {
        // create socket to listen
        Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        EndPoint endPoint=new IPEndPoint(IPAddress.Any, 9011);
        socket.Bind (endPoint);
        socket.Listen (5);

        Thread thread = new Thread (() =>
        {
            while (true)
            {
                Console.WriteLine ("waiting for new connection...");
                clientSocket = socket.Accept ();
                Console.WriteLine ("connected");
            }
        });

        thread.Start ();
        isHost = true;
    }

    public bool StartClient (string ipAddress)
    {
        clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
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
        clientSocket.Send (dataBytes);
    }

    public Packet Receive ()
    {
        byte[] buffer=new byte[1024*4];
        int readBytes = clientSocket.Receive(buffer);
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

    private void ProcessMessage (Packet packet)
    {
        // id = 0 -> player connected
        // id = 0 -> player made a move
        hasUpdate = true;
        message = packet;
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
        return message;
    }


}

