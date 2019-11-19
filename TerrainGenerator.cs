using UnityEngine;
using System.Collections;
using System.Linq;

public class TerrainGenerator : MonoBehaviour {

	public int terrainWidth = 1000;
	public int terrainHeight = 100;
	public int terrainLength = 1000;
	public float noiseScale = 200;
	public int octaves = 5;
	[Range(0,1)]
	public float persistance;
	public float lacunarity = 2;
	public int seed = 0;
    [Range(0, 1)]
    public float treeDensity;
    public int distanceBetweenTrees = 1;
	public Vector2 offset = new Vector2(0, 0);
	public GameObject RandomTree;
	//public GameObject particles;
	public GameObject[] rocks;
	Terrain terrain;
    string status = "";
    bool showStatus = true;
    GameObject player;
    Rigidbody playerBody;

    public IEnumerator GenerateTerrain() {

		terrain = GetComponent<Terrain> ();
		if (!terrain) {
			Debug.Log ("Generate failed. No Terrain Component attached to this GameObject.");
		}

        status = "Resetting terrain";

        // Set terrain size
        TerrainData terrain_data = terrain.terrainData;
        terrain_data.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
        yield return null;

        // Reset
        terrain_data.SetHeights(0, 0, new float[terrain_data.heightmapWidth, terrain_data.heightmapHeight]);
        terrain_data.treeInstances = new TreeInstance[0];
        terrain_data.SetDetailLayer(0, 0, 0, new int[terrain_data.detailWidth, terrain_data.detailHeight]);
        //terrain_data.SetAlphamaps(0, 0, new float[terrain_data.alphamapWidth, terrain_data.alphamapHeight, terrain_data.alphamapLayers]);
        terrain.Flush();
        yield return null;

        // Setup player
        player = GameObject.FindGameObjectWithTag("Player");
        PlacePlayerOnGround();

        status = "Generating terrain";
        yield return new WaitForSeconds(2f);

        // Generate new random seed value
        seed = Random.Range (0, 60000);

        yield return StartCoroutine(GenerateShape(terrain_data));
        yield return StartCoroutine(GenerateCanyons(terrain_data));
        yield return StartCoroutine(GenerateErosion(terrain_data));

        status = "Terrain generated";
        yield return new WaitForSeconds(2f);
        //showStatus = false;
        terrain_data = null;
    }

	public IEnumerator GenerateShape(TerrainData terrain_data) {

        float[,] noiseMap = Noise.GenerateMap(terrain_data.heightmapWidth, terrain_data.heightmapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] heights = terrain_data.GetHeights(0, 0, terrain_data.heightmapWidth, terrain_data.heightmapHeight);
        int t = 20;

        for (int y = 0; y < terrain_data.heightmapHeight; y++)
        {
            for (int x = 0; x < terrain_data.heightmapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                heights[y, x] = currentHeight;
            }
            t--;
            if (t <= 0)
            {
                status = "Terraforming land " + y + "/" + terrain_data.heightmapHeight;
                terrain_data.SetHeights(0, 0, heights);
                PlacePlayerOnGround();
                t = 20;
                yield return null;
            }
        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();
    }

	public IEnumerator GenerateCanyons(TerrainData terrain_data) {

		float[,] noiseMap = Noise.GenerateMap (terrain_data.heightmapWidth, terrain_data.heightmapHeight, seed + 1, noiseScale/2, octaves, persistance, lacunarity + 1, offset);
		float[,] heights = terrain_data.GetHeights (0, 0, terrain_data.heightmapWidth, terrain_data.heightmapHeight);
        int t = 20;

		for (int y = 0; y < terrain_data.heightmapHeight; y++) {
			for (int x = 0; x < terrain_data.heightmapWidth; x++) {

				float currentHeight = noiseMap [x, y];

				if (currentHeight < 0.5f)
					heights [y, x] = heights [y, x] / 1.3f;
			}
            t--;
            if (t <= 0)
            {
                status = "Generating canyons " + y + "/" + terrain_data.heightmapHeight;
                terrain_data.SetHeights(0, 0, heights);
                PlacePlayerOnGround();
                t = 20;
                yield return null;
            }
            
        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();

        noiseMap = Noise.GenerateMap(terrain_data.heightmapWidth, terrain_data.heightmapHeight, seed + 2, noiseScale / 2, octaves + 1, persistance / 2, lacunarity, offset);
        heights = terrain_data.GetHeights(0, 0, terrain_data.heightmapWidth, terrain_data.heightmapHeight);
        t = 20;

        for (int y = 0; y < terrain_data.heightmapHeight; y++)
        {
            for (int x = 0; x < terrain_data.heightmapWidth; x++)
            {

                float currentHeight = noiseMap[x, y];

                if (currentHeight > 0.5f)
                    heights[y, x] = heights[y, x] + currentHeight / 10;
            }
            t--;
            if (t <= 0)
            {
                status = "2nd canyon pass " + y + "/" + terrain_data.heightmapHeight;
                terrain_data.SetHeights(0, 0, heights);
                PlacePlayerOnGround();
                t = 20;
                yield return null;
            }

        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();
    }

    public IEnumerator GenerateErosion(TerrainData terrain_data)
    {

        float[,] noiseMap = Noise.GenerateMap(terrain_data.heightmapWidth, terrain_data.heightmapHeight, seed + 3, noiseScale / 2, octaves + 1, persistance / 2, lacunarity * 4, offset);
        float[,] heights = terrain_data.GetHeights(0, 0, terrain_data.heightmapWidth, terrain_data.heightmapHeight);
        int t = 20;

        for (int y = 0; y < terrain_data.heightmapHeight; y++)
        {
            for (int x = 0; x < terrain_data.heightmapWidth; x++)
            {

                float currentHeight = noiseMap[x, y];

                if (currentHeight > 0.0f && currentHeight < 0.4f)
                    heights[y, x] = (heights[y, x] + currentHeight) / 2;
            }
            t--;
            if (t <= 0)
            {
                status = "Adding erosion " + y + "/" + terrain_data.heightmapHeight;
                terrain_data.SetHeights(0, 0, heights);
                PlacePlayerOnGround();
                t = 20;
                yield return null;
            }

        }

        terrain_data.SetHeights(0, 0, heights);
        terrain.Flush();
    }

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

    void OnGUI()
    {
        if (showStatus)
            GUI.Label(new Rect(10, 10, 300, 50), status);

    }
}