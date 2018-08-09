using System;

// tcp 数据包
public class TcpPacket
{
	// 报文类型
	public short command { get; set; }

	// 报文体长度
	public byte[] bytes { get; set; }

	// 加密, 用于发包
	public TcpPacket (short command, string body, string aesKey)
	{
		this.command = command;

		if (body == null) {
			this.bytes = new byte[0];
		} else {
			if (RequireEncrypt ()) {
				body = AesUtil.Encrypt (aesKey, body);
			}

			this.bytes = System.Text.Encoding.UTF8.GetBytes (body);
		}
	}

	// 创建一个拷贝
	public TcpPacket (TcpBuffer buffer)
	{
		this.command = buffer.command;
		this.bytes = new byte[buffer.bodyLength];
		Buffer.BlockCopy (buffer.bytes, TcpBuffer.headerLength, this.bytes, 0, buffer.bodyLength);
	}

	// 报文字节数组转为字符串. 可能需要解密, 用于收包
	public String GetString (string aesKey)
	{
		string result = System.Text.Encoding.UTF8.GetString (bytes);

		if (!RequireEncrypt ()) {
			return result;
		}

		return AesUtil.Decrypt (aesKey, result);
	}

	// 是否需要加解密
	private bool RequireEncrypt ()
	{
		// 心跳, 进入房间, 会话超时不需要加密, 其他都需要
		return command != 1 && command != 2 && command != 1000 && command != 101;
	}
}
