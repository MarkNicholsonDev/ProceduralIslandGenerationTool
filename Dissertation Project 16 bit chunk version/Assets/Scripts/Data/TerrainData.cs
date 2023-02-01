using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData {
    public int mapWidth;
    public int mapHeight;

    public int maskWidth;
    public int maskHeight;

    public float[,] IslandMask;
    [Range(0, 1)]
    public float falloffStart;
    [Range(0, 1)]
    public float falloffEnd;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public VegetationLayer[] vegetationLayers;

    public bool useIslandGen;
    public bool useVegetation;
    public bool vegetationPlaced;

    public float minHeight {
        get {
            return meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }

    public float maxHeight {
        get {
            return meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }

    protected override void OnValidate() {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }

        if (mapHeight < 1)
        {
            mapHeight = 1;
        }

        base.OnValidate();
    }

    //Makes the class appear in the inspector
    [System.Serializable]
    public class VegetationLayer {
        public GameObject vegetationObj;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float endHeight;
        [Range(0, 1)]
        public float vegetationDensity;
    }
}
