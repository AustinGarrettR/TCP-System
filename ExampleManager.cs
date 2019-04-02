using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleManager
{
    public ExampleManager(ConnectionHandler connectionHandler)
    {
        connectionHandler.NotifyPacketReceived += receivePacket;
    }

    public void receivePacket(int packetId, Packet packet)
    {
        if(packetId == 1)
        {
            ToServerPacket_1 data = (ToServerPacket_1) packet;
            doAction(data.index, data.sentence, data.floats);
        }
    }

    public void doAction(int index, string sentence, float[] floats)
    {
        Debug.Log("Action completed. Index:" + index + " Sentence:" + sentence + " floats length:" + floats.Length);
    }

}
