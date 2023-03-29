using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLayerDrawer : MonoBehaviour {
	// Map Layer Struct
	public struct MapLayerStruct {
		public float value;
		public float uncertainty;
	}
	public enum MapLayers {
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

	//Map charateristics
	[Header("Magnetic Map Parameters")]
	public Vector2 origin = new Vector2(0.0f, 0.0f);
	public Vector2 dimension = new Vector2(30.0f, 20.0f);
	public float resolution = 0.1f;

	[Header("GameObjects for Pose and Map visualization")]
	public GameObject layerMapPlane;

	//Map Layers
	const int mapLayerCounts = 10;
	public float interpolationRange = 1;
	public Vector2[] mapLayerRange = new Vector2[mapLayerCounts];
	public MapLayerStruct[][,] mapLayerValues = new MapLayerStruct[mapLayerCounts][,];

	public bool[] enabledLayers = new bool[mapLayerCounts];
	public Texture2D emptyMapTexture;
	public Texture2D[] mapLayerTexture = new Texture2D[mapLayerCounts];

	// Start is called before the first frame update
	void Start() {
		//Init layer maps
		int sizeX = Mathf.RoundToInt(dimension.x / resolution);
		int sizeY = Mathf.RoundToInt(dimension.y / resolution);

		for (int l = 0; l < 9; l++) {
			mapLayerValues[l] = new MapLayerStruct[sizeX, sizeY];
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					mapLayerValues[l][x, y].value = 0.0f;
					mapLayerValues[l][x, y].uncertainty = 1.0f;
				}
			}
		}
	}

	// Update is called once per frame
	void Update() {
        
	}

	public void ChangeSelectedLayer(MapLayers layer, bool show) {
		enabledLayers[(int)layer] = show;
	}

	public void UpdateMapValues(MapLayers layer, Vector2 localCoordinate, float value) {
		//Debug.Log("New sample: " + localCoordinate.x.ToString("F2") + ";" + localCoordinate.y.ToString("F2") + " " + value.ToString("F2"));

		//Convert from local coordinate to map layer array index
		int indexX = Mathf.CeilToInt((localCoordinate.x - (origin.x - (dimension.x / 2.0f))) * (mapLayerValues[(int)layer].GetLength(0) / dimension.x));
		int indexY = Mathf.CeilToInt((localCoordinate.y - (origin.y - (dimension.y / 2.0f))) * (mapLayerValues[(int)layer].GetLength(1) / dimension.y));

		//Check for index out of range
		if(indexX < 0 || indexX >= mapLayerValues[(int)layer].GetLength(0)) {
			return;
		}
		if (indexY < 0 || indexY >= mapLayerValues[(int)layer].GetLength(1)) {
			return;
		}

		//Update values
		mapLayerValues[(int)layer][indexX, indexY].value = value;
		mapLayerValues[(int)layer][indexX, indexY].uncertainty = 0;

		//Linear Decay in fixed range from newly added point
		 for(int lx = (indexX - Mathf.RoundToInt(interpolationRange / resolution)); lx < (indexX + Mathf.RoundToInt(interpolationRange / resolution)); lx += 1) {
			for (int ly = (indexY - Mathf.RoundToInt(interpolationRange / resolution)); ly < (indexY + Mathf.RoundToInt(interpolationRange / resolution)); ly += 1) {
				if (lx < 0 || ly < 0 || lx >= mapLayerValues[(int)layer].GetLength(0) || ly >= mapLayerValues[(int)layer].GetLength(1)) {
					continue;
				}

				//The loop is for a square area, the bellow code makes sure that only the cirular portion of it is used
				Vector2 robotPos = new Vector2(indexX, indexY);
				Vector2 pointPos = new Vector2(lx, ly);
				float distance = Vector2.Distance(robotPos, pointPos);
				if (distance > (interpolationRange / resolution)) {
					continue;
				}

				//Uncertainty increases linearly with distance from real reading
				float newUncertainty = distance / (interpolationRange / resolution);

				//Only use estimated value if it has lower uncertainty aka is closer to a real reading
				if(mapLayerValues[(int)layer][lx, ly].uncertainty > newUncertainty) {
					mapLayerValues[(int)layer][lx, ly].value = value;
					mapLayerValues[(int)layer][lx, ly].uncertainty = newUncertainty;
				}
			}
		}

		//Update corresponding texture
		//Destroy(airTemperatureMapTexture);
		mapLayerTexture[(int)layer] = GenerateTexture(mapLayerValues[(int)layer], mapLayerRange[(int)layer]);

		//if(enabledLayers[(int)layer] == true) {
		//	layerMapPlane.GetComponent<Renderer>().material.mainTexture = mapLayerTexture[(int)layer];
		//}
		//else {
		//	layerMapPlane.GetComponent<Renderer>().material.mainTexture = emptyMapTexture;
		//}

		bool layerEnabled = false;
		for (int l = 0; l < mapLayerCounts; l++) {
			if (enabledLayers[l] == true) {
				layerMapPlane.GetComponent<Renderer>().material.mainTexture = mapLayerTexture[l];
				layerEnabled = true;
				break;
			}
		}

		if (layerEnabled == false) {
			layerMapPlane.GetComponent<Renderer>().material.mainTexture = emptyMapTexture;
		}
	}

	Texture2D GenerateTexture(MapLayerStruct[,] values, Vector2 maxRange) {
		Texture2D texture = new Texture2D(values.GetLength(0), values.GetLength(1), TextureFormat.ARGB32, false);
		texture.filterMode = FilterMode.Point;

		//Generate Gradient used to draw the Magnetic Field Texture
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKey = new GradientColorKey[5];
		GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

		//Add Color Keys
		colorKey[0].color = Color.blue;
		colorKey[0].time = 0.0f;
		colorKey[1].color = Color.cyan;
		colorKey[1].time = 0.25f;
		colorKey[2].color = Color.green;
		colorKey[2].time = 0.5f;
		colorKey[3].color = Color.yellow;
		colorKey[3].time = 0.75f;
		colorKey[4].color = Color.red;
		colorKey[4].time = 1.0f;

		//Add Alpha Keys
		alphaKey[0].alpha = 1.0f;
		alphaKey[0].time = 0.0f;
		alphaKey[1].alpha = 1.0f;
		alphaKey[1].time = 1.0f;

		//Apply keys
		gradient.SetKeys(colorKey, alphaKey);

		Color[] colors = new Color[values.GetLength(0) * values.GetLength(1)];
		for (int x = 0; x < values.GetLength(0); x++) {
			for (int y = 0; y < values.GetLength(1); y++) {
				//Scale values to range 0 to 1
				float colorIndex = (values[x, y].value - maxRange.x) * (1.0f / (maxRange.y - maxRange.x));
				if(colorIndex > 1) {
					colorIndex = 1;
				}
				else if(colorIndex < 0) {
					colorIndex = 0;
				}
				Color color = gradient.Evaluate(colorIndex);
				color.a = (1.0f - values[x, y].uncertainty);
				colors[(values.GetLength(0) - x - 1) + (values.GetLength(1) - y - 1) * values.GetLength(0)] = color;
			}
		}

		texture.SetPixels(colors, 0);
		texture.Apply();

		return texture;
	}

	Texture2D GenerateTexture(float[,] values) {
		Texture2D texture = new Texture2D(values.GetLength(0), values.GetLength(1), TextureFormat.ARGB32, false);
		texture.filterMode = FilterMode.Point;

		//Generate Gradient used to draw the Magnetic Field Texture
		Gradient gradient = new Gradient();
		GradientColorKey[] colorKey = new GradientColorKey[2];
		GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

		//Add Color Keys
		colorKey[0].color = Color.green;
		colorKey[0].time = 0.0f;
		// colorKey[1].color = Color.cyan;
		// colorKey[1].time = 0.25f;
		// colorKey[2].color = Color.green;
		// colorKey[2].time = 0.5f;
		// colorKey[3].color = Color.yellow;
		// colorKey[3].time = 0.75f;
		colorKey[1].color = Color.red;
		colorKey[1].time = 1.0f;

		//Add Alpha Keys
		alphaKey[0].alpha = 0.5f;
		alphaKey[0].time = 0.0f;
		alphaKey[1].alpha = 0.5f;
		alphaKey[1].time = 1.0f;

		//Apply keys
		gradient.SetKeys(colorKey, alphaKey);

		Color[] colors = new Color[values.GetLength(0) * values.GetLength(1)];
		for (int x = 0; x < values.GetLength(0); x++) {
			for (int y = 0; y < values.GetLength(1); y++) {
				float colorIndex = values[x, y];
				Color color = gradient.Evaluate(colorIndex);
				colors[(values.GetLength(0) - x - 1) + (values.GetLength(1) - y - 1) * values.GetLength(0)] = color;
			}
		}

		texture.SetPixels(colors, 0);
		texture.Apply();

		return texture;
	}
}
