using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class AcquisitionServer : MonoBehaviour {
	//Server Thread
	Thread serverThread;
	public int serverPort = 25001;
	UdpClient udpListener;
	bool serverRunning;

	//Server clients variables
	public IPAddress lastClientAddress;
	public int lastClientPort;

	//Server Thread to Unity Thread communiaction variables
	private object queueLock = new object();
	private Queue<byte[]> taskQueue = new Queue<byte[]>();

	public MapLayerDrawer mapLayerDrawer;

	// Start is called before the first frame update
	void Start() {
		//Create and start up a server thread, outside Unity main thread
		ThreadStart thread = new ThreadStart(ServerThreadStart);
		serverThread = new Thread(thread);
		serverThread.Start();
	}

	void OnDestroy() {
		serverRunning = false;

		Thread.Sleep(100);
	}

	struct SensorData {
		public float timestamp;
		public float localCoordinateX;
		public float localCoordinateY;
		public float localCoordinateZ;
		public int sensorDataTypes;
		public float[] sensorData;

		public SensorData(int sensorCount) {
			timestamp = 0;
			localCoordinateX = 0;
			localCoordinateY = 0;
			localCoordinateZ = 0;
			sensorDataTypes = 0;
			sensorData = new float[sensorCount];
		}
	}

	SensorData FromArray(byte[] bytes) {
		SensorData sensorData = new SensorData(9);

		BinaryReader reader = new BinaryReader(new MemoryStream(bytes));

		sensorData.timestamp = reader.ReadSingle();
		sensorData.localCoordinateX = reader.ReadSingle();
		sensorData.localCoordinateY = reader.ReadSingle();
		sensorData.localCoordinateZ = reader.ReadSingle();
		sensorData.sensorDataTypes = reader.ReadInt32();

		for (int i = 0; i < sensorData.sensorData.Length; i++) {
			int hasSensor = (sensorData.sensorDataTypes >> i) & 0x01;
			if (hasSensor == 1) {
				sensorData.sensorData[i] = reader.ReadSingle();
			}
		}

		return sensorData;
	}

	// Update is called once per frame
	void Update() {
		lock (queueLock) {
			if (taskQueue.Count > 0) {
				byte[] msg = taskQueue.Dequeue();
				//Debug.Log($" {Encoding.ASCII.GetString(msg, 0, msg.Length)}");

				SensorData sensorData = FromArray(msg);

				if((sensorData.sensorDataTypes & 0x01) == 0x01) {
					Debug.Log("New Temperature sample: " + sensorData.localCoordinateX.ToString("F2") + ";" + sensorData.localCoordinateY.ToString("F2") + " " + sensorData.sensorData[0].ToString("F2") + "°C");
				}
				if ((sensorData.sensorDataTypes & 0x02) == 0x02) {
					Debug.Log("New Humidity sample: " + sensorData.localCoordinateX.ToString("F2") + ";" + sensorData.localCoordinateY.ToString("F2") + " " + sensorData.sensorData[1].ToString("F2") + "%");
				}
				if ((sensorData.sensorDataTypes & 0x04) == 0x04) {
					Debug.Log("New Pressure sample: " + sensorData.localCoordinateX.ToString("F2") + ";" + sensorData.localCoordinateY.ToString("F2") + " " + sensorData.sensorData[2].ToString("F2") + "hPa");
				}

				for (int s = 0; s < sensorData.sensorData.Length; s++) {
					int enabled = ((sensorData.sensorDataTypes >> s) & 0x01);
					if (enabled == 0x01) {
						mapLayerDrawer.UpdateMapValues((MapLayerDrawer.MapLayers)s, new Vector2(sensorData.localCoordinateX, sensorData.localCoordinateY), sensorData.sensorData[s]);
					}
				}
			}
		}
	}

	void ServerThreadStart() {
		udpListener = new UdpClient(serverPort);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, serverPort);

		serverRunning = true;
		while (serverRunning) {
			Debug.Log("Acquisition Server: Waiting for broadcast");
			byte[] bytes = udpListener.Receive(ref groupEP);
			Debug.Log($"Acquisition Server: Received data packet from {groupEP}");
			lastClientAddress = groupEP.Address;
			lastClientPort = groupEP.Port;

			//Send message back
			//udpListener.Send(bytes, bytes.Length, groupEP);

			//Add message to queue, to be read in the Update() method (Unity thread)
			lock (queueLock) {
				if (taskQueue.Count < 100) {
					taskQueue.Enqueue(bytes);
				}
			}
		}

		udpListener.Close();
	}
}
