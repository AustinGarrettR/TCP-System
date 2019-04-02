using System;
using System.Text;
public class ByteConverter
{
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
	public static int getInt(byte[] b)
	{
		return BitConverter.ToInt32(b, 0);
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
