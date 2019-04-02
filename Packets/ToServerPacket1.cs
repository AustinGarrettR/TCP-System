using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ToServerPacket_1 : Packet
{
    //Packet ID
    public int packetId { get => 1; }

    //Data
    public int index;
    public string sentence;
    public float[] floats;

    //Constructor
    public ToServerPacket_1(int index, string sentence, float[] floats)
    {
        this.index = index;
        this.sentence = sentence;
        this.floats = floats;
    }

    
}
