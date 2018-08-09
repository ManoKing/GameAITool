using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Newtonsoft.Json;

public class TcpUtilDemo : MonoBehaviour
{
	TcpUtil tcpUtil;

	const short commandHeartBeat = (short)1;
	const short commandEnterRoom = (short)2;
	const short commandLeaveRoom = (short)3;
	const short commandBet = (short)4;
	// TODO more commands

	// Use this for initialization
	void Start ()
	{
		tcpUtil = new TcpUtil ();
        
		// event handlers
		tcpUtil.AddEventHandler (TcpManager.EventType.ConnectFaild, () => {
			Debug.Log ("connect failed");
			// TODO 重连?
		});

		tcpUtil.AddEventHandler (TcpManager.EventType.ConnectLost, () => {
			Debug.Log ("connect lost");
			// TODO 重连?
		});

		tcpUtil.AddEventHandler (TcpManager.EventType.ConnectSuccess, () => {
			Debug.Log ("connect success");
			EnterRoom ();
		});

		tcpUtil.AddEventHandler (TcpManager.EventType.ReceiveFailed, () => {
			Debug.Log ("receive failed");
			// TODO 重连?
		});

		tcpUtil.AddEventHandler (TcpManager.EventType.SendFailed, () => {
			Debug.Log ("send failed");
			// TODO 重连?
		});

		// packet handlers
		tcpUtil.AddPacketHanlder (commandHeartBeat, (body) => {
			Debug.Log ("command- " + commandHeartBeat + ": " + body);
            
		});

		tcpUtil.AddPacketHanlder (commandEnterRoom, (body) => {
			Debug.Log ("command- " + commandEnterRoom + ": " + body);
			// 模拟下注
			Bet ();
		});

		tcpUtil.AddPacketHanlder (commandBet, (body) => {
			Debug.Log ("command- " + commandBet + ": " + body);
			// 模拟退出房间
			// LeaveRoom ();
		});

		tcpUtil.AddPacketHanlder (commandLeaveRoom, (body) => {
			Debug.Log ("command- " + commandLeaveRoom + ": " + body);
		});

		// connect
		tcpUtil.Connect ("192.168.1.27", 5000, "1234567890abcdef");  // aes key 从 http 登录的返回获取
	}

	void EnterRoom ()
	{
		var request = new Dictionary<string, object> ();
		request.Add ("memberId", "test-memberId"); // 从 http 登录的返回获取
		request.Add ("roomId", "test-roomId"); 

        request.Add ("encryptedRoomId", AesUtil.Encrypt ("1234567890abcdef", "test-roomId")); // aes key 从 http 登录的返回获取
        // TODO
        //tcpUtil.SendPacket (commandEnterRoom, JsonConvert.SerializeObject (request));
    }

    void Bet ()
	{
		var request = new Dictionary<string, object> ();
		request.Add ("stake", 100);
		request.Add ("selectionType", "PLACE_1");
		// TODO more parameters
		//tcpUtil.SendPacket (commandBet, JsonConvert.SerializeObject (request));
	}

	void LeaveRoom ()
	{
		tcpUtil.SendPacket (commandLeaveRoom, null);
	}

	// Update is called once per frame
	void Update ()
	{
		tcpUtil.Update (Time.deltaTime);
	}

	void OnDestroy ()
	{
		tcpUtil.Close ();
	}
}
