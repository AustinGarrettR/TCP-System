using System;
using System.Text;
public class ByteConverter
{
    public static byte[] getBytes(Object o)
    {
        if (o is int)
            return getBytes((int)o);
        if (o is short)
            return getBytes((short)o);
        if (o is float)
            return getBytes((float)o);
        if (o is long)
            return getBytes((long)o);
        if (o is string)
            return getBytes((string)o);
        else
            throw new Exception("Unsupported value type.");

    }
	public static byte[] getBytes(string s)
	{
		return Encoding.UTF8.GetBytes(s);
	}
	public static string getString(byte[] bytes)
	{
		return new string(Encoding.UTF8.GetChars(bytes));
	}
	public static byte[] getBytes(int I32)
	{
		return BitConverter.GetBytes(I32);
	}
    public static byte[] getBytes(short s)
    {
        return BitConverter.GetBytes(s);
    }
    public static byte[] getBytes(long l)
    {
        return BitConverter.GetBytes(l);
    }
    public static int getInt(byte[] b)
	{
		return BitConverter.ToInt32(b, 0);
	}
    public static long getLong(byte[] b)
    {
        return BitConverter.ToInt64(b, 0);
    }
    public static short getShort(byte[] b)
    {
        return BitConverter.ToInt16(b, 0);
    }
    public static float getFloat(byte[] b)
	{
		return BitConverter.ToSingle(b, 0);
	}
	public static byte[] getBytes(float f)
	{
		return BitConverter.GetBytes(f);
	}
}
