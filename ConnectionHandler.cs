using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;

public class ConnectionHandler : Manager
{

    /*
     * Override Functions
     */

    public override void init()
    {
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    public override void update()
    {

        lock (lockObject)
        {
            foreach (Action action in qeuedFunctions)
            {

                action.Invoke();
            }
            qeuedFunctions.Clear();
        }

    }

    public override void shutdown()
    {
        throw new NotImplementedException();
    }

    /*
     * Internal Variables
     */

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
    public delegate void NotifyPacketReceivedDelegate(int packetId, Packet packet);

    //Events
    public event NotifyClientDisconnectedDelegate NotifyClientDisconnected;
    public event NotifyClientConnectedDelegate NotifyClientConnected;
    public event NotifyPacketReceivedDelegate NotifyPacketReceived;

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
            packetBytes = MarshalConverter.getBytes(packet);
        } catch
        {
            throw new Exception("Unable to serialize packet struct data to byte array for packet: "+packet.packetId);
        }

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

    //Repeat Async TCP Listen function
    private void ListenForIncommingRequests()
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

    //Repeat method for TCP Listener
    private void RunLoopStep()
    {
        if (disconnectedClients.Count > 0)
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

        if (tcpListener.Pending())
        {
            TcpClient newClient = tcpListener.AcceptTcpClient();
            newClient.NoDelay = true;
            OnPlayerConnected(newClient);
        }

        foreach (var c in connectedClients)
        {

            if (IsSocketConnected(c.Client) == false)
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

        int packetId = PacketReader.ReadIntAsync(ref bytes);
        Packet packet = MarshalConverter.fromBytes<Packet>(bytes);

        lock (lockObject)
        {
            qeuedFunctions.Add(() => NotifyPacketReceived(packetId, packet));
        }
    }

    //Triggers event for a client connecting
    private void OnPlayerConnected(TcpClient client)
    {
        lock (lockObject)
        {
            qeuedFunctions.Add(() => NotifyClientConnected(client));
        }
        Debug.Log("Player Connected. IP:" + ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString());
    }

    //Triggers event for a client disconnecting
    private void OnPlayerDisconnected(TcpClient client)
    {
        lock (lockObject)
        {
            qeuedFunctions.Add(() => NotifyClientDisconnected(client));
        }
    }    
}
