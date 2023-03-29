using UnityEngine;
using UnityEngine.UIElements;

public class MapLayerMenu : MonoBehaviour {

	public Color selectedColor = new Color(0, 0.5f, 0);
	public Color unselectedColor = new Color(0.125f, 0.125f, 0.125f);

	enum MapLayers {
		Air_Temperature = 0,
		Air_Humidity = 1,
		Air_Pressure = 2,
		Magnetic_Field = 3,
		Soil_Temperature = 4,
		Soil_Humidity = 5,
		Soil_EC = 6,
		Soil_pH = 7,
		Soil_Salinity = 8,
		Soil_Strength = 9
	};
	Button layerMenu;
	Button[] layerSelectButtons = new Button[10];
	bool[] lazerSelectButtonClicked = new bool[10];

	public MapLayerDrawer mapLayerDrawer;

	public void OnEnable() {
		VisualElement root = GetComponent<UIDocument>().rootVisualElement;
		
		//Get all menu buttons
		layerMenu = root.Q<Button>("MapLayerMenuButton");
		layerSelectButtons[(int)MapLayers.Air_Temperature] = root.Q<Button>("MapLayerAirTemperatureButton");
		layerSelectButtons[(int)MapLayers.Air_Humidity] = root.Q<Button>("MapLayerAirHumidityButton");
		layerSelectButtons[(int)MapLayers.Air_Pressure] = root.Q<Button>("MapLayerAirPressureButton");
		layerSelectButtons[(int)MapLayers.Magnetic_Field] = root.Q<Button>("MapLayerMagneticFieldButton");
		layerSelectButtons[(int)MapLayers.Soil_Temperature] = root.Q<Button>("MapLayerSoilTemperatureButton");
		layerSelectButtons[(int)MapLayers.Soil_Humidity] = root.Q<Button>("MapLayerSoilHumidityButton");
		layerSelectButtons[(int)MapLayers.Soil_EC] = root.Q<Button>("MapLayerSoilECButton");
		layerSelectButtons[(int)MapLayers.Soil_pH] = root.Q<Button>("MapLayerSoilpHButton");
		layerSelectButtons[(int)MapLayers.Soil_Salinity] = root.Q<Button>("MapLayerSoilSalinityButton");
		layerSelectButtons[(int)MapLayers.Soil_Strength] = root.Q<Button>("MapLayerSoilStrengthButton");
		
		//Setup callbacks for all menu buttons
		layerMenu.clicked += () => LayerMenuClicked();
		layerSelectButtons[(int)MapLayers.Air_Temperature].clicked += () => LayerAirTemperatureClicked();
		layerSelectButtons[(int)MapLayers.Air_Humidity].clicked += () => LayerAirHumidityClicked();
		layerSelectButtons[(int)MapLayers.Air_Pressure].clicked += () => LayerAirPressureClicked();
		layerSelectButtons[(int)MapLayers.Magnetic_Field].clicked += () => LayerMagneticFieldClicked();
		layerSelectButtons[(int)MapLayers.Soil_Temperature].clicked += () => LayerSoilTemperatureClicked();
		layerSelectButtons[(int)MapLayers.Soil_Humidity].clicked += () => LayerSoilHumidityClicked();
		layerSelectButtons[(int)MapLayers.Soil_EC].clicked += () => LayerSoilECClicked();
		layerSelectButtons[(int)MapLayers.Soil_pH].clicked += () => LayerSoilpHClicked();
		layerSelectButtons[(int)MapLayers.Soil_Salinity].clicked += () => LayerSoilSalinityClicked();
		layerSelectButtons[(int)MapLayers.Soil_Strength].clicked += () => LayerSoilStrengthClicked();

		//Init clicked
		for (int i = 0; i < lazerSelectButtonClicked.Length; i++) {
			lazerSelectButtonClicked[i] = false;
		}
	}

	void LayerMenuClicked() {
		Debug.Log("Layer Menu Button Clicked!");
	}

