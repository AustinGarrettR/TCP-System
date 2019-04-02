using System;
using System.Collections.Generic;
public class PacketReader
{
	private static byte[] packetBytes;
	public static void ReadPacket(byte[] packet)
	{
		PacketReader.packetBytes = packet;
	}
	public static int ReadInt()
	{
		byte[] array = PacketReader.packetBytes;
		Array.Copy(PacketReader.packetBytes, 0, array, 0, 4);
		int @int = ByteConverter.getInt(array);
		int num = 4;
		int num2 = PacketReader.packetBytes.Length - num;
		byte[] destinationArray = new byte[num2];
		Array.Copy(PacketReader.packetBytes, num, destinationArray, 0, num2);
		PacketReader.packetBytes = destinationArray;
		return @int;
	}
	public static float ReadFloat()
	{
		byte[] array = PacketReader.packetBytes;
		Array.Copy(PacketReader.packetBytes, 0, array, 0, 4);
		float @float = ByteConverter.getFloat(array);
		int num = 4;
		int num2 = PacketReader.packetBytes.Length - num;
		byte[] destinationArray = new byte[num2];
		Array.Copy(PacketReader.packetBytes, num, destinationArray, 0, num2);
		PacketReader.packetBytes = destinationArray;
		return @float;
	}
	public static string ReadString()
	{
		int num = PacketReader.ReadInt();
		List<byte> list = new List<byte>(PacketReader.packetBytes);
		byte[] bytes = list.GetRange(0, num).ToArray();
		string @string = ByteConverter.getString(bytes);
		int num2 = num;
		int num3 = PacketReader.packetBytes.Length - num2;
		byte[] destinationArray = new byte[num3];
		Array.Copy(PacketReader.packetBytes, num2, destinationArray, 0, num3);
		PacketReader.packetBytes = destinationArray;
		return @string;
	}
    public static byte[] getRemainingBytes()
    {
        return packetBytes;
    }

    public static int ReadIntAsync(ref byte[] bytes)
    {
        byte[] array = bytes;
        Array.Copy(bytes, 0, array, 0, 4);
        int @int = ByteConverter.getInt(array);
        int num = 4;
        int num2 = bytes.Length - num;
        byte[] destinationArray = new byte[num2];
        Array.Copy(bytes, num, destinationArray, 0, num2);
        bytes = destinationArray;
        return @int;
    }
}
