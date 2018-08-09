using System;
using System.Text;
using System.Security.Cryptography;

public class AesUtil
{
	// 加密
	public static byte[] Encrypt (string key, byte[] data)
	{
		byte[] keyArray = UTF8Encoding.UTF8.GetBytes (key);

		RijndaelManaged rDel = new RijndaelManaged ();
		rDel.Key = keyArray;
		rDel.Mode = CipherMode.ECB;
		rDel.Padding = PaddingMode.PKCS7;

		ICryptoTransform cTransform = rDel.CreateEncryptor ();
		return cTransform.TransformFinalBlock (data, 0, data.Length);
	}

	// 加密
	public static string Encrypt (string key, string data)
	{
		byte[] array = Encrypt (key, UTF8Encoding.UTF8.GetBytes (data));
		return Convert.ToBase64String (array, 0, array.Length);
	}

	// 解密
	public static byte[] Decrypt (string key, byte[] data)
	{
		byte[] keyArray = UTF8Encoding.UTF8.GetBytes (key);

		RijndaelManaged rDel = new RijndaelManaged ();
		rDel.Key = keyArray;
		rDel.Mode = CipherMode.ECB;
		rDel.Padding = PaddingMode.PKCS7;

		ICryptoTransform cTransform = rDel.CreateDecryptor ();
		return cTransform.TransformFinalBlock (data, 0, data.Length);
	}

	// 解密
	public static string Decrypt (string key, string data)
	{
		byte[] array = Decrypt (key, Convert.FromBase64String (data));
		return UTF8Encoding.UTF8.GetString (array);
	}
}
