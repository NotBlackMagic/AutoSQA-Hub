using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	//Camera (look at target)
	Transform target;

	//Look at settings
	public bool lockXZ = false;

    // Start is called before the first frame update
    void Start()
    {
		//Get main camera of the scene
		target = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
		// Rotate the camera every frame so it keeps looking at the target
		if (lockXZ == false) {
			transform.LookAt(target);
		}
		else {
			Vector3 targetProjected = new Vector3(target.position.x, this.transform.position.y, target.transform.position.z);
			transform.LookAt(targetProjected);
		}		
	}
}
