using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// tcp 客户端
public class TcpUtil
{
	private Dictionary<TcpManager.EventType, Action> eventHandlers = new Dictionary<TcpManager.EventType, Action> ();
	private Dictionary<short, Action<string>> packetHandlers = new Dictionary<short, Action<string>> ();

	private String aesKey;
	private TcpManager tcpManager;
	private Socket socket;
	private TcpBuffer tcpBuffer;

	public TcpUtil ()
	{
	}

	#region 注册处理器

	// 注册事件处理器
	public void AddEventHandler (TcpManager.EventType e, Action handler)
	{
		eventHandlers.Add (e, handler);
	}

	// 注册报文处理器
	public void AddPacketHanlder (short command, Action<string> handler)
	{
		packetHandlers.Add (command, handler);
	}

	#endregion

	#region 创建连接

	// 创建连接
	public void Connect (string ip, int port, string aesKey)
	{
		this.aesKey = aesKey;
		tcpManager = new TcpManager ();
		IPEndPoint endPoint = new IPEndPoint (IPAddress.Parse (ip), port);
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.SendTimeout = 10000;
		socket.ReceiveTimeout = 10000;
		socket.BeginConnect (endPoint, new AsyncCallback (ConnectCallback), socket);
	}

	// 连接回调
	private void ConnectCallback (IAsyncResult ar)
	{
		try {
			socket.EndConnect (ar);

			if (!socket.Connected) {
				tcpManager.AddEvent (TcpManager.EventType.ConnectFaild);
				return;
			}

			tcpManager.AddEvent (TcpManager.EventType.ConnectSuccess);

			Receive ();
		} catch (Exception) {
			tcpManager.AddEvent (TcpManager.EventType.ConnectFaild);
		}
	}

	#endregion

	#region 处理事件和报文

	private const double heatbeatInterval = 5.0;
	private double heartbeatTimer = 0;
    bool isBreak;
	public void Update (float delta)
	{
		if (socket == null || !socket.Connected) {
            if (isBreak)
            {
                isBreak = false;
            }
            return;
		}

		// 心跳包
		heartbeatTimer += delta;
		if (heartbeatTimer > heatbeatInterval) {
			heartbeatTimer -= heatbeatInterval;
			SendPacket ((short)1, null);
        }

		// 发送报文
		for (TcpPacket packet = tcpManager.GetOutPacket (); packet != null;) {
			Send (packet);            
            packet = tcpManager.GetOutPacket ();
        }

		// 处理接收的报文
		for (TcpPacket packet = tcpManager.GetInPacket (); packet != null;) {
			Action<string> handler = null;
			if (packetHandlers.TryGetValue (packet.command, out handler)) {
				if (handler != null) {
					handler (packet.GetString (aesKey));
				}
			}            
            packet = tcpManager.GetInPacket ();
        }

		// 处理事件
		for (TcpManager.EventType e = tcpManager.GetEvent (); e != TcpManager.EventType.None;) {
			Action handler = null;
			if (eventHandlers.TryGetValue (e, out handler)) {
				if (handler != null) {
					handler ();
				}
			}
            e = tcpManager.GetEvent ();
            isBreak = true;
        }
	}

	#endregion

	#region 发送报文

	// 发送报文
	public void SendPacket (short command, String body)
	{
		tcpManager.AddOutPacket (new TcpPacket (command, body, aesKey));
	}

	private void Send (TcpPacket packet)
	{
		try {
			byte[] buffer = new byte[TcpBuffer.headerLength + packet.bytes.Length];
			System.BitConverter.GetBytes ((short)IPAddress.NetworkToHostOrder (packet.command)).CopyTo (buffer, 0);
			System.BitConverter.GetBytes (IPAddress.NetworkToHostOrder (packet.bytes.Length)).CopyTo (buffer, 2);
			Buffer.BlockCopy (packet.bytes, 0, buffer, TcpBuffer.headerLength, packet.bytes.Length);
			socket.BeginSend (buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback (SendCallBack), socket);
		} catch (Exception) {
			tcpManager.AddEvent (TcpManager.EventType.SendFailed);
		}
	}

	private void SendCallBack (IAsyncResult ar)
	{
		try {
			int byteSend = socket.EndSend (ar);
			if (byteSend < 1) {
				tcpManager.AddEvent (TcpManager.EventType.ConnectLost);	
			}
		} catch (Exception) {
			tcpManager.AddEvent (TcpManager.EventType.SendFailed);
		}
	}

	#endregion


	#region 接收报文

	// 接收报文
	private void Receive ()
	{
		tcpBuffer = new TcpBuffer ();

		try {
			socket.BeginReceive (tcpBuffer.bytes, 0, TcpBuffer.headerLength, SocketFlags.None, new AsyncCallback (ReceiveHeader), socket);
		} catch (Exception e) {
            Debug.LogError(e);
            tcpManager.AddEvent (TcpManager.EventType.ReceiveFailed);
		}
	}

	// 报文头回调
	private void ReceiveHeader (IAsyncResult ar)
	{
		try {
			int read = socket.EndReceive (ar);
			if (read < 1) {
				tcpManager.AddEvent (TcpManager.EventType.ConnectLost);
				return;
			}

			tcpBuffer.readLength += read;
			if (tcpBuffer.readLength < TcpBuffer.headerLength) {
				socket.BeginReceive (tcpBuffer.bytes, tcpBuffer.readLength, TcpBuffer.headerLength - tcpBuffer.readLength, SocketFlags.None, new AsyncCallback (ReceiveHeader), socket);
				return;
			}

			tcpBuffer.DecodeHeader ();
			tcpBuffer.readLength = 0;
			socket.BeginReceive (tcpBuffer.bytes, TcpBuffer.headerLength, tcpBuffer.bodyLength, SocketFlags.None, new AsyncCallback (ReceiveBody), socket);

		} catch (Exception e) {
            Debug.LogError(e);
            tcpManager.AddEvent (TcpManager.EventType.ReceiveFailed);
		}
	}

	// 报文体回调
	private void ReceiveBody (IAsyncResult ar)
	{
		try {
            if (tcpBuffer.bodyLength != 0)
            {
                int read = socket.EndReceive(ar);
                if (read < 1)
                {
                    tcpManager.AddEvent(TcpManager.EventType.ConnectLost);
                    return;
                }

                tcpBuffer.readLength += read;
                if (tcpBuffer.readLength < tcpBuffer.bodyLength)
                {
                    socket.BeginReceive(tcpBuffer.bytes, TcpBuffer.headerLength + tcpBuffer.readLength, tcpBuffer.bodyLength - tcpBuffer.readLength, SocketFlags.None, new AsyncCallback(ReceiveBody), socket);
                    return;
                }
            }

			tcpManager.AddInPacket (new TcpPacket (tcpBuffer));

			tcpBuffer.Reset ();
			socket.BeginReceive (tcpBuffer.bytes, 0, TcpBuffer.headerLength, SocketFlags.None, new AsyncCallback (ReceiveHeader), socket);

		} catch (Exception e) {
            Debug.LogError(e);
			tcpManager.AddEvent (TcpManager.EventType.ReceiveFailed);
		}
	}

	#endregion


	#region 断开连接

	// 断开连接
	public void Close ()
	{
		if (socket == null || !socket.Connected) {
			return;
		}

		socket.BeginDisconnect (false, new AsyncCallback (DisconnectCallBack), socket);
	}

	// 断开回调
	private void DisconnectCallBack (IAsyncResult ar)
	{
		socket.EndDisconnect (ar);
		socket.Close ();
		socket = null;
	}

	#endregion
}
