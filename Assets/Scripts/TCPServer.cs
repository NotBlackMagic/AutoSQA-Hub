using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour {
	//Server Thread
	Thread serverThread;
	public int serverPort = 25001;
	TcpListener tcpServer;
	bool serverRunning;

    // Start is called before the first frame update
    void Start() {
		//Create and start up a server thread, outside Unity main thread
		ThreadStart thread = new ThreadStart(ServerThreadStart);
		serverThread = new Thread(thread);
		serverThread.Start();
	}

    // Update is called once per frame
    void Update() {
        
    }

	private void OnDestroy() {
		serverRunning = false;

		Thread.Sleep(100);
	}

	void ServerThreadStart() {
		//Create server
		tcpServer = new TcpListener(IPAddress.Any, serverPort);
		tcpServer.Start();

		Debug.Log("TCP Server: Started on Port " + serverPort.ToString());

		byte[] buffer = new byte[1024];

		serverRunning = true;
		while (serverRunning) {
			Debug.Log("TCP Server: Waiting for connection...");

			//Wait for client to connect
			using TcpClient tcpClient = tcpServer.AcceptTcpClient();
			Debug.Log("TCP Server: Client connected");

			//Read data from network stream
			NetworkStream networkStream = tcpClient.GetStream();

			int bytesRead = 0;
			
			while((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) != 0) {
				//Decode data
				string dataString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

				//Check if valid string and output
				if (dataString != null && dataString != "") {
					Debug.Log("TCP Server: Received: " + dataString);
					networkStream.Write(buffer, 0, bytesRead);
				}
			}
		}
	}
}
