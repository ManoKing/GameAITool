using System;
using System.Collections;
using System.Collections.Generic;

// tcp 事件和报文管理器
public class TcpManager
{
	private Queue events = new Queue ();
	private Queue inPackets = new Queue ();
	private Queue outPackets = new Queue ();

	public enum EventType
	{
		None = 0,
		ConnectFaild,
		ConnectSuccess,
		ConnectLost,
		ReceiveFailed,
		SendFailed
	}

	public TcpManager ()
	{
	}

	public void AddEvent (EventType e)
	{
		lock (events) {
			events.Enqueue (e);
		}
	}

	public EventType GetEvent ()
	{
		lock (events) {
			if (events.Count == 0) {
				return EventType.None;
			}

			return (EventType)events.Dequeue ();
		}
	}

	public void AddInPacket (TcpPacket packet)
	{
		lock (inPackets) {
			inPackets.Enqueue (packet);
		}
	}

	public TcpPacket GetInPacket ()
	{
		lock (inPackets) {
			if (inPackets.Count == 0) {
				return null;
			}

			return (TcpPacket)inPackets.Dequeue ();
		}
	}

	public void AddOutPacket (TcpPacket packet)
	{
		lock (outPackets) {
			outPackets.Enqueue (packet);
		}
	}

	public TcpPacket GetOutPacket ()
	{
		lock (outPackets) {
			if (outPackets.Count == 0) {
				return null;
			}

			return (TcpPacket)outPackets.Dequeue ();
		}
	}
}
