using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;

public class Mission {
	public class Action {
		public enum ActionType {
			ActionType_Idle = 10,
			ActionType_MoveAbsolute = 20,
			ActionType_MoveRelative = 21,
			ActionType_SingleAcquisition = 30,
			ActionType_StartAcquisition = 31,
			ActionType_StopAcquisition = 33
		}
		
		public int actionID { get; set; }
		public ActionType actionType { get; set; }
		public int actionParam1 { get; set; }
		public int actionParam2 { get; set; }
		public int actionParam3 { get; set; }
		public int actionParam4 { get; set; }

		public Action() {
			this.actionID = 0;
			this.actionType = ActionType.ActionType_Idle;
			this.actionParam1 = 0;
			this.actionParam2 = 0;
			this.actionParam3 = 0;
			this.actionParam4 = 0;
		}

		public Action(int actionID, ActionType actionType, int param1, int param2, int param3, int param4) {
			this.actionID = actionID;
			this.actionType = actionType;
			this.actionParam1 = param1;
			this.actionParam2 = param2;
			this.actionParam3 = param3;
			this.actionParam4 = param4;
		}

		public byte[] Serialize() {
			using (MemoryStream m = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(m)) {
					writer.Write(actionID);
					writer.Write((byte)actionType);
					writer.Write(actionParam1);
					writer.Write(actionParam2);
					writer.Write(actionParam3);
					writer.Write(actionParam4);
				}
				return m.ToArray();
			}
		}

		public static Action Desserialize(byte[] data) {
			Action result = new Action();
			using (MemoryStream m = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(m)) {
					result.actionID = reader.ReadInt32();
					result.actionType = (ActionType)reader.ReadByte();
					result.actionParam1 = reader.ReadInt32();
					result.actionParam2 = reader.ReadInt32();
					result.actionParam3 = reader.ReadInt32();
					result.actionParam4 = reader.ReadInt32();
				}
			}
			return result;
		}

		public static int Size() {
			return (4 + 1 + 4*4);
		}
	}

	public enum MissionStatus {
		MissionStatus_Unassigned = 0,
		MissionStatus_Assigned = 1,
		MissionStatus_Uploaded = 2,
		MissionStatus_Running = 3,
		MissionStatus_Completed = 4,
		MissionStatus_Error = -1
	}

	public int missionTarget { get; set; }
	public MissionStatus missionStatus { get; set; }
	List<Action> actionList = new List<Action>();

	public void AddAction(Action action) {
		actionList.Add(action);
	}

	public void AddActionAt(Action action, int index) {
		actionList.Insert(index, action);
	}

	public void AddActionAfter(Action action, int actionID) {
		int beforeIndex = actionList.FindIndex(a => a.actionID == actionID);
		actionList.Insert(beforeIndex + 1, action);
	}

	public void AddActionBefore(Action action, int actionID) {
		int beforeIndex = actionList.FindIndex(a => a.actionID == actionID);
		if(beforeIndex == 0) {
			return;
		}
		actionList.Insert(beforeIndex - 1, action);
	}

	public void RemoveAction(int actionID) {
		Action toRemove = actionList.Find(a => a.actionID == actionID);
		if(toRemove != null) {
			actionList.Remove(toRemove);
		}
	}

	public void RemoveActionAt(int index) {
		actionList.RemoveAt(index);
	}

	public int ActionCount() {
		return actionList.Count;
	}

	public Action GetAction(int actionID) {
		return actionList.Find(a => a.actionID == actionID);
	}

	public Action GetActionAt(int index) {
		return actionList[index];
	}

	public Action[] GetActionsAsArray() {
		return actionList.ToArray();
	}
}

public class MissionMessage {
	public enum MissionMessageType {
		MissionMessage_RequestMission = 1,
		MissionMessage_BaseInformation = 2,
		MissionMessage_Accept = 3,
		MissionMessage_Packet = 10,
		MissionMessage_PacketACK = 11,
		MissionMessage_PacketNACK = 12,
 		MissionMessage_PacketREQ = 13
	}

	public class MissionMessageACK {
		public int packetPart { get; set; }
		
		public byte[] Serialize() {
			using (MemoryStream m = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(m)) {
					writer.Write(packetPart);
				}
				return m.ToArray();
			}
		}

