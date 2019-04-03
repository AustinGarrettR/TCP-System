using System;

[Serializable]
[Packet(1, "Example packet used for testing.")]
public class ExamplePacket_1 : Packet
{
    //Packet Id
    int Packet.packetId => 1;

    //Data
    public int index;
    public int index2;

    public byte[] getBytes()
    {
        byte[] bytes = null;
        PacketWriter.Add(ref bytes, index); //1-int
        PacketWriter.Add(ref bytes, index2); //2-int
        return bytes;
    }

    public void readPacket(byte[] bytes)
    {
        index = PacketReader.ReadInt(ref bytes); //1-int
        index2 = PacketReader.ReadInt(ref bytes); //2-int
    }

}
