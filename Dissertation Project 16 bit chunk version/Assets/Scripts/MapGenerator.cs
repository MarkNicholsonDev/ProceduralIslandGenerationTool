using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {NoiseMap, Mesh, FalloffMap}
    public DrawMode drawMode;
    
    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetailEditor;
    public Material terrainMaterial;

    public bool autoUpdate;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    //void Awake() {
    //    terrainData.IslandMask = IslandMapGenerator.GenerateIslandMap(mapChunkSize, mapChunkSize, terrainData.falloffStart, terrainData.falloffEnd);
    //}

    void OnValuesUpdated() {
        if (!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        VegetationGenerator vegetationGenerator = FindObjectOfType<VegetationGenerator>();
        //3 drawing modes for generation using an Enum to change between them
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh) {
            //terrainData.IslandMask = IslandMapGenerator.GenerateIslandMap(mapChunkSize, mapChunkSize, terrainData.falloffStart, terrainData.falloffEnd);
            
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(GenerateMapData(Vector2.zero).heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, levelOfDetailEditor));
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(terrainData.IslandMask));
            if (terrainData.useVegetation) { 
                vegetationGenerator.GenerateVegetation(display.meshFilter.sharedMesh, Vector3.zero);
            }
        }
        else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(IslandMapGenerator.GenerateIslandMap(mapChunkSize, mapChunkSize, terrainData.falloffStart, terrainData.falloffEnd)));
        }
    }

    void OnTextureValuesUpdated() {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback) {
        MapData mapData = GenerateMapData(centre);
        // lock means that no other thread can execute the same same bit of code inside until it's turn
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));

        }
    }

    public void RequestMeshData(MapData mapData, int lod,  Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        // lock means that no other thread can execute the same same bit of code inside until it's turn
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));

        }
    }

    void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre ) {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset);
        float[,] vegetationMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseData.seed + 1, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset);
        
        if (terrainData.useIslandGen)
        {
            terrainData.IslandMask = IslandMapGenerator.GenerateIslandMap(mapChunkSize, mapChunkSize, terrainData.falloffStart, terrainData.falloffEnd);
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (terrainData.useIslandGen)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(terrainData.IslandMask[x, y] - noiseMap[x, y]);
                    }
                }
            }
        }
        
       

        //textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        return new MapData(noiseMap, vegetationMap);
    }

    void OnValidate() {
        if (terrainData != null) {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }

        if (noiseData != null) {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureData != null) {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    //<T> makes the value generic so it can hold anything
    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly float[,] vegetationMap;

    public MapData(float[,] heightMap, float[,] vegetationMap) {
        this.heightMap = heightMap;
        this.vegetationMap = vegetationMap;
    }
}
