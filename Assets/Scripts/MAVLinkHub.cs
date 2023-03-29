using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAVLinkHub : MonoBehaviour {
	//Serial Port Variables
	public string port = "COM1";
	public int baudrate = 57600;
	public bool rxUSBThreadRun = true;
	Thread rxUSBThread;
	SerialPort serialPort = new SerialPort();

	//MAVLink messages
	MAVLink.MavlinkParse mavlinkParser;

	//Message content
	public int systemID = 0;
	public int componentID = 0;
	public Vector3 rotation;
	public Vector3 position;
	public bool armed;
	public int flightMode;
	public float batteryVoltage;
	public float batteryRemaining;
	public int mavlinkVersion;

	// Start is called before the first frame update
	void Start() {
		mavlinkParser = new MAVLink.MavlinkParse(false);
	}

	// Update is called once per frame
	void Update() {

	}

	private void OnDestroy() {
		rxUSBThreadRun = false;

		Thread.Sleep(100);

		serialPort.Close();
	}

	public bool MAVLinkConnect(string port, int baudrate) {
		if (serialPort.IsOpen == false) {
			try {
				serialPort.PortName = port;
				serialPort.BaudRate = baudrate;
				serialPort.DataBits = 8;
				serialPort.StopBits = StopBits.One;
				serialPort.Parity = Parity.None;
				serialPort.NewLine = "\n";
				serialPort.Open();

				//USBWrite(Opcodes.connect, null);

				//usbRXStopwatch.Start();

				Debug.Log("MAVLink Serial Connected");

				this.port = port;
				this.baudrate = baudrate;

				rxUSBThreadRun = true;
				rxUSBThread = new Thread(USBRXThread);
				rxUSBThread.Start();

				return true;
			}
			catch (System.Exception ex) {
				Debug.LogError("Serial Connect Error" + ex.Message);
				return false;
			}
		}
		else {
			return true;
		}
	}

	public void MAVlinkArm(bool arm, bool forced) {
		MAVLink.mavlink_command_long_t cmd = new MAVLink.mavlink_command_long_t();
		cmd.target_system = (byte)systemID;
		cmd.target_component = (byte)componentID;
		cmd.command = (ushort)MAVLink.MAV_CMD.COMPONENT_ARM_DISARM;
		if(arm) {
			//Arm command
			cmd.param1 = 1;
		}
		else {
			//Dissarm command
			cmd.param1 = 0;
		}
		if(forced) {
			//Force arming or disarming
			cmd.param2 = 21196;
		}
		else {
			//Arm-disarm unless prevented by safety checks
			cmd.param2 = 0;
		}
		byte[] data = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, cmd);
		//byte[] data = mavlinkParser.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, cmd);
		serialPort.Write(data, 0, data.Length);
	}

	public void MAVLinkSetFlightMode(float mode) {
		MAVLink.mavlink_command_long_t cmd = new MAVLink.mavlink_command_long_t();
		cmd.target_system = (byte)systemID;
		cmd.target_component = (byte)componentID;
		cmd.command = (ushort)MAVLink.MAV_CMD.DO_SET_MODE;
		if(mode == 0) {
			//Manual Mode
			cmd.param1 = 217;   //Base Mode
			cmd.param2 = 1;     //Main Mode
			cmd.param3 = 0;     //Sub Mode
		}
		else {
			//Mission Mode
			cmd.param1 = 157;   //Base Mode
			cmd.param2 = 4;     //Main Mode
			cmd.param3 = 4;     //Sub Mode
		}

		byte[] data = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, cmd);
		//byte[] data = mavlinkParser.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, cmd);
		serialPort.Write(data, 0, data.Length);
	}

	private void USBRXThread() {
		MAVLink.MAVLinkMessage message;
		while (rxUSBThreadRun && serialPort.IsOpen) {
			message = mavlinkParser.ReadPacket(serialPort.BaseStream);
			if(message != null) {
				switch(message.msgid) {
					case 0:
						//Heartbeat Message
						MAVLink.mavlink_heartbeat_t heartbeat = (MAVLink.mavlink_heartbeat_t)message.data;
						//Get target ID
						systemID = message.sysid;
						componentID = message.compid;
						//Check for Armed status
						if ((heartbeat.base_mode & (byte)MAVLink.MAV_MODE_FLAG.SAFETY_ARMED) == (byte)MAVLink.MAV_MODE_FLAG.SAFETY_ARMED) {
							//Armed flag set
							armed = true;
						}
						else {
							//Armed flag not set
							armed = false;
						}
						//Check for MAVLink protocol verison
						if (message.ismavlink2 == true) {
							mavlinkVersion = 2;
						}
						else {
							mavlinkVersion = 1;
						}
						//Check for flight mode
						if ((heartbeat.base_mode & (byte)MAVLink.MAV_MODE.MANUAL_DISARMED) == (byte)MAVLink.MAV_MODE.MANUAL_DISARMED) {
							//Manual mode
							flightMode = 0;
						}
						else if((heartbeat.base_mode & (byte)28) == (byte)28) {
							//Mission or Hold mode
							flightMode = 1;
						}
						else {
							flightMode = 1;
						}
						break;
					case 1:
						//System Status Message
						MAVLink.mavlink_sys_status_t systemStatus = (MAVLink.mavlink_sys_status_t)message.data;
						batteryVoltage = systemStatus.voltage_battery * 0.001f;
						batteryRemaining = systemStatus.battery_remaining;
						break;
					case 4:
						//Ping Message
						MAVLink.mavlink_ping_t ping = (MAVLink.mavlink_ping_t)message.data;
						break;
					case 8:
						//Link Node Status Message
						MAVLink.mavlink_link_node_status_t linkNodeStatus = (MAVLink.mavlink_link_node_status_t)message.data;
						break;
					case 24:
						//GPS Raw Message
						MAVLink.mavlink_gps_raw_int_t gpsRaw = (MAVLink.mavlink_gps_raw_int_t)message.data;
						break;
					case 29:
						//Scalled Pressure Mesage
						MAVLink.mavlink_scaled_pressure_t scalledPressure = (MAVLink.mavlink_scaled_pressure_t)message.data;
						break;
					case 30:
						//Attiude Message
						MAVLink.mavlink_attitude_t attitude = (MAVLink.mavlink_attitude_t)message.data;
						//Convert to degrees and to Unity positionn reference
						rotation.x = attitude.pitch * Mathf.Rad2Deg;
						rotation.z = attitude.yaw * Mathf.Rad2Deg;
						rotation.z = attitude.roll * Mathf.Rad2Deg;
						//Debug.Log("MAVLink Message: Attitude: TimeBoot = " + attitude.time_boot_ms.ToString());
						//Debug.Log("Roll: " + attitude.roll.ToString());
						//Debug.Log("Pitch: " + attitude.pitch.ToString());
						//Debug.Log("Yaw: " + attitude.yaw.ToString());
						break;
					case 31:
						//Attitude Quaternion Message
						MAVLink.mavlink_attitude_quaternion_t attitudeQuaternion = (MAVLink.mavlink_attitude_quaternion_t)message.data;
						break;
					case 32:
						//Local Position Message
						MAVLink.mavlink_local_position_ned_t localPosition = (MAVLink.mavlink_local_position_ned_t)message.data;
						position.x = localPosition.y;
						position.y = localPosition.z;
						position.z = localPosition.x;
						break;
					case 36:
						//Servo Output Raw Message
						MAVLink.mavlink_servo_output_raw_t servoOutputRaw = (MAVLink.mavlink_servo_output_raw_t)message.data;
						break;
					case 74:
						//VFR Hut Message
						MAVLink.mavlink_vfr_hud_t vfrHud = (MAVLink.mavlink_vfr_hud_t)message.data;
						break;
					case 141:
						//Altitude Message
						MAVLink.mavlink_altitude_t altitude = (MAVLink.mavlink_altitude_t)message.data;
						break;
					case 147:
						//Battery Status Message
						MAVLink.mavlink_battery_status_t batteryStatus = (MAVLink.mavlink_battery_status_t)message.data;
						break;
					case 230:
						//Estimator Status Message
						MAVLink.mavlink_estimator_status_t estimatorStatus = (MAVLink.mavlink_estimator_status_t)message.data;
						break;
					case 241:
						//Vibration Message
						MAVLink.mavlink_vibration_t vibration = (MAVLink.mavlink_vibration_t)message.data;
						break;
					case 245:
						//Extended System State Message
						MAVLink.mavlink_extended_sys_state_t extendedSystemState = (MAVLink.mavlink_extended_sys_state_t)message.data;
						break;
					case 9000:
						//Wheel Distance/Encoder Message
						MAVLink.mavlink_wheel_distance_t wheelDistance = (MAVLink.mavlink_wheel_distance_t)message.data;
						break;
					default:
						Debug.Log("MAVLink Message: ID: " + message.msgid.ToString());
						break;
				}
			}
		}
	}
}