		public static MissionMessageACK Desserialize(byte[] data) {
			MissionMessageACK result = new MissionMessageACK();
			using (MemoryStream m = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(m)) {
					result.packetPart = reader.ReadInt32();
				}
			}
			return result;
		}
	}

	public class MissionMessageBaseInformation {
		public int missionSize { get; set; }
		
		public byte[] Serialize() {
			using (MemoryStream m = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(m)) {
					writer.Write(missionSize);
				}
				return m.ToArray();
			}
		}

		public static MissionMessageBaseInformation Desserialize(byte[] data) {
			MissionMessageBaseInformation result = new MissionMessageBaseInformation();
			using (MemoryStream m = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(m)) {
					result.missionSize = reader.ReadInt32();
				}
			}
			return result;
		}
	}

	public class MissionMessagePacket {
		public int part { get; set; }
		public int totalParts { get; set; }
		public int actionCount { get; set; }
		public Mission.Action[] actions { get; set; }

		public byte[] Serialize() {
			using (MemoryStream m = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(m)) {
					writer.Write(part);
					writer.Write(totalParts);
					writer.Write(actionCount);
					foreach (Mission.Action action in actions) {
						writer.Write(action.Serialize());
					}
				}
				return m.ToArray();
			}
		}

		public static MissionMessagePacket Desserialize(byte[] data) {
			MissionMessagePacket result = new MissionMessagePacket();
			using (MemoryStream m = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(m)) {
					result.part = reader.ReadInt32();
					result.totalParts = reader.ReadInt32();
					result.actionCount = reader.ReadInt32();
					result.actions = new Mission.Action[result.actionCount];
					for(int i = 0; i < result.actionCount; i++) {
						result.actions[i] = Mission.Action.Desserialize(reader.ReadBytes(Mission.Action.Size()));
					}
				}
			}
			return result;
		}
	}

	public MissionMessageType messageType { get; set; }
	public int systemID { get; set; }
	public int messageLength { get; set; }
	public byte[] message { get; set; }

	public byte[] Serialize() {
		using (MemoryStream m = new MemoryStream()) {
			using (BinaryWriter writer = new BinaryWriter(m)) {
				writer.Write((byte)messageType);
				writer.Write(systemID);
				writer.Write(messageLength);
				writer.Write(message);
			}
			return m.ToArray();
		}
	}

	public static MissionMessage Desserialize(byte[] data) {
		MissionMessage result = new MissionMessage();
		using (MemoryStream m = new MemoryStream(data)) {
			using (BinaryReader reader = new BinaryReader(m)) {
				result.messageType = (MissionMessageType)reader.ReadByte();
				result.systemID = reader.ReadInt32();
				result.messageLength = reader.ReadInt32();
				result.message = reader.ReadBytes(result.messageLength);
			}
		}
		return result;
	}
}

public class MissionServer : MonoBehaviour {
	//Server Thread
	Thread serverThread;
	public int serverPort = 25002;
	UdpClient udpListener;
	bool serverRunning;

	//Server clients variables
	public IPAddress lastClientAddress;
	public int lastClientPort;

	//Mission stuff
	Mission mission;

	// Start is called before the first frame update
	void Start() {
		//Create and start up a server thread, outside Unity main thread
		ThreadStart thread = new ThreadStart(ServerThreadStart);
		serverThread = new Thread(thread);
		serverThread.Start();

		mission = new Mission();
		mission.missionTarget = 10;
		mission.missionStatus = Mission.MissionStatus.MissionStatus_Assigned;
		mission.AddAction(new Mission.Action(0, Mission.Action.ActionType.ActionType_SingleAcquisition, 1, 0, 0, 0));
		mission.AddAction(new Mission.Action(1, Mission.Action.ActionType.ActionType_SingleAcquisition, 1, 0, 0, 0));
		mission.AddAction(new Mission.Action(2, Mission.Action.ActionType.ActionType_SingleAcquisition, 1, 0, 0, 0));
		mission.AddAction(new Mission.Action(3, Mission.Action.ActionType.ActionType_SingleAcquisition, 1, 0, 0, 0));
		mission.AddAction(new Mission.Action(4, Mission.Action.ActionType.ActionType_SingleAcquisition, 1, 0, 0, 0));
	}

	void OnDestroy() {
		serverRunning = false;

		Thread.Sleep(100);
	}

	void ServerThreadStart() {
		udpListener = new UdpClient(serverPort);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, serverPort);

