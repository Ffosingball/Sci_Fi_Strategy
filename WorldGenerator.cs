using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;


//Job to generate a single row of the waterMap types
public struct SimpleWaterMapRowGeneration : IJob
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


public struct AdvancedWaterMapRowGeneration : IJob
{
    public NativeArray<WaterTiles> values;
    //0 - y value, 1 - offsetX, 2 - offsetY, 3 - scale
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

    private WaterTiles[][] waterMap;
    private BiomeTiles[][] biomeMap;

    private bool finishedWaterMap = false;
    private bool finishedBiomeMap = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int seed = Random.Range(-99999999,99999999);
        Debug.Log("Seed: "+seed);
        WorldConfigurations worldConfigurations = new WorldConfigurations(1000,1000,seed);

        waterMap = new WaterTiles[worldConfigurations.width][];
        biomeMap = new BiomeTiles[worldConfigurations.width][];

        StartCoroutine(GenerateWorld(worldConfigurations));
    }

    //Generate world by given configurations
    private IEnumerator GenerateWorld(WorldConfigurations worldConfigurations)
    {
        System.Random rng = new System.Random(worldConfigurations.seed);

        StartCoroutine(GenerateWaterMap(worldConfigurations, rng));
        while (!finishedWaterMap)
        {
            yield return null;
        }

        StartCoroutine(GenerateBiomeMap(worldConfigurations, rng));
        while (!finishedBiomeMap)
        {
            yield return null;
        }

        StartCoroutine(DrawTiles(worldConfigurations, rng));
    }


    private IEnumerator GenerateWaterMap(WorldConfigurations worldConfigurations, System.Random rng)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<WaterTiles>> results = new List<NativeArray<WaterTiles>>();
        List<NativeArray<float>> configs = new List<NativeArray<float>>();

        float offsetX = rng.Next(-100000, 100000);
        float offsetY = rng.Next(-100000, 100000);
        float isleOffsetX = rng.Next(-100000, 100000);
        float isleOffsetY = rng.Next(-100000, 100000);
        float lakeOffsetX = rng.Next(-100000, 100000);
        float lakeOffsetY = rng.Next(-100000, 100000);
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            NativeArray<WaterTiles> _values = new NativeArray<WaterTiles>(worldConfigurations.height, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(10, Allocator.TempJob);
            _configVals[0] = x;
            _configVals[1] = offsetX;
            _configVals[2] = offsetY;
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

        //JobHandle.CompleteAll(jobHandles);

        for (int x = 0; x < worldConfigurations.width; x++)
        {
            if (jobHandles[x].IsCompleted)
            {
                jobHandles[x].Complete();
                waterMap[x] = results[x].ToArray();
                results[x].Dispose();
                configs[x * 2].Dispose();
                configs[(x * 2) + 1].Dispose();
            }
            else
            {
                while (!jobHandles[x].IsCompleted)
                {
                    yield return null;
                }

                jobHandles[x].Complete();
                waterMap[x] = results[x].ToArray();
                results[x].Dispose();
                configs[x * 2].Dispose();
                configs[(x * 2) + 1].Dispose();
            }
        }

        jobHandles.Dispose();
        finishedWaterMap = true;
    }


    private IEnumerator GenerateBiomeMap(WorldConfigurations worldConfigurations, System.Random rng)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<BiomeTiles>> results = new List<NativeArray<BiomeTiles>>();
        List<NativeArray<float>> configs = new List<NativeArray<float>>();

        float offsetX = rng.Next(-100000, 100000);
        float offsetY = rng.Next(-100000, 100000);
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            NativeArray<BiomeTiles> _values = new NativeArray<BiomeTiles>(worldConfigurations.height, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(4, Allocator.TempJob);
            _configVals[0] = x;
            _configVals[1] = offsetX;
            _configVals[2] = offsetY;
            _configVals[3] = worldConfigurations.biomeScale;
            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.biomeLevels, Allocator.TempJob);

            BiomesMapRowGeneration job = new BiomesMapRowGeneration { values = _values, configurationVals = _configVals, levelVals = _levelVals };
            jobHandles.Add(job.Schedule());

            configs.Add(_configVals);
            configs.Add(_levelVals);
            results.Add(_values);
        }

        //JobHandle.CompleteAll(jobHandles);

        for (int x = 0; x < worldConfigurations.width; x++)
        {
            if (jobHandles[x].IsCompleted)
            {
                jobHandles[x].Complete();
                biomeMap[x] = results[x].ToArray();
                results[x].Dispose();
                configs[x * 2].Dispose();
                configs[(x * 2) + 1].Dispose();
            }
            else 
            {
                while (!jobHandles[x].IsCompleted) 
                {
                    yield return null;
                }

                jobHandles[x].Complete();
                biomeMap[x] = results[x].ToArray();
                results[x].Dispose();
                configs[x * 2].Dispose();
                configs[(x * 2) + 1].Dispose();
            }
        }

        jobHandles.Dispose();
        finishedBiomeMap = true;
    }


    private IEnumerator DrawTiles(WorldConfigurations worldConfigurations, System.Random rng)
    {
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            Vector3Int[] positions = new Vector3Int[worldConfigurations.height];
            TileBase[] tiles = new TileBase[worldConfigurations.height];

            for (int y = 0; y < worldConfigurations.height; y++)
            {
                //Tile tile = new Tile();

                switch (waterMap[x][y]) 
                {
                    case WaterTiles.DeepWater:
                        tiles[y] = deepWaterTiles[rng.Next(0, deepWaterTiles.Length)];
                        break;
                    case WaterTiles.ShallowWater:
                        tiles[y] = shallowWaterTiles[rng.Next(0, shallowWaterTiles.Length)];
                        break;
                    case WaterTiles.Beach:
                        tiles[y] = beachTiles[rng.Next(0, beachTiles.Length)];
                        break;
                    case WaterTiles.Land:
                        switch (biomeMap[x][y]) 
                        {
                            case BiomeTiles.Rock:
                                tiles[y] = rockTiles[rng.Next(0, beachTiles.Length)];
                                break;
                            case BiomeTiles.Steppe:
                                tiles[y] = steppeTiles[rng.Next(0, beachTiles.Length)];
                                break;
                            case BiomeTiles.Forest:
                                tiles[y] = forestTiles[rng.Next(0, beachTiles.Length)];
                                break;
                        }
                        break;
                    default:
                        tiles[y] = placeholder;
                        break;
                }

                positions[y] = new Vector3Int(x, y, 0);
                //tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }

            tilemap.SetTiles(positions,tiles);
            yield return null;
        }
    }
}

