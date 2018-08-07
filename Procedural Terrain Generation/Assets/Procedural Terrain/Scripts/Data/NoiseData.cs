using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData {

    public Noise.NormalizeMode normalizeMode;

    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public RollwaySettings rollwaySettings;

    protected override void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;

        if (octaves < 0)
            octaves = 1;

        base.OnValidate();
    }

    [System.Serializable]
    public class RollwaySettings
    {
        public bool generateRollway;
        [Range(0, 1)]
        public float xPos;
        [Range(0, 1)]
        public float yPos;
        [Range(0, 1)]
        public float xLenght;
        [Range(0, 1)]
        public float yLenght;
        [Range(1, 10)]
        public int smoothRadius;
        [Range(0, 3)]
        public int smoothIntensity;
    }
}
