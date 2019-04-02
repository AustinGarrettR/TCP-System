using UnityEngine;

public class ExampleManager
{
    //Constructor
    public ExampleManager(ConnectionHandler connectionHandler)
    {
        //Add our function to the event
        connectionHandler.NotifyPacketReceived += receivePacket;
    }

    //Event function for receiving packets.
    public void receivePacket(int packetId, Packet packet)
    {
        if(packetId == 1)
        {
            ExamplePacket_1 data = (ExamplePacket_1) packet;
            doAction(data.index, data.sentence, data.floats);
        }
    }

    //Action for packet 1
    public void doAction(int index, string sentence, float[] floats)
    {
        Debug.Log("Action completed. Index:" + index + " Sentence:" + sentence + " floats length:" + floats.Length);
    }

}
