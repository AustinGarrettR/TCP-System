using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;

public class ConnectionManager : Manager
{

    /*
     * Constructor
     */

    //Assign if instance is running as server or client
    public ConnectionManager(bool isServer)
    {
        this._isServer = isServer;
    }

    /*
     * Override Functions
     */

    //Called on start
    public override void init(params System.Object[] parameters)
    {
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(isServer ? new ThreadStart(serverListenLoop) : new ThreadStart(clientListenLoop));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    //Called every frame on main thread
    public override void update()
    {

        //Lock to ensure thread safety
        lock (lockObject)
        {
            //Iterate queue'd up event functions on main thread, originating from async thread
            foreach (Action action in qeuedFunctions)
            {
                if(action != null)
                    action.Invoke();
            }
            qeuedFunctions.Clear();
        }

    }

    //Called on program shutdown
    public override void shutdown()
    {

        tcpListenerThread.Abort();

        //Disconnect each client
        foreach(TcpClient c in connectedClients)
        {
            if(c != null)
                c.Close();            
        }

        //Close listener
        if(tcpListener != null)
            tcpListener.Stop();
    }

    /*
     * Internal Variables
     */

    //Is instance running as server or client
    private bool _isServer;
    public virtual bool isServer { get => _isServer; }

    //Configuration
    private byte delimiter = SharedConfig.DELIMITER;
    private byte _escape = SharedConfig.ESCAPE;
    private int port = SharedConfig.TCP_PORT;
    private int maxBufferSize = SharedConfig.MAX_BUFFER_SIZE;

    //TCP Variables
    private TcpListener tcpListener;
    private Thread tcpListenerThread;

    //Event Delegates
    public delegate void NotifyClientDisconnectedDelegate(TcpClient client);
    public delegate void NotifyClientConnectedDelegate(TcpClient client);
    public delegate void NotifyPacketReceivedDelegate(int packetId, byte[] rawPacket);
    public delegate void NotifyFailedConnectDelegate();
    public delegate void NotifyOnDisconnectedFromServerDelegate();
    public delegate void NotifyOnConnectedToServerDelegate();

    //Events
    public event NotifyClientDisconnectedDelegate NotifyClientDisconnected;
    public event NotifyClientConnectedDelegate NotifyClientConnected;
    public event NotifyPacketReceivedDelegate NotifyPacketReceived;
    public event NotifyFailedConnectDelegate NotifyFailedConnect;
    public event NotifyOnDisconnectedFromServerDelegate NotifyOnDisconnectedFromServer;
    public event NotifyOnConnectedToServerDelegate NotifyOnConnectedToServer;

    //Locks
    private System.Object lockObject = new object();

    //Client Lists
    private List<TcpClient> connectedClients = new List<TcpClient>();
    private List<TcpClient> disconnectedClients = new List<TcpClient>();

    //Clients currently who have received an escape byte and are waiting for next byte
    private Dictionary<TcpClient, Boolean> escapedClients = new Dictionary<TcpClient, bool>();

    //Queued Functions (Used for events)
    private List<Action> qeuedFunctions = new List<Action>();

    //Queued Packet buffer
    private List<byte> queuedMsg = new List<byte>();

    //Client connected boolean
    private bool connected;


    /*
     * Internal Functions
     */

    //Public Gateway method for sending packet to server. 
    public void sendPacketToServer(Packet packet)
    {
        if(connectedClients.Count != 1)
        {
            throw new Exception("Attempted sending packet to server when there is either 0 or more than 1 connection active, indicating this is the server.");
        }

        sendPacket(connectedClients[0], packet);
    }

    //Public Gateway method for sending packet to client
    public void sendPacketToClient(TcpClient client, Packet packet)
    {
        sendPacket(client, packet);
    }

    //Internal Method for sending packets
    private void sendPacket(TcpClient c, Packet packet)
    {
        //Ensure TcpClient is not null
        if (c == null) return;

        byte[] packetBytes;

        //Serialize packet struct data to byte array
        try
        {
            packetBytes = packet.getBytes();
        } catch (Exception e)
        {
            throw e;
        }

        Debug.Log("Packet bytes size:" + packetBytes.Length);

        //Send packetID over network
        sendEscapedBytes(c, ByteConverter.getBytes(packet.packetId));       

        //Send packet data over network
        sendEscapedBytes(c, packetBytes);        

        //Send delimiter to indiciate the end of packet
        c.GetStream().WriteByte(_escape);
        c.GetStream().WriteByte(delimiter);
    }

    //Escape our bytes and send over network
    private void sendEscapedBytes(TcpClient c, byte[] bytes)
    {
        foreach (byte b in bytes) {
            if (b == _escape)
            {
                c.GetStream().WriteByte(_escape);
                c.GetStream().WriteByte(_escape);
            }
            else
                c.GetStream().WriteByte(b);
        }
    }

    //Repeat Async TCP Listen function for Server
    private void serverListenLoop()
    {
        try
        {
            // Create listener on localhost port		
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Debug.Log("TCP Listening on port "+port);
            Byte[] bytes = new Byte[maxBufferSize];
            while (true)
            {
                RunLoopStep();
                System.Threading.Thread.Sleep(10);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    //Repeat Async TCP Listen function for Client
    private void clientListenLoop()
    {
        try
        {
            // Create listener on localhost port

            TcpClient server;
            try
            {

                server = new TcpClient();
                server.NoDelay = true;
                server.Connect(IPAddress.Loopback, port);
                Debug.Log("Connecting to server on port " + port);

            }
            catch (SocketException e)
            {
                OnFailedConnect();
                Debug.LogError(e);
                System.Threading.Thread.CurrentThread.Abort();

                return;
            } catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            connectedClients.Add(server);

            Byte[] bytes = new Byte[maxBufferSize];
            while (true)
            {
                RunLoopStep();
                System.Threading.Thread.Sleep(10);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    //Indicates if socket is connected
    private bool IsSocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if ((part1 && part2) || !s.Connected)
            return false;
        else
            return true;
    }

    //Client disconnect
    public void Disconnect()
    {
        if(isServer)
        {
            throw new Exception("Server is attempting to disconnect as a client.");
        }

        TcpClient tcpClient = connectedClients[0];
        if (tcpClient == null) return;

        tcpClient.Close();
        tcpClient = null;
        if (connected == true)
        {
            connected = false;
            OnDisconnectedFromServer();
        }
    }

    //Repeat method for TCP Listener
    private void RunLoopStep()
    {
        //Is server, check for disconnecting clients
        if (isServer && disconnectedClients.Count > 0)
        {
            TcpClient[] _disconnectedClients = disconnectedClients.ToArray();
            this.disconnectedClients.Clear();

            foreach (var disC in _disconnectedClients)
            {
                connectedClients.Remove(disC);
                escapedClients.Remove(disC);
                OnPlayerDisconnected(disC);

            }
        }

        //Is server and is receiving a pending connection
        if (isServer && tcpListener.Pending())
        {
            TcpClient newClient = tcpListener.AcceptTcpClient();
            newClient.NoDelay = true;

            connectedClients.Add(newClient);
            escapedClients.Add(newClient, false);

            OnPlayerConnected(newClient);
        }

        //Client code
        if (isServer == false)
        {
            if (connectedClients[0] == null || IsSocketConnected(connectedClients[0].Client) == false)
            {
                if (connected == true)
                {
                    Debug.Log("Lost connection to the server.");
                    connected = false;
                    OnDisconnectedFromServer();
                }
                return;
            }
            else if (connectedClients[0].Connected)
            {
                if (connected == false)
                {
                    connected = true;
                    OnConnectedToServer();
                }
            }
        }

        foreach (var c in connectedClients)
        {

            if (isServer && IsSocketConnected(c.Client) == false)
            {
                disconnectedClients.Add(c);
                continue;
            }

            int bytesAvailable = c.Available;
            if (bytesAvailable == 0)
            {
                continue;
            }

            List<byte> bytesReceived = new List<byte>();

            while (c.Available > 0 && c.Connected)
            {
                byte[] nextByte = new byte[1];
                c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
                bytesReceived.AddRange(nextByte);
                if (nextByte[0] == _escape)
                {
                    escapedClients[c] = true;
                }
                else
                {
                    queuedMsg.AddRange(nextByte);
                }

                if (escapedClients[c])
                {
                    if (c.Available > 0)
                    {
                        escapedClients[c] = false;
                        byte[] nextByte2 = new byte[1];
                        c.Client.Receive(nextByte2, 0, 1, SocketFlags.None);
                        if (nextByte2[0] == delimiter)
                        {
                            if (queuedMsg.Count > 0)
                            {
                                byte[] bytes = queuedMsg.ToArray();
                                queuedMsg.Clear();                                                                
                                OnPacketReceived(bytes);
                            }
                        }
                        else
                        {
                            queuedMsg.AddRange(nextByte2);
                        }
                    }
                }
            }
        }
    }

    //Triggers event for packet receiving
    private void OnPacketReceived(byte[] bytes)
    {

        int packetId = PacketReader.ReadInt(ref bytes);

        lock (lockObject)
        {
            qeuedFunctions.Add(() => NotifyPacketReceived(packetId, bytes));
        }
    }

    //Triggers event for a client connecting
    private void OnPlayerConnected(TcpClient client)
    {
        if (NotifyClientConnected != null)
            lock (lockObject)
            {
                qeuedFunctions.Add(() => NotifyClientConnected(client));
            }
        Debug.Log("Server: "+isServer+". Connected to IP:" + ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString());
    }

    //Triggers event for a client disconnecting
    private void OnPlayerDisconnected(TcpClient client)
    {
        if(NotifyClientDisconnected != null)
            lock (lockObject)
            {
                qeuedFunctions.Add(() => NotifyClientDisconnected(client));
            }
    }

    //Triggers event for a client failing to connect
    private void OnFailedConnect()
    {
        if(NotifyFailedConnect != null)
            lock (lockObject)
            {
                qeuedFunctions.Add(() => NotifyFailedConnect());
            }
    }

    //Triggers event when disconnected from server
    private void OnDisconnectedFromServer()
    {
        if(NotifyOnDisconnectedFromServer != null)
            lock (lockObject)
            {
                qeuedFunctions.Add(() => NotifyOnDisconnectedFromServer());
            }
    }

    //Triggers event when connected to server
    private void OnConnectedToServer()
    {
        if(NotifyOnConnectedToServer != null)
            lock (lockObject)
            {
                qeuedFunctions.Add(() => NotifyOnConnectedToServer());
            }
    }
}
