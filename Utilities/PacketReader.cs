using System;
using System.Collections.Generic;
public class PacketReader
{
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
