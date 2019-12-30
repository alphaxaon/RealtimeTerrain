using UnityEngine;
using System.Collections;

/**
 * Terrain Generator
**/
public class TerrainGenerator : MonoBehaviour
{
    /**
     * Setup variables
    **/
    Terrain terrain;
    string status = "";

    /**
     * Customizable parameters
    **/
	public int terrainWidth = 1000;
	public int terrainHeight = 100;
	public int terrainLength = 1000;
	public float noiseScale = 200;
	public int octaves = 5;
	[Range(0,1)]
	public float persistance;
	public float lacunarity = 2;
	public int seed = 0;
    public Vector2 offset = new Vector2(0, 0);
    public bool showStatus = false;
    public bool generateTerrainOnStart = true;

    /**
     * Generate the terrain
    **/
    private IEnumerator GenerateTerrain()
    {
		terrain = GetComponent<Terrain>();

		if (!terrain) {
            status = "Generate failed. No Terrain Component attached to this GameObject.";
			Debug.Log(status);
            yield break;
		}

        // Set terrain size
        status = "Resetting terrain";
        TerrainData terrain_data = terrain.terrainData;
        terrain_data.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
        yield return null;

        // Reset
        terrain_data.SetHeights(0, 0, new float[terrain_data.heightmapResolution, terrain_data.heightmapResolution]);
        terrain_data.treeInstances = new TreeInstance[0];
        terrain_data.SetDetailLayer(0, 0, 0, new int[terrain_data.detailWidth, terrain_data.detailHeight]);
        terrain_data.SetAlphamaps(0, 0, new float[terrain_data.alphamapWidth, terrain_data.alphamapHeight, terrain_data.alphamapLayers]);
        terrain.Flush();
        yield return null;

        // Generate new random seed value
        status = "Generating terrain";
        yield return new WaitForSeconds(0.1f);
        seed = Random.Range (0, 60000);

        // Generate features
        yield return StartCoroutine(GenerateShape(terrain_data, 20));
        yield return StartCoroutine(GenerateCanyons(terrain_data, 20));
        yield return StartCoroutine(GenerateErosion(terrain_data, 20));

        // Finish up
        status = "Terrain generated";
        yield return new WaitForSeconds(0.1f);
        terrain_data = null;
    }

    /**
     * Generate a noisemap to use as a heightmap for our terrain
    **/
	private IEnumerator GenerateShape(TerrainData terrain_data, int updateEveryNthFrame) 
    {
        float[,] noiseMap = Noise.GenerateMap(terrain_data.heightmapResolution, terrain_data.heightmapResolution, seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] heights = terrain_data.GetHeights(0, 0, terrain_data.heightmapResolution, terrain_data.heightmapResolution);
        int t = updateEveryNthFrame;

        for (int y = 0; y < terrain_data.heightmapResolution; y++) {
            for (int x = 0; x < terrain_data.heightmapResolution; x++) {
                float currentHeight = noiseMap[x, y];
                heights[y, x] = currentHeight;
            }
            t--;
            if (t <= 0) {
                status = "Terraforming land " + y + "/" + terrain_data.heightmapResolution;
                terrain_data.SetHeights(0, 0, heights);
                t = updateEveryNthFrame;
                yield return null;
            }
        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();
    }

    /**
     * Generate reverse heightmap to create canyons
    **/
	private IEnumerator GenerateCanyons(TerrainData terrain_data, int updateEveryNthFrame) 
    {
		float[,] noiseMap = Noise.GenerateMap (terrain_data.heightmapResolution, terrain_data.heightmapResolution, seed + 1, noiseScale/2, octaves, persistance, lacunarity + 1, offset);
		float[,] heights = terrain_data.GetHeights (0, 0, terrain_data.heightmapResolution, terrain_data.heightmapResolution);
        int t = updateEveryNthFrame;

		for (int y = 0; y < terrain_data.heightmapResolution; y++) {
			for (int x = 0; x < terrain_data.heightmapResolution; x++) {
				float currentHeight = noiseMap [x, y];

				if (currentHeight < 0.5f)
					heights [y, x] = heights [y, x] / 1.3f;
			}

            t--;

            if (t <= 0)
            {
                status = "Generating canyons " + y + "/" + terrain_data.heightmapResolution;
                terrain_data.SetHeights(0, 0, heights);
                t = updateEveryNthFrame;
                yield return null;
            }
        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();

        // Add more variation
        noiseMap = Noise.GenerateMap(terrain_data.heightmapResolution, terrain_data.heightmapResolution, seed + 2, noiseScale / 2, octaves + 1, persistance / 2, lacunarity, offset);
        heights = terrain_data.GetHeights(0, 0, terrain_data.heightmapResolution, terrain_data.heightmapResolution);
        t = updateEveryNthFrame;

        for (int y = 0; y < terrain_data.heightmapResolution; y++) {
            for (int x = 0; x < terrain_data.heightmapResolution; x++) {
                float currentHeight = noiseMap[x, y];

                if (currentHeight > 0.5f)
                    heights[y, x] = heights[y, x] + currentHeight / 10;
            }
            t--;
            if (t <= 0)
            {
                status = "2nd canyon pass " + y + "/" + terrain_data.heightmapResolution;
                terrain_data.SetHeights(0, 0, heights);
                t = updateEveryNthFrame;
                yield return null;
            }
        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();
    }

    /**
     * Generate finer noise to add detail
    **/
    public IEnumerator GenerateErosion(TerrainData terrain_data, int updateEveryNthFrame)
    {
        float[,] noiseMap = Noise.GenerateMap(terrain_data.heightmapResolution, terrain_data.heightmapResolution, seed + 3, noiseScale / 2, octaves + 1, persistance / 2, lacunarity * 4, offset);
        float[,] heights = terrain_data.GetHeights(0, 0, terrain_data.heightmapResolution, terrain_data.heightmapResolution);
        int t = updateEveryNthFrame;

        for (int y = 0; y < terrain_data.heightmapResolution; y++) {
            for (int x = 0; x < terrain_data.heightmapResolution; x++) {
                float currentHeight = noiseMap[x, y];

                if (currentHeight > 0.0f && currentHeight < 0.4f)
                    heights[y, x] = (heights[y, x] + currentHeight) / 2;
            }

            t--;

            if (t <= 0)
            {
                status = "Adding erosion " + y + "/" + terrain_data.heightmapResolution;
                terrain_data.SetHeights(0, 0, heights);
                t = updateEveryNthFrame;
                yield return null;
            }
        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();
    }

    /**
     * Set limits on customizable values
    **/
    void OnValidate() {
		if (terrainWidth < 1)
			terrainWidth = 1;

		if (terrainHeight < 1)
			terrainHeight = 1;

		if (lacunarity < 1)
			lacunarity = 1;

		if (octaves < 0)
			octaves = 0;
	}

    /**
     * Display generation status
    **/
    void OnGUI()
    {
        if (showStatus)
            GUI.Label(new Rect(10, 10, 300, 50), status);
    }

    /**
     * Generate terrain when game starts
    **/
    private void Start()
    {
        if (generateTerrainOnStart)
            StartCoroutine(GenerateTerrain());
    }

    /**
     * Generate terrain in Editor
    **/
    public void GenerateTerrainInEditor()
    {
        StartCoroutine(GenerateTerrain());
    }
}