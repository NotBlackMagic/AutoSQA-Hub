using UnityEngine;
using UnityEngine.UIElements;

public class PathMenu : MonoBehaviour {
	public MissionPlanner missionPlanner;

	public Color selectedColor = new Color(0, 0.5f, 0);
	public Color unselectedColor = new Color(0.125f, 0.125f, 0.125f);

	Button pathMenu;
	Button pathAddWaypoint;
	Button pathRemoveWaypoint;

	bool pathMenuButtonSelected = false;
	bool addWaypointButtonSelected = false;
	bool removeWaypointButtonSelected = false;
	public void OnEnable() {
		VisualElement root = GetComponent<UIDocument>().rootVisualElement;

		//Get all menu buttons
		pathMenu = root.Q<Button>("PathMenuButton");
		pathAddWaypoint = root.Q<Button>("PathAddWaypointButton");
		pathRemoveWaypoint = root.Q<Button>("PathRemoveWaypointButton");

		//Setup callbacks for all menu buttons
		pathMenu.clicked += () => PathMenuClicked();
		pathAddWaypoint.clicked += () => PathAddWaypointClicked();
		pathRemoveWaypoint.clicked += () => PathRemoveWaypointClicked();
	}

	void PathMenuClicked() {
		//Debug.Log("Path Menu Button Clicked!");

		pathMenuButtonSelected = !pathMenuButtonSelected;
		addWaypointButtonSelected = false;
		removeWaypointButtonSelected = false;
	}

	void PathAddWaypointClicked() {
		//Debug.Log("Path Add Waypoint Button Clicked!");

		addWaypointButtonSelected = !addWaypointButtonSelected;
		removeWaypointButtonSelected = false;

		if(addWaypointButtonSelected == true) {
			pathAddWaypoint.style.borderLeftColor = selectedColor;
			pathAddWaypoint.style.borderRightColor = selectedColor;
			pathAddWaypoint.style.borderTopColor = selectedColor;
			pathAddWaypoint.style.borderBottomColor = selectedColor;
		}
		else {
			pathAddWaypoint.style.borderLeftColor = unselectedColor;
			pathAddWaypoint.style.borderRightColor = unselectedColor;
			pathAddWaypoint.style.borderTopColor = unselectedColor;
			pathAddWaypoint.style.borderBottomColor = unselectedColor;
		}

		if (removeWaypointButtonSelected == true) {
			pathRemoveWaypoint.style.borderLeftColor = selectedColor;
			pathRemoveWaypoint.style.borderRightColor = selectedColor;
			pathRemoveWaypoint.style.borderTopColor = selectedColor;
			pathRemoveWaypoint.style.borderBottomColor = selectedColor;
		}
		else {
			pathRemoveWaypoint.style.borderLeftColor = unselectedColor;
			pathRemoveWaypoint.style.borderRightColor = unselectedColor;
			pathRemoveWaypoint.style.borderTopColor = unselectedColor;
			pathRemoveWaypoint.style.borderBottomColor = unselectedColor;
		}

		missionPlanner.addWaypointModeEnabled = addWaypointButtonSelected;
		missionPlanner.removeWaypointModeEnabled = removeWaypointButtonSelected;
	}

	void PathRemoveWaypointClicked() {
		//Debug.Log("Path Remove Waypoint Button Clicked!");

		removeWaypointButtonSelected = !removeWaypointButtonSelected;
		addWaypointButtonSelected = false;

		if (addWaypointButtonSelected == true) {
			pathAddWaypoint.style.borderLeftColor = selectedColor;
			pathAddWaypoint.style.borderRightColor = selectedColor;
			pathAddWaypoint.style.borderTopColor = selectedColor;
			pathAddWaypoint.style.borderBottomColor = selectedColor;
		}
		else {
			pathAddWaypoint.style.borderLeftColor = unselectedColor;
			pathAddWaypoint.style.borderRightColor = unselectedColor;
			pathAddWaypoint.style.borderTopColor = unselectedColor;
			pathAddWaypoint.style.borderBottomColor = unselectedColor;
		}

		if (removeWaypointButtonSelected == true) {
			pathRemoveWaypoint.style.borderLeftColor = selectedColor;
			pathRemoveWaypoint.style.borderRightColor = selectedColor;
			pathRemoveWaypoint.style.borderTopColor = selectedColor;
			pathRemoveWaypoint.style.borderBottomColor = selectedColor;
		}
		else {
			pathRemoveWaypoint.style.borderLeftColor = unselectedColor;
			pathRemoveWaypoint.style.borderRightColor = unselectedColor;
			pathRemoveWaypoint.style.borderTopColor = unselectedColor;
			pathRemoveWaypoint.style.borderBottomColor = unselectedColor;
		}

		missionPlanner.addWaypointModeEnabled = addWaypointButtonSelected;
		missionPlanner.removeWaypointModeEnabled = removeWaypointButtonSelected;
	}
}
