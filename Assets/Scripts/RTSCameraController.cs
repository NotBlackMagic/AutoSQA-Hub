using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
	//Holder for the Camera
	public Transform cameraTransform;

	//Camera following object transform
	public static RTSCameraController instance;
	public Transform followTransform;

	//Movement settings
	public float normalSpeed;
	public float fastSpeed;
	public float movementSpeed;
	public float movementTime;
	public float rotationAmount;
	public Vector3 rotationLimitMin;
	public Vector3 rotationLimitMax;
	public float zoomLimitMin;
	public float zoomLimitMax;
	public Vector3 zoomAmountKeyboard;
	public Vector3 zoomAmountMouse;

	Vector3 newPosition;
	Vector3 newRotation;
	Vector3 newZoom;

	//Mouse movement (drag) variables
	Vector3 dragStartPosition;
	Vector3 dragCurrentPosition;
	Vector3 rotateStartPosition;
	Vector3 rotateCurrentPosition;

    // Start is called before the first frame update
    void Start() {
		newPosition = this.transform.position;
		newRotation = this.transform.rotation.eulerAngles;
		newZoom = cameraTransform.localPosition;

		//cameraTransform.LookAt(this.transform);
	}

    // Update is called once per frame
    void LateUpdate() {
		if(followTransform != null) {
			this.transform.position = followTransform.position;
		}
		else {
			MouseInputHandler();
			MovementInputHandler();

			//RaycastHit hit;
			//if(Physics.Raycast(this.transform.position, -Vector3.up, out hit)) {
			//	Vector3 offset = transform.position - hit.point;
			//	this.transform.position -= offset;
			//}
		}
		
		if(Input.GetKeyDown(KeyCode.Escape)) {
			followTransform = null;
		}
	}

	void MouseInputHandler() {
		//Camera movement (position) based on mouse left button click-n-drag
		if(Input.GetMouseButtonDown(0)) {
			Plane plane = new Plane(Vector3.up, Vector3.zero);

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			float entry;

			if(plane.Raycast(ray, out entry)) {
				dragStartPosition = ray.GetPoint(entry);
			}
		}
		if(Input.GetMouseButton(0)) {
			Plane plane = new Plane(Vector3.up, Vector3.zero);

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			float entry;

			if (plane.Raycast(ray, out entry)) {
				dragCurrentPosition = ray.GetPoint(entry);

				newPosition = this.transform.position + (dragStartPosition - dragCurrentPosition);
			}
		}

		//Camera movement (rotation) based on mouse middle button click-n-drag
		if(Input.GetMouseButtonDown(2)) {
			rotateStartPosition = Input.mousePosition;
		}
		if(Input.GetMouseButton(2)) {
			rotateCurrentPosition = Input.mousePosition;

			Vector3 difference = rotateStartPosition - rotateCurrentPosition;
			rotateStartPosition = rotateCurrentPosition;

			//newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5.0f));

			newRotation.x += (-difference.y / 5.0f);

			//Keep inside limits
			if(newRotation.x > rotationLimitMax.x) {
				newRotation.x = rotationLimitMax.x;
			}
			else if(newRotation.x < rotationLimitMin.x) {
				newRotation.x = rotationLimitMin.x;
			}

			newRotation.y += (-difference.x / 5.0f);
		}

		//Camera Zoom based on mouse scroll wheel
		if (Input.mouseScrollDelta.y != 0) {
			float distance = Vector3.Distance(this.transform.position, cameraTransform.localPosition);
			newZoom -= Input.mouseScrollDelta.y * zoomAmountMouse * distance;

			if (newZoom.y > zoomLimitMax) {
				newZoom.y = zoomLimitMax;
			}
			else if(newZoom.y < zoomLimitMin) {
				newZoom.y = zoomLimitMin;
			}
		}
	}

	void MovementInputHandler() {
		//Change movement speed based on SHIFT key
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
			movementSpeed = fastSpeed;
		}
		else {
			movementSpeed = normalSpeed;
		}

		//Camera movement (position) using WASD or Arrow Keys
		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			newPosition += (Vector3.forward * movementSpeed);
		}
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
			newPosition += (Vector3.forward * -movementSpeed);
		}
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
			newPosition += (Vector3.right * movementSpeed);
		}
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
			newPosition += (Vector3.right * -movementSpeed);
		}

		//Camera movement (rotation) based on QE keys
		if(Input.GetKey(KeyCode.Q)) {
			//newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
			newRotation.y += rotationAmount;
		}
		if (Input.GetKey(KeyCode.E)) {
			//newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
			newRotation.y += -rotationAmount;
		}

		//Quick set rotation: "T" for Top Down, "P" for 45deg perspective
		if (Input.GetKey(KeyCode.T)) {
			newRotation.x = 0;
			newRotation.y = 0;
		}
		if (Input.GetKey(KeyCode.P)) {
			newRotation.x = -45;
			newRotation.y = 45;
		}

		//Camera Zoom based on R(+) and F(-) Key
		if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.KeypadPlus)) {
			float distance = Vector3.Distance(this.transform.position, cameraTransform.localPosition);
			newZoom += zoomAmountKeyboard * distance;

			if (newZoom.y > zoomLimitMax) {
				newZoom.y = zoomLimitMax;
			}
		}
		if(Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.KeypadMinus)) {
			float distance = Vector3.Distance(this.transform.position, cameraTransform.localPosition);
			newZoom -= zoomAmountKeyboard * distance;

			if (newZoom.y < zoomLimitMin) {
				newZoom.y = zoomLimitMin;
			}
		}

		//Smoothing position transition from old to new
		this.transform.position = Vector3.Lerp(this.transform.position, newPosition, Time.deltaTime * movementTime);
		this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(newRotation), Time.deltaTime * movementTime);
		cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
	}
}
