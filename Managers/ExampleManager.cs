using UnityEngine;

public class ExampleManager : Manager
{

    private ConnectionManager connectionHandler;

    public override void init(params object[] parameters)
    {
        connectionHandler = (ConnectionManager) parameters[0];
        //Add our function to the event
        connectionHandler.NotifyPacketReceived += receivePacket;
    }

    public override void update()
    {
        
    }

    public override void shutdown()
    {
        
    }

    //Event function for receiving packets.
    public void receivePacket(int packetId, byte[] rawPacket)
    {
        if(packetId == 1)
        {
            ExamplePacket_1 packet = new ExamplePacket_1();
            packet.readPacket(rawPacket);
            doAction(packet.index, packet.index2);
        }
    }

    //Action for packet 1
    public void doAction(int index, int index2)
    {
        Debug.Log("Action completed. Index:" + index + " Index2:" + index2);
    }

}