		//Mission tranfer status
		bool transferStarted = false;
		bool currentPartACKed = false;
		int currentPart = 0;
		int partTotal = 0;

		serverRunning = true;
		while (serverRunning) {
			Debug.Log("Mission Server: Waiting for broadcast");
			byte[] rxBytes = udpListener.Receive(ref groupEP);
			//Debug.Log($"Mission Server: Received inquery from {groupEP} :" + System.Text.Encoding.ASCII.GetString(rxBytes));
			lastClientAddress = groupEP.Address;
			lastClientPort = groupEP.Port;

			MissionMessage message = MissionMessage.Desserialize(rxBytes);
			Debug.Log("Mission Server: From: " + message.systemID.ToString() + " Type: " + message.messageType.ToString() + " Len: " + message.messageLength.ToString());

			switch(message.messageType) {
				case MissionMessage.MissionMessageType.MissionMessage_RequestMission: {
					//Is mission request message, return information about available mission
					MissionMessage infoMessage = new MissionMessage();
					infoMessage.messageType = MissionMessage.MissionMessageType.MissionMessage_BaseInformation;
					infoMessage.systemID = message.systemID;

					MissionMessage.MissionMessageBaseInformation baseInfo = new MissionMessage.MissionMessageBaseInformation();
					baseInfo.missionSize = 0;
					if (message.systemID == mission.missionTarget) {
						//Have mission for target
						baseInfo.missionSize = mission.ActionCount();
					}

					infoMessage.message = baseInfo.Serialize();
					infoMessage.messageLength = infoMessage.message.Length;

					//Send message back
					byte[] txBytes = infoMessage.Serialize();
					udpListener.Send(txBytes, txBytes.Length, groupEP);
					break;
				}
				case MissionMessage.MissionMessageType.MissionMessage_Accept: {
					//Mission accept message, start transmitting mission actions
					MissionMessage actionPacketMessage = new MissionMessage();
					actionPacketMessage.messageType = MissionMessage.MissionMessageType.MissionMessage_Packet;
					actionPacketMessage.systemID = message.systemID;

					if (message.systemID != mission.missionTarget) {
						//No have mission for it, do nothing
						break;
					}

					//Started Mission transfer
					Debug.Log("Mission Server: Mission accepted, start transfer");
					transferStarted = true;
					currentPart = 0;
					currentPartACKed = false;

					MissionMessage.MissionMessagePacket packet = new MissionMessage.MissionMessagePacket();
					packet.part = 0;
					packet.totalParts = 1;
					packet.actionCount = mission.ActionCount();
					packet.actions = mission.GetActionsAsArray();

					actionPacketMessage.message = packet.Serialize();
					actionPacketMessage.messageLength = actionPacketMessage.message.Length;

					//Send message back
					byte[] txBytes = actionPacketMessage.Serialize();
					udpListener.Send(txBytes, txBytes.Length, groupEP);
					Debug.Log("Mission Server: Mission part " + (packet.part + 1).ToString() + " of " + packet.totalParts.ToString());
					break;
				}
				case MissionMessage.MissionMessageType.MissionMessage_PacketACK: {
					//Packet acknowledged
					MissionMessage.MissionMessageACK ackMessage = MissionMessage.MissionMessageACK.Desserialize(message.message);
					Debug.Log("Mission Server: Packet ACK: " + ackMessage.packetPart.ToString());

					if(ackMessage.packetPart == currentPart) {
						//ACK for current packet
						currentPartACKed = true;
						currentPart += 1;
					}

					if(currentPart == partTotal) {
						//Transfer finished
						transferStarted = false;
						currentPart = 0;
						Debug.Log("Mission Server: Transfer complete");
					}

					break;
				}
				case MissionMessage.MissionMessageType.MissionMessage_PacketNACK: {
					//Packet NOT acknowledged
					MissionMessage.MissionMessageACK nackMessage = MissionMessage.MissionMessageACK.Desserialize(message.message);
					Debug.Log("Mission Server: Packet NACK: " + nackMessage.packetPart.ToString());
					break;
				}
				case MissionMessage.MissionMessageType.MissionMessage_PacketREQ: {
					//Request for specific packet
					MissionMessage.MissionMessageACK reqMessage = MissionMessage.MissionMessageACK.Desserialize(message.message);
					Debug.Log("Mission Server: Packet REQ: " + reqMessage.packetPart.ToString());
					break;
				}
			}
		}

		udpListener.Close();
	}
}
