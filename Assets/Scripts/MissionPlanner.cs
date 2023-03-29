using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MissionPlanner : MonoBehaviour {
	//Mission Points
	public List<MissionPoint> missionPointList = new List<MissionPoint>();

	//Primite object for mission points
	public GameObject waypointObject;
	public GameObject samplePointObject;
	public LineRenderer waypointPath;

	public GameObject rotationCenterObject;
	public LineRenderer waypointPathSmooth;

	//Settings
	public bool addWaypointModeEnabled = false;
	public bool removeWaypointModeEnabled = false;
	public float sampleDistance;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
		//Add waypoint on Left Click
		if(addWaypointModeEnabled == false) {
			return;
		}
        if(Input.GetMouseButtonDown(0)) {
			//Avoid clickes over UI area (very rudimental...)
			if(Input.mousePosition.x < 100 || Input.mousePosition.y < 100) {
				return;
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit hit = new RaycastHit();

			if (Physics.Raycast(ray, out hit)) {
				if(hit.transform.tag == "Terrain") {
					//Clicked on Terrain
					GameObject newWaypoint = Instantiate(waypointObject, hit.point, Quaternion.identity);
					newWaypoint.transform.parent = this.transform;

					//Add sample points inbetween waypoints (last and new one)
					if(missionPointList.Count > 0) {
						//Vector3 lastPoint = missionPointList[missionPointList.Count - 1].position;
						//float distance = Vector3.Distance(lastPoint, newWaypoint.transform.position);

						//while (distance > sampleDistance) {
						//	Vector3 direction = newWaypoint.transform.position - lastPoint;
						//	direction.Normalize();
						//	direction *= sampleDistance;

						//	Vector3 newPoint = lastPoint + direction;
						//	GameObject newSamplePoint = Instantiate(samplePointObject, newPoint, Quaternion.identity);
						//	newSamplePoint.transform.parent = this.transform;

						//	MissionPoint samplePoint = new MissionPoint();
						//	samplePoint.position = newPoint;
						//	samplePoint.radius = 1;
						//	samplePoint.missionMode = MissionPoint.MissionMode.Sample;
						//	missionPointList.Add(samplePoint);

						//	//Add to waypoint path
						//	waypointPath.positionCount = missionPointList.Count;
						//	waypointPath.SetPosition(waypointPath.positionCount - 1, hit.point);

						//	lastPoint = missionPointList[missionPointList.Count - 1].position;
						//	distance = Vector3.Distance(lastPoint, newWaypoint.transform.position);
						//}
					}

					MissionPoint missionPoint = new MissionPoint();
					missionPoint.position = hit.point;
					missionPoint.radius = 1;
					if(missionPointList.Count == 0) {
						//First waypoint so set Mission Mode to "Start"
						missionPoint.missionMode = MissionPoint.MissionMode.Start;
					}
					else {
						//New added waypoint is last so set Mission Mode to "Stop"
						missionPoint.missionMode = MissionPoint.MissionMode.Stop;
						//Set previouse waypoint Mission Mode to "Waypoint" (was "Stop")
						missionPointList[missionPointList.Count - 1].missionMode = MissionPoint.MissionMode.Waypoint;

						if(missionPointList.Count >= 2) {
							//Round corners, for rover smooth movement
							Vector2 intersection = new Vector2(	missionPointList[missionPointList.Count - 1].position.x,
																missionPointList[missionPointList.Count - 1].position.z);
							Vector2 from = new Vector2(	missionPointList[missionPointList.Count - 2].position.x,
														missionPointList[missionPointList.Count - 2].position.z);
							Vector2 to = new Vector2(	hit.point.x,
														hit.point.z);
							Vector2[] points = DrawRoundedCorner(intersection, from, to, 2);

							if(points.Length > 0) {
								//Remove intersection point
								float y = missionPointList[missionPointList.Count - 1].position.y;
								Vector3 point = new Vector3(points[0].x, y, points[0].y);
								waypointPathSmooth.SetPosition(waypointPathSmooth.positionCount - 1, point);

								//Add circle points
								int index = waypointPathSmooth.positionCount - 1;
								waypointPathSmooth.positionCount = waypointPathSmooth.positionCount + points.Length - 1;
								for (int i = 1; i < points.Length; i++) {
									point = new Vector3(points[i].x, y, points[i].y);
									waypointPathSmooth.SetPosition(index + i, point);
								}
							}
						}
					}
					missionPointList.Add(missionPoint);

					//Add to waypoint path
					waypointPath.positionCount = waypointPath.positionCount + 1;
					waypointPath.SetPosition(waypointPath.positionCount - 1, hit.point);

					waypointPathSmooth.positionCount = waypointPathSmooth.positionCount + 1;
					waypointPathSmooth.SetPosition(waypointPathSmooth.positionCount - 1, hit.point);
				}
				else if(hit.transform.tag == "Waypoint") {
					//Clicked an existing waypoint
					MissionPoint missionPoint = missionPointList.Find((x) => x.position == hit.transform.position);
					missionPointList.Remove(missionPoint);

					if(missionPoint.missionMode == MissionPoint.MissionMode.Start && missionPointList.Count > 0) {
						missionPointList[0].missionMode = MissionPoint.MissionMode.Start;
					}
					else if(missionPoint.missionMode == MissionPoint.MissionMode.Stop && missionPointList.Count > 1) {
						missionPointList[missionPointList.Count - 1].missionMode = MissionPoint.MissionMode.Waypoint;
					}

					GameObject.Destroy(hit.transform.gameObject);

					//Redraw complete waypoint path
					waypointPath.positionCount = missionPointList.Count;
					for (int i = 0; i < missionPointList.Count; i++) {
						waypointPath.SetPosition(i, missionPointList[i].position);
					}
				}
			}
		}
    }

	private Vector2[] DrawRoundedCorner(Vector2 intersection, Vector2 start, Vector2 end, float radius) {
		//https://stackoverflow.com/questions/24771828/how-to-calculate-rounded-corners-for-a-polygon

		//Vectors between points
		Vector2 v1 = intersection - start;
		Vector2 v2 = intersection - end;

		//Angle between vectors
		float angle = Vector2.Angle(v1, v2) / 2;
		angle = angle * Mathf.Deg2Rad;

		// The length of segment between angular point and the points of intersection with the circle of a given radius
		float tan = Mathf.Abs(Mathf.Tan(angle));
		float segment = radius / tan;

		//Check the segment
		float length1 = v1.magnitude;
		float length2 = v2.magnitude;

		float length = Mathf.Min(length1, length2);

		if (segment > length) {
			segment = length;
			radius = (float)(length * tan);
			Debug.Log("Radius to large");
		}

		// Points of intersection are calculated by the proportion between the coordinates of the vector, length of vector and the length of the segment.
		Vector2 p1Cross = intersection - v1.normalized * segment;
		Vector2 p2Cross = intersection - v2.normalized * segment;

		//GameObject p1 = new GameObject();
		//p1.transform.position = new Vector3(p1Cross.x, 0, p1Cross.y);
		//GameObject p2 = new GameObject();
		//p2.transform.position = new Vector3(p2Cross.x, 0, p2Cross.y);

		// Calculation of the coordinates of the circle center by uing the perpendicular vector
		Vector2 centerShift = new Vector2(v1.y, -v1.x);	//(p1Cross - intersection) + (p2Cross - intersection);
		if(Vector2.Angle(centerShift, v2) < 90) {
			//Use other perpendicular vector (always the one on the "inside")
			centerShift = -centerShift;
		}
		Vector2 circlePoint = p1Cross + centerShift.normalized * radius;

		//GameObject c = new GameObject();
		GameObject c = Instantiate(rotationCenterObject, new Vector3(circlePoint.x, 0, circlePoint.y), Quaternion.identity);
		//c.transform.position = new Vector3(circlePoint.x, 0, circlePoint.y);
		//c.transform.parent = this.transform;

		//StartAngle and EndAngle of arc
		float startAngle = Vector2.SignedAngle(Vector2.right, p1Cross - circlePoint); // Mathf.Atan2(p1Cross.y - circlePoint.y, p1Cross.x - circlePoint.x);
		startAngle = startAngle * Mathf.Deg2Rad;
		float endAngle = Vector2.SignedAngle(Vector2.right, p2Cross - circlePoint); //Mathf.Atan2(p2Cross.y - circlePoint.y, p2Cross.x - circlePoint.x);
		endAngle = endAngle * Mathf.Deg2Rad;

		//Sweep angle
		float sweepAngle = endAngle - startAngle;

		//Some additional checks
		//if (sweepAngle < 0) {
		//	startAngle = endAngle;
		//	sweepAngle = -sweepAngle;
		//}

		if (sweepAngle > Mathf.PI) {
			sweepAngle = Mathf.PI - sweepAngle;
		}
		else if(sweepAngle < -Mathf.PI) {
			sweepAngle = 2*Mathf.PI + sweepAngle;
		}

		//One point for each degree. But in some cases it will be necessary to use more points. Just change a degreeFactor.
		float degreeFactor = 5 * Mathf.Deg2Rad;
		int pointsCount = (int)Mathf.Abs(sweepAngle / degreeFactor);
		int sign = (int)Mathf.Sign(sweepAngle);

		Vector2[] points = new Vector2[pointsCount];

		for (int i = 0; i < pointsCount; ++i) {
			float pointX = (float)(circlePoint.x + Mathf.Cos(startAngle + sign * i * degreeFactor) * radius);
			float pointY = (float)(circlePoint.y + Mathf.Sin(startAngle + sign * i * degreeFactor) * radius);

			points[i] = new Vector2(pointX, pointY);
		}

		return points;
		//return circlePoint;
	}

	public class MissionPoint {
		public enum MissionMode {
			Start,
			Waypoint,
			Stop,
			Sample
		}
		public Vector3 position;
		public float radius;
		public MissionMode missionMode;
	}
}
