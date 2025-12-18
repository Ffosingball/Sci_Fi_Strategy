using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.STP;


//Job to generate a single row of the waterMap types
public struct SimpleWaterMapRowGeneration : IJob
{
    public NativeArray<WaterTiles> values;
    //0 - x value, 1 - offsetY, 2 - offsetX, 3 - scale
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


public struct AdvancedWaterMapRowGeneration : IJob
{
    public NativeArray<WaterTiles> values;
    //0 - x value, 1 - offsetY, 2 - offsetX, 3 - scale
    //4 - isleOffsetX, 5 - isleOffestY, 6 - isleScale
    //7 - lakeOffsetX, 8 - lakeOffestY, 9 - lakeScale
    public NativeArray<float> configurationVals;
    public NativeArray<float> levelVals;

    public void Execute()
    {
        for (int i = 0; i < values.Length; i++)
        {
            float x = (configurationVals[0] + configurationVals[2]) * configurationVals[3];
            float y = (i + configurationVals[1]) * configurationVals[3];
            float sample = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[5]) * configurationVals[6];
            y = (i + configurationVals[4]) * configurationVals[6];
            float sample2 = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[8]) * configurationVals[9];
            y = (i + configurationVals[7]) * configurationVals[9];
            float sample3 = Mathf.PerlinNoise(x, y);

            float finalSample = sample + sample2 - sample3;

            if (finalSample < levelVals[0])
                values[i] = WaterTiles.DeepWater;
            else if (finalSample < levelVals[1])
                values[i] = WaterTiles.ShallowWater;
            else if (finalSample < levelVals[2])
                values[i] = WaterTiles.Beach;
            else
            {
                values[i] = WaterTiles.Land;
             
            }
        }
    }
}

//Job to generate a single row of the biomesMap types
public struct BiomesMapRowGeneration : IJob
{
    public NativeArray<BiomeTiles> values;
    //0 - x value, 1 - offsetY, 2 - offsetX, 3 - scale
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
    //public Tilemap tilemap;
    public GameObject chunkPrefab;
    public GameObject grid;

    public Tile[] deepWaterTiles;
    public Tile[] shallowWaterTiles;
    public Tile[] beachTiles;
    public Tile[] rockTiles;
    public Tile[] steppeTiles;
    public Tile[] forestTiles;
    public Tile placeholder;

    private WorldConfigurations worldConfigurations;
    private System.Random rng;
    float waterOffsetX;
    float waterOffsetY;
    float isleOffsetX;
    float isleOffsetY;
    float lakeOffsetX;
    float lakeOffsetY;
    float biomeOffsetX;
    float biomeOffsetY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int seed = Random.Range(-99999999,99999999);
        Debug.Log("Seed: "+seed);
        worldConfigurations = new WorldConfigurations(seed);
        rng = new System.Random(worldConfigurations.seed);

