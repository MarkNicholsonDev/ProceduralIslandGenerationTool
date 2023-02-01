using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationGenerator : MonoBehaviour {
    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public void GenerateVegetation(Mesh terrainMesh, Vector2 meshPos) {

        //For every vertice within the mesh
        Vector3[] vertices = terrainMesh.vertices;
        Vector3 meshPosV3 = new Vector3(meshPos.x, 0, meshPos.y);

        if (!Application.isPlaying) {
            if (terrainData.vegetationPlaced)
            {
                Debug.Log(transform.childCount);
                foreach (Transform childVeg in transform)
                {
                    Debug.Log("Previous Vegetation deleted");
                    DestroyImmediate(childVeg.gameObject);
                }

                terrainData.vegetationPlaced = false;
            }
        }

        for (int i = 0; i < vertices.Length; i++) {
            Debug.Log("No. of layers:" + terrainData.vegetationLayers.Length);
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            float heightPercent = Mathf.InverseLerp(terrainData.minHeight, terrainData.maxHeight, worldPos.y);
            for (int j = 0; j < terrainData.vegetationLayers.Length; j++) {
                if (heightPercent > terrainData.vegetationLayers[j].startHeight && heightPercent < terrainData.vegetationLayers[j].endHeight) {
                    float randomChance = Random.Range(0.0f, 1.0f);
                    if (randomChance > terrainData.vegetationLayers[j].vegetationDensity) {
                        Instantiate(terrainData.vegetationLayers[j].vegetationObj, worldPos + meshPosV3, Quaternion.identity, transform);
                    }
                }
            }
        }

        terrainData.vegetationPlaced = true;
    }
}