	void LayerSelectionChanged(MapLayers layer, bool selected) {
		lazerSelectButtonClicked[(int)layer] = selected;

		if (lazerSelectButtonClicked[(int)layer] == true) {
			layerSelectButtons[(int)layer].style.borderLeftColor = selectedColor;
			layerSelectButtons[(int)layer].style.borderRightColor = selectedColor;
			layerSelectButtons[(int)layer].style.borderTopColor = selectedColor;
			layerSelectButtons[(int)layer].style.borderBottomColor = selectedColor;
		}
		else {
			layerSelectButtons[(int)layer].style.borderLeftColor = unselectedColor;
			layerSelectButtons[(int)layer].style.borderRightColor = unselectedColor;
			layerSelectButtons[(int)layer].style.borderTopColor = unselectedColor;
			layerSelectButtons[(int)layer].style.borderBottomColor = unselectedColor;
		}

		//Disable all others
		for (int i = 0; i < lazerSelectButtonClicked.Length; i++) {
			if (i == (int)layer) {
				continue;
			}
			lazerSelectButtonClicked[i] = false;

			layerSelectButtons[i].style.borderLeftColor = unselectedColor;
			layerSelectButtons[i].style.borderRightColor = unselectedColor;
			layerSelectButtons[i].style.borderTopColor = unselectedColor;
			layerSelectButtons[i].style.borderBottomColor = unselectedColor;
		}

		mapLayerDrawer.ChangeSelectedLayer((MapLayerDrawer.MapLayers)layer, lazerSelectButtonClicked[(int)layer]);
		mapLayerDrawer.UpdateMapValues((MapLayerDrawer.MapLayers)layer, new Vector2(0, 0), 25.0f);
	}
	void LayerAirTemperatureClicked() {
		//Debug.Log("Layer Air Temperature Button Clicked!");
		int layer = (int)MapLayers.Air_Temperature;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Air_Temperature, lazerSelectButtonClicked[layer]);
	}
	void LayerAirHumidityClicked() {
		//Debug.Log("Layer Air Humidty Button Clicked!");
		int layer = (int)MapLayers.Air_Humidity;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Air_Humidity, lazerSelectButtonClicked[layer]);
	}
	void LayerAirPressureClicked() {
		//Debug.Log("Layer Air Pressure Button Clicked!");
		int layer = (int)MapLayers.Air_Pressure;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Air_Pressure, lazerSelectButtonClicked[layer]);
	}
	void LayerMagneticFieldClicked() {
		//Debug.Log("Layer Magentic Field Button Clicked!");
		int layer = (int)MapLayers.Magnetic_Field;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Magnetic_Field, lazerSelectButtonClicked[layer]);
	}
	void LayerSoilTemperatureClicked() {
		//Debug.Log("Layer Soil Temperature Button Clicked!");
		int layer = (int)MapLayers.Soil_Temperature;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Soil_Temperature, lazerSelectButtonClicked[layer]);
	}
	void LayerSoilHumidityClicked() {
		//Debug.Log("Layer Soil Humidty Button Clicked!");
		int layer = (int)MapLayers.Soil_Humidity;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Soil_Humidity, lazerSelectButtonClicked[layer]);
	}
	void LayerSoilECClicked() {
		//Debug.Log("Layer Soil EC Button Clicked!");
		int layer = (int)MapLayers.Soil_EC;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Soil_EC, lazerSelectButtonClicked[layer]);
	}
	void LayerSoilpHClicked() {
		//Debug.Log("Layer Soil pH Button Clicked!");
		int layer = (int)MapLayers.Soil_pH;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Soil_pH, lazerSelectButtonClicked[layer]);
	}
	void LayerSoilSalinityClicked() {
		//Debug.Log("Layer Soil Salinity Button Clicked!");
		int layer = (int)MapLayers.Soil_Salinity;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Soil_Salinity, lazerSelectButtonClicked[layer]);
	}

	void LayerSoilStrengthClicked() {
		//Debug.Log("Layer Soil Strength Button Clicked!");
		int layer = (int)MapLayers.Soil_Strength;
		lazerSelectButtonClicked[layer] = !lazerSelectButtonClicked[layer];
		LayerSelectionChanged(MapLayers.Soil_Strength, lazerSelectButtonClicked[layer]);
	}
}
