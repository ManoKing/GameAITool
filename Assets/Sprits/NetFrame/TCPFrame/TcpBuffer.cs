using System;
using System.Net;

// tcp 报文接收缓冲
public class TcpBuffer
{
	// 报文头长度, 6 字节 = 报文类型 2 字节 + 报文体长度 4 字节
	public const int headerLength = 6;

	// 缓冲
	public byte[] bytes { get; set; }

	// 已读长度
	public int readLength { get; set; }

	// 报文类型
	public short command { get; set; }

	// 报文体长度
	public int bodyLength { get; set; }

	public TcpBuffer ()
	{
		this.bytes = new byte[10240];
	}

	// 从 bytes 前 6 个字节解析出报文类型和报文体长度
	public void DecodeHeader ()
	{
		command = IPAddress.NetworkToHostOrder (System.BitConverter.ToInt16 (bytes, 0));
		bodyLength = IPAddress.NetworkToHostOrder (System.BitConverter.ToInt32 (bytes, 2));
	}

	// 重置
	public void Reset ()
	{
		readLength = 0;
		command = 0;
		bodyLength = 0;
	}
}