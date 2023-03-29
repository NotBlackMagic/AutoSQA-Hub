using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverController : MonoBehaviour
{
	//MAVLink Link object
	public MAVLinkHub mavlink;

	//Buggy Transform
	public Transform buggyTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		buggyTransform.rotation = Quaternion.Euler(mavlink.rotation);
		//buggyTransform.position = mavlink.position;
    }
}
