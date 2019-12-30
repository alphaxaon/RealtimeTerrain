using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(TerrainGenerator))]

public class TerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Terrain"))
        {
            terrainGenerator.GenerateTerrain();
        }
    }
}
