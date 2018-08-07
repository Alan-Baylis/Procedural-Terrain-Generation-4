using UnityEngine;
using System.Collections;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode, bool generateRollway, NoiseData.RollwaySettings rollwaySettings = null) {
        
        float[,] noiseMap = new float[mapWidth, mapHeight];


        System.Random prng = new System.Random (seed);
		Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) - offset.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0) {
			scale = 0.0001f;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y-halfHeight + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				if (normalizeMode == NormalizeMode.Local) {
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				} else {
					float normalizedHeight = (noiseMap [x, y] + 1) / (maxPossibleHeight/0.9f);
					noiseMap [x, y] = Mathf.Clamp(normalizedHeight,0, int.MaxValue);
				}
			}
		}

        if (generateRollway)
        {
            return GenerateRollway(noiseMap, rollwaySettings);
        }

		return noiseMap;
	}

    static float[,] GenerateRollway(float[,] map, NoiseData.RollwaySettings settings)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Vector2 mid = new Vector2(settings.xPos * width, settings.yPos * height);

        float xMax = mid.x + settings.xLenght * width / 2f;
        float xMin = mid.y - settings.xLenght * width / 2f;
        float yMax = mid.y + settings.yLenght * height / 2f;
        float yMin = mid.y - settings.yLenght * height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if ((x > xMin && x < xMax && y > yMin && y < yMax))
                {
                    map[x, y] = 0.5f;
                }
            }
        }

        for (int i = 0; i < settings.smoothIntensity; i++)
        {
            map = SmoothTerrain(map, settings);
        }

        return RoughTerrain((map));
    }

    static float[,] RoughTerrain(float[,] map)
    {
        System.Random rand = new System.Random(1);

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if(map[x,y] >= 0.25f)
                    map[x, y] += rand.Next(0, 1000) / 1000f * 0.025f;
            }
        }

        return map;
    }

    static float[,] SmoothTerrain(float[,] map, NoiseData.RollwaySettings settings)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Vector2 mid = new Vector2(settings.xPos * width, settings.yPos * height);

        float xMax = mid.x + settings.xLenght * width / 2f;
        float xMin = mid.y - settings.xLenght * width / 2f;
        float yMax = mid.y + settings.yLenght * height / 2f;
        float yMin = mid.y - settings.yLenght * height / 2f;

        int smoothRadius = settings.smoothRadius;

        for (int y = 1; y < height; y++)
        {
            for (int x = 1; x < width; x++)
            {
                int delta = smoothRadius;
                float xMaxDelta = Mathf.Abs(xMax - x);
                float yMaxDelta = Mathf.Abs(yMax - y);
                float xMinDelta = Mathf.Abs(xMin - x);
                float yMinDelta = Mathf.Abs(yMin - y);

                if (((xMaxDelta < delta || xMinDelta < delta) && (y < yMax) && y > yMin) || ((yMaxDelta < delta || yMinDelta < delta) && (x < xMax) && x > xMin))
                {

                    float avgHeight = 0;

                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            avgHeight += map[x + i, y + j];
                        }
                    }

                    avgHeight /= 9;

                    map[x, y] = avgHeight;
                }
            }
        }
    
        return map;
    }
}
