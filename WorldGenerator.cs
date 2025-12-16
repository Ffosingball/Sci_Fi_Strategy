using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

//Job to generate a single row of the waterMap types
public struct WaterMapRowGeneration : IJob 
{
    public NativeArray<WaterTiles> values;
    //0 - y value, 1 - offsetX, 2 - offsetY, 3 - scale
    public NativeArray<float> configurationVals;
    public NativeArray<float> levelVals;

    public void Execute() 
    {
        for (int i = 0; i < values.Length; i++)
        {
            float x = (configurationVals[0] + configurationVals[2])* configurationVals[3];
            float y = (i + configurationVals[1]) * configurationVals[3];
            float sample = Mathf.PerlinNoise(x, y);

            if (sample < levelVals[0])
                values[i] = WaterTiles.DeepWater;
            else if (sample < levelVals[1])
                values[i] = WaterTiles.ShallowWater;
            else if (sample < levelVals[2])
                values[i] = WaterTiles.Beach;
            else
                values[i] = WaterTiles.Land;
        }
    }
}

//Job to generate a single row of the biomesMap types
public struct BiomesMapRowGeneration : IJob
{
    public NativeArray<BiomeTiles> values;
    //0 - y value, 1 - offsetX, 2 - offsetY, 3 - scale
    public NativeArray<float> configurationVals;
    public NativeArray<float> levelVals;

    public void Execute()
    {
        for (int i = 0; i < values.Length; i++)
        {
            float x = (configurationVals[0] + configurationVals[2]) * configurationVals[3];
            float y = (i + configurationVals[1]) * configurationVals[3];
            float sample = Mathf.PerlinNoise(x, y);

            if (sample < levelVals[0])
                values[i] = BiomeTiles.Rock;
            else if (sample < levelVals[1])
                values[i] = BiomeTiles.Steppe;
            else
                values[i] = BiomeTiles.Forest;
        }
    }
}


public class WorldGenerator : MonoBehaviour
{
    public Tilemap tilemap;

    public Tile[] deepWaterTiles;
    public Tile[] shallowWaterTiles;
    public Tile[] beachTiles;
    public Tile[] rockTiles;
    public Tile[] steppeTiles;
    public Tile[] forestTiles;
    public Tile placeholder;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int seed = Random.Range(-99999999,99999999);
        Debug.Log("Seed: "+seed);
        WorldConfigurations worldConfigurations = new WorldConfigurations(100,100,seed);
        GenerateWorld(worldConfigurations);
    }

    //Generate world by given configurations
    void GenerateWorld(WorldConfigurations worldConfigurations)
    {
        System.Random rng = new System.Random(worldConfigurations.seed);

        WaterTiles[][] waterMap = GenerateWaterMap(worldConfigurations, rng);
        BiomeTiles[][] biomeMap = GenerateBiomeMap(worldConfigurations, rng);

        DrawTiles(waterMap, biomeMap, worldConfigurations, rng);
    }


    WaterTiles[][] GenerateWaterMap(WorldConfigurations worldConfigurations, System.Random rng)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<WaterTiles>> results = new List<NativeArray<WaterTiles>>();

        float offsetX = rng.Next(-100000, 100000);
        float offsetY = rng.Next(-100000, 100000);
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            NativeArray<WaterTiles> _values = new NativeArray<WaterTiles>(worldConfigurations.height, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(4, Allocator.Persistent);
            _configVals[0] = x;
            _configVals[1] = offsetX;
            _configVals[2] = offsetY;
            _configVals[3] = worldConfigurations.waterScale;
            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.waterLevels, Allocator.Persistent);

            WaterMapRowGeneration job = new WaterMapRowGeneration { values = _values,configurationVals=_configVals,levelVals=_levelVals };
            jobHandles.Add(job.Schedule());
            results.Add(_values);
        }

        JobHandle.CompleteAll(jobHandles);

        WaterTiles[][] waterMap = new WaterTiles[worldConfigurations.width][];
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            waterMap[x] = results[x].ToArray();
        }

        return waterMap;
    }


    BiomeTiles[][] GenerateBiomeMap(WorldConfigurations worldConfigurations, System.Random rng)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<BiomeTiles>> results = new List<NativeArray<BiomeTiles>>();

        float offsetX = rng.Next(-100000, 100000);
        float offsetY = rng.Next(-100000, 100000);
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            NativeArray<BiomeTiles> _values = new NativeArray<BiomeTiles>(worldConfigurations.height, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(4, Allocator.Persistent);
            _configVals[0] = x;
            _configVals[1] = offsetX;
            _configVals[2] = offsetY;
            _configVals[3] = worldConfigurations.biomeScale;
            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.biomeLevels, Allocator.Persistent);

            BiomesMapRowGeneration job = new BiomesMapRowGeneration { values = _values, configurationVals = _configVals, levelVals = _levelVals };
            jobHandles.Add(job.Schedule());
            results.Add(_values);
        }

        JobHandle.CompleteAll(jobHandles);

        BiomeTiles[][] biomesMap = new BiomeTiles[worldConfigurations.width][];
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            biomesMap[x] = results[x].ToArray();
        }

        return biomesMap;
    }


    void DrawTiles(WaterTiles[][] waterMap, BiomeTiles[][] biomeMap, WorldConfigurations worldConfigurations, System.Random rng)
    {
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            for (int y = 0; y < worldConfigurations.height; y++)
            {
                Tile tile = new Tile();

                switch (waterMap[x][y]) 
                {
                    case WaterTiles.DeepWater:
                        tile = deepWaterTiles[rng.Next(0, deepWaterTiles.Length)];
                        break;
                    case WaterTiles.ShallowWater:
                        tile = shallowWaterTiles[rng.Next(0, shallowWaterTiles.Length)];
                        break;
                    case WaterTiles.Beach:
                        tile = beachTiles[rng.Next(0, beachTiles.Length)];
                        break;
                    case WaterTiles.Land:
                        switch (biomeMap[x][y]) 
                        {
                            case BiomeTiles.Rock:
                                tile = rockTiles[rng.Next(0, beachTiles.Length)];
                                break;
                            case BiomeTiles.Steppe:
                                tile = steppeTiles[rng.Next(0, beachTiles.Length)];
                                break;
                            case BiomeTiles.Forest:
                                tile = forestTiles[rng.Next(0, beachTiles.Length)];
                                break;
                        }
                        break;
                    default:
                        tile = placeholder;
                        break;
                }

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}

