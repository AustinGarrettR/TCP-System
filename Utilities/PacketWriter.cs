using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PacketWriter
{

    //Converts object to byte array, limited to specific primitives
    public static void Add(ref byte[] packetBytes, System.Object data)
    {
        if (data is int || data is short || data is string || data is float || data is long)
        {
            byte[] bytes = ByteConverter.getBytes(data);
            if (packetBytes != null)
            {
                List<byte> list = new List<byte>(packetBytes);
                List<byte> collection = new List<byte>(bytes);
                list.AddRange(collection);
                packetBytes = list.ToArray();
            }
            else
            {
                packetBytes = bytes;
            }
        }
        else
        {
            throw new Exception("Object type is unsupported and can not be serialized.");
        }        

    }

}
