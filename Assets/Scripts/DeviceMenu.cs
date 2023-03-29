using System.IO.Ports;
using UnityEngine;
using UnityEngine.UIElements;

public class DeviceMenu : MonoBehaviour {
	//Root UI element
	VisualElement rootUI;

	//MAVLink Connection UI Elements
	DropdownField mavlinkPortDropdown;
	DropdownField mavlinkBaudrateDropdown;
	Button mavlinkConenctButton;

	//Device Window UI Elements
	Label mavlinkDeviceNameLabel;
	Label mavlinkConnectionStatusLabel;
	Label mavlinkVersionLabel;
	Label missionACQServerConnectionStatusLabel;
	Label missionServerConnectionIPLabel;
	Label deviceStatusLabel;
	Label deviceFlightModeLabel;
	Label deviceBatteryLabel;
	Button deviceArmButton;

	//Other Unity Objects
	public GameObject mavlinkHubHolder;
	MAVLinkHub mavlinkHub;
	public GameObject acquistionServerHolder;
	AcquisitionServer acqServer;

    // Start is called before the first frame update
    void OnEnable() {
		rootUI = GetComponent<UIDocument>().rootVisualElement;
		mavlinkHub = mavlinkHubHolder.GetComponent<MAVLinkHub>();
		acqServer = acquistionServerHolder.GetComponent<AcquisitionServer>();

		//Get MAVLink Connection UI Elements
		mavlinkPortDropdown = rootUI.Q<DropdownField>("MAVLinkMenuPortDropdown");
		mavlinkBaudrateDropdown = rootUI.Q<DropdownField>("MAVLinkMenuBaudDropdown");
		mavlinkConenctButton = rootUI.Q<Button>("MAVLinkMenuConnectButton");
		//Setup callbacks for buttons
		mavlinkConenctButton.clicked += () => MAVLinkConnectClicked();
		//Get serial ports and add to port dropdown
		string[] ports = SerialPort.GetPortNames();
		foreach(string port in ports) {
			mavlinkPortDropdown.choices.Add(port);
		}
		mavlinkPortDropdown.choices.RemoveAt(0);
		mavlinkPortDropdown.index = 0;

		//Get Device Window UI Elements
		mavlinkDeviceNameLabel = rootUI.Q<Label>("MAVLinkDeviceNameLabel");
		mavlinkConnectionStatusLabel = rootUI.Q<Label>("MAVLinkDeviceConnectionLabel");
		mavlinkVersionLabel = rootUI.Q<Label>("MAVLinkVersionLabel");
		missionACQServerConnectionStatusLabel = rootUI.Q<Label>("MAVLinkDeviceACQServerLabel");
		missionServerConnectionIPLabel = rootUI.Q<Label>("MAVLinkDeviceIPLabel");
		deviceStatusLabel = rootUI.Q<Label>("MAVLinkDeviceStatusLabel"); 
		deviceFlightModeLabel = rootUI.Q<Label>("MAVLinkDeviceFlightModeLabel");
		deviceBatteryLabel = rootUI.Q<Label>("MAVLinkDeviceBatteryLabel");
		deviceArmButton = rootUI.Q<Button>("MAVLinkDeviceArmButton");
		//Setup callbacks for buttons
		deviceArmButton.clicked += () => DeviceArmClicked();
	}

	private void Update() {
		//MAVLink realted variabls/values update
		mavlinkDeviceNameLabel.text = "MR-Buggy3" + " ID: " + mavlinkHub.systemID;
		mavlinkVersionLabel.text = "V" + mavlinkHub.mavlinkVersion.ToString();
		deviceBatteryLabel.text = mavlinkHub.batteryRemaining.ToString("F0") + "%" + "(" + mavlinkHub.batteryVoltage.ToString("F2") + "V)";
		if(mavlinkHub.armed == true) {
			deviceStatusLabel.text = "Armed (Idle)";
		}
		else {
			deviceStatusLabel.text = "Un-Armed";
		}
		if(mavlinkHub.flightMode == 0) {
			deviceFlightModeLabel.text = "Manual";
		}
		else {
			deviceFlightModeLabel.text = "Mission";
		}
		//Acqusition Server related variables/values update
		if(acqServer.lastClientAddress != null) {
			missionACQServerConnectionStatusLabel.text = "Connected";
			missionServerConnectionIPLabel.text = acqServer.lastClientAddress.ToString() + ":" + acqServer.lastClientPort.ToString();
		}
	}

	void MAVLinkConnectClicked() {
		Debug.Log("MAVLink Connect Button Clicked!");
		string port = mavlinkPortDropdown.text;
		int baudrate = int.Parse(mavlinkBaudrateDropdown.text);
		Debug.Log("Port: " + port + " Baudrate: " + baudrate.ToString());
		bool connected = mavlinkHub.MAVLinkConnect(port, baudrate);
		if (connected == true) {
			mavlinkConnectionStatusLabel.text = "Connected";
		}
	}

	void DeviceArmClicked() {
		Debug.Log("Device Arm Button Clicked!");
		mavlinkHub.MAVLinkSetFlightMode(0);
		mavlinkHub.MAVlinkArm(true, true);
	}
}