        waterOffsetX = rng.Next(-100000, 100000);
        waterOffsetY = rng.Next(-100000, 100000);
        isleOffsetX = rng.Next(-100000, 100000);
        isleOffsetY = rng.Next(-100000, 100000);
        lakeOffsetX = rng.Next(-100000, 100000);
        lakeOffsetY = rng.Next(-100000, 100000);
        biomeOffsetX = rng.Next(-100000, 100000);
        biomeOffsetY = rng.Next(-100000, 100000);
        //GenerateWorld();
    }

    //Generate world by given configurations
    /*void GenerateWorld()
    {
        WaterTiles[][] waterMap = GenerateWaterMap();
        BiomeTiles[][] biomeMap = GenerateBiomeMap();

        DrawTiles(waterMap, biomeMap);
    }*/


    public GameObject GenerateChunk(Vector2 position, Vector2Int chunkSizes)
    {
        Debug.Log("Generate chunk!");
        GameObject newChunk = Instantiate(chunkPrefab,new Vector3(position.x,position.y-(chunkSizes.y/4),0f),Quaternion.identity);
        newChunk.transform.SetParent(grid.transform);
        //newChunk.name = "C" + position.x + ";" + position.y;

        Vector2 isometricPosition = new Vector2(position.x+(2*position.y),(position.y*2)-position.x);

        WaterTiles[][] waterMap = GenerateWaterMap(isometricPosition, chunkSizes);
        BiomeTiles[][] biomeMap = GenerateBiomeMap(isometricPosition, chunkSizes);

        DrawTiles(waterMap, biomeMap, newChunk, chunkSizes);

        return newChunk;
    }


    WaterTiles[][] GenerateWaterMap(Vector2 position, Vector2Int chunkSizes)
    {
        Debug.Log("World Pos: "+position.x+";"+position.y);
        Debug.Log("Offsets: " + waterOffsetX + ";" + waterOffsetY);
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<WaterTiles>> results = new List<NativeArray<WaterTiles>>();
        List<NativeArray<float>> configs = new List<NativeArray<float>>();

        Debug.Log("X: "+(position.x + waterOffsetX) * worldConfigurations.waterScale);
        Debug.Log("Y: "+(waterOffsetY + position.y) * worldConfigurations.waterScale);

        for (int x = 0; x < chunkSizes.x; x++)
        {
            NativeArray<WaterTiles> _values = new NativeArray<WaterTiles>(chunkSizes.y, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(10, Allocator.TempJob);
            _configVals[0] = x+position.x;
            _configVals[1] = waterOffsetY+position.y;
            _configVals[2] = waterOffsetX;
            _configVals[3] = worldConfigurations.waterScale;
            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.waterLevels, Allocator.TempJob);

            if (worldConfigurations.advancedGeneration)
            {
                //Debug.Log("Advanced");
                _configVals[4] = isleOffsetX;
                _configVals[5] = isleOffsetY;
                _configVals[6] = worldConfigurations.isleScale;
                _configVals[7] = lakeOffsetX;
                _configVals[8] = lakeOffsetY;
                _configVals[9] = worldConfigurations.lakeScale;
                AdvancedWaterMapRowGeneration job = new AdvancedWaterMapRowGeneration { values = _values, configurationVals = _configVals, levelVals = _levelVals };
                jobHandles.Add(job.Schedule());
            }
            else
            {
                //Debug.Log("Simple");
                SimpleWaterMapRowGeneration job = new SimpleWaterMapRowGeneration { values = _values, configurationVals = _configVals, levelVals = _levelVals };
                jobHandles.Add(job.Schedule());
            }

            configs.Add(_configVals);
            configs.Add(_levelVals);
            results.Add(_values);
        }

        JobHandle.CompleteAll(jobHandles);

        WaterTiles[][] waterMap = new WaterTiles[chunkSizes.x][];
        for (int x = 0; x < chunkSizes.x; x++)
        {
            waterMap[x] = results[x].ToArray();
            results[x].Dispose();
            configs[x * 2].Dispose();
            configs[(x * 2)+1].Dispose();
        }

        jobHandles.Dispose();
        return waterMap;
    }


    BiomeTiles[][] GenerateBiomeMap(Vector2 position, Vector2Int chunkSizes)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<BiomeTiles>> results = new List<NativeArray<BiomeTiles>>();
        List<NativeArray<float>> configs = new List<NativeArray<float>>();

        for (int x = 0; x < chunkSizes.x; x++)
        {
            NativeArray<BiomeTiles> _values = new NativeArray<BiomeTiles>(chunkSizes.y, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(4, Allocator.TempJob);
            _configVals[0] = x+position.x;
            _configVals[1] = biomeOffsetY+position.y;
            _configVals[2] = biomeOffsetX;
            _configVals[3] = worldConfigurations.biomeScale;
            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.biomeLevels, Allocator.TempJob);

            BiomesMapRowGeneration job = new BiomesMapRowGeneration { values = _values, configurationVals = _configVals, levelVals = _levelVals };
            jobHandles.Add(job.Schedule());

            configs.Add(_configVals);
            configs.Add(_levelVals);
            results.Add(_values);
        }

        JobHandle.CompleteAll(jobHandles);

        BiomeTiles[][] biomesMap = new BiomeTiles[chunkSizes.x][];
        for (int x = 0; x < chunkSizes.x; x++)
        {
            biomesMap[x] = results[x].ToArray();
            results[x].Dispose();
            configs[x * 2].Dispose();
            configs[(x * 2) + 1].Dispose();
        }

        jobHandles.Dispose();
        return biomesMap;
    }


    void DrawTiles(WaterTiles[][] waterMap, BiomeTiles[][] biomeMap, GameObject newChunk, Vector2Int chunkSizes)
    {
        Tilemap tilemap = newChunk.GetComponent<Tilemap>();

        for (int x = 0; x < chunkSizes.x; x++)
        {
            for (int y = 0; y < chunkSizes.y; y++)
            {
                Tile tile = new Tile();

                if (x == 0 && y == 0)
                    tile = placeholder;
                else
                {
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
                }

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}

