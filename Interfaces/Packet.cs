using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Packet(-1, "Undefined Packet")]
public interface Packet
{
    int packetId { get; }
    byte[] getBytes();
    void readPacket(byte[] bytes);
}
