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

//Three levels of row generation:
//1st - iron and bauxite
//2nd - coal and oil
//3rd - copper, rare earth el., uranium
public struct OreMapRowGeneration : IJob
{
    public NativeArray<OreTiles> values;
    //0 - y value, 1 - offset1X, 2 - offset1Y, 3 - offset2X, 
    //4 - offest2Y, 5 - offset3X, 6 - offest3Y,
    //7 - scaleIron, 8 - scaleBauxite, 9 - scaleOil,
    //10 - scaleCoal, 11 - scaleREE, 12 - scaleCopper, 13 - scaleUranium
    public NativeArray<float> configurationVals;
    //0 - Iron, 1 - Bauxite, 2 - Oil
    //3 - Coal, 4 - REE, 5 - Copper, 6 - Uranium
    public NativeArray<float> levelVals;

    public void Execute()
    {
        for (int i = 0; i < values.Length; i++)
        {
            float x = (configurationVals[0] + configurationVals[2]) * configurationVals[7];
            float y = (i + configurationVals[1]) * configurationVals[7];
            float sample = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[2]) * configurationVals[8];
            y = (i + configurationVals[1]) * configurationVals[8];
            float sample2 = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[4]) * configurationVals[9];
            y = (i + configurationVals[3]) * configurationVals[9];
            float sample3 = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[4]) * configurationVals[10];
            y = (i + configurationVals[3]) * configurationVals[10];
            float sample4 = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[6]) * configurationVals[11];
            y = (i + configurationVals[5]) * configurationVals[11];
            float sample5 = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[6]) * configurationVals[12];
            y = (i + configurationVals[5]) * configurationVals[12];
            float sample6 = Mathf.PerlinNoise(x, y);

            x = (configurationVals[0] + configurationVals[6]) * configurationVals[13];
            y = (i + configurationVals[5]) * configurationVals[13];
            float sample7 = Mathf.PerlinNoise(x, y);

            if (sample > levelVals[0])
            {
                values[i] = OreTiles.Iron;
            }
            else if (sample2 > levelVals[1])
            {
                values[i] = OreTiles.Bauxite;
            }
            else if (sample4 > levelVals[3])
            {
                values[i] = OreTiles.Coal;
            }
            else if (sample3 > levelVals[2])
            {
                values[i] = OreTiles.Oil;
            }
            else if (sample6 > levelVals[5])
            {
                values[i] = OreTiles.Copper;
            }
            else if (sample5 > levelVals[4])
            {
                values[i] = OreTiles.REE;
            }
            else if (sample7 > levelVals[6])
            {
                values[i] = OreTiles.Uranium;
            }
            else
            {
                values[i] = OreTiles.None;
            }
        }
    }
}


public class WorldGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap oreTilemap;

    public Tile[] deepWaterTiles;
    public Tile[] shallowWaterTiles;
    public Tile[] beachTiles;
    public Tile[] rockTiles;
    public Tile[] steppeTiles;
    public Tile[] forestTiles;
    public Tile placeholder;
    //iron, bauxite, oil, coal, rare earth elements, copper, uranium
    public Tile[] oreTiles;

    private WaterTiles[][] waterMap;
    private BiomeTiles[][] biomeMap;
    private OreTiles[][] oreMap;

    private bool finishedWaterMap = false;
    private bool finishedBiomeMap = false;
    private bool finishedDrawingWorld = false;
    private bool finishedOreMap = false;
    private bool finishedDrawingOres = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int seed = Random.Range(-99999999,99999999);
        Debug.Log("Seed: "+seed);
        WorldConfigurations worldConfigurations = new WorldConfigurations(1000,1000,seed);

        waterMap = new WaterTiles[worldConfigurations.width][];
        biomeMap = new BiomeTiles[worldConfigurations.width][];
        oreMap = new OreTiles[worldConfigurations.width][];

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
        Debug.Log("Generated water map");

        StartCoroutine(GenerateBiomeMap(worldConfigurations, rng));
        while (!finishedBiomeMap)
        {
            yield return null;
        }
        Debug.Log("Generated biome map");

        StartCoroutine(DrawTiles(worldConfigurations, rng));
        while (!finishedDrawingWorld)
        {
            yield return null;
        }
        Debug.Log("Drew tiles");

        StartCoroutine(GenerateOreMap(worldConfigurations, rng));
        while (!finishedOreMap)
        {
            yield return null;
        }
        Debug.Log("Generated ore map");

        StartCoroutine(DrawOreTiles(worldConfigurations, rng));
        while (!finishedDrawingOres)
        {
            yield return null;
        }
        Debug.Log("Drew ores");
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


    private IEnumerator GenerateOreMap(WorldConfigurations worldConfigurations, System.Random _rng)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<OreTiles>> results = new List<NativeArray<OreTiles>>();
        List<NativeArray<float>> configs = new List<NativeArray<float>>();

        float offset1X = _rng.Next(-100000, 100000);
        float offset1Y = _rng.Next(-100000, 100000);
        float offset2X = _rng.Next(-100000, 100000);
        float offset2Y = _rng.Next(-100000, 100000);
        float offset3X = _rng.Next(-100000, 100000);
        float offset3Y = _rng.Next(-100000, 100000);
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            NativeArray<OreTiles> _values = new NativeArray<OreTiles>(worldConfigurations.height, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(14, Allocator.TempJob);

            _configVals[0] = x;
            _configVals[1] = offset1X;
            _configVals[2] = offset1Y;
            _configVals[3] = offset2X;
            _configVals[4] = offset2Y;
            _configVals[5] = offset3X;
            _configVals[6] = offset3Y;
            _configVals[7] = worldConfigurations.oreScales[0];
            _configVals[8] = worldConfigurations.oreScales[1];
            _configVals[9] = worldConfigurations.oreScales[2];
            _configVals[10] = worldConfigurations.oreScales[3];
            _configVals[11] = worldConfigurations.oreScales[4];
            _configVals[12] = worldConfigurations.oreScales[5];
            _configVals[13] = worldConfigurations.oreScales[6];

            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.oreLevels, Allocator.TempJob);

            OreMapRowGeneration job = new OreMapRowGeneration { values = _values, configurationVals = _configVals, levelVals = _levelVals };
            jobHandles.Add(job.Schedule());

            configs.Add(_configVals);
            configs.Add(_levelVals);
            results.Add(_values);
        }

        //JobHandle.CompleteAll(jobHandles);
        //Debug.Log("Len: "+oreMap.Length);

        for (int x = 0; x < worldConfigurations.width; x++)
        {
            if (jobHandles[x].IsCompleted)
            {
                jobHandles[x].Complete();
                oreMap[x] = results[x].ToArray();
                results[x].Dispose();
                configs[x * 2].Dispose();
                configs[(x * 2) + 1].Dispose();

                //Debug.Log("L: "+oreMap[x].Length);
            }
            else 
            {
                while (!jobHandles[x].IsCompleted) 
                {
                    yield return null;
                }

                jobHandles[x].Complete();
                oreMap[x] = results[x].ToArray();
                results[x].Dispose();
                configs[x * 2].Dispose();
                configs[(x * 2) + 1].Dispose();

                //Debug.Log("L: "+oreMap[x].Length);
            }
        }

        jobHandles.Dispose();
        finishedOreMap = true;
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
            //Debug.Log(x);
            yield return null;
        }

        finishedDrawingWorld = true;
    }

    private IEnumerator DrawOreTiles(WorldConfigurations worldConfigurations, System.Random rng)
    {
        for (int x = 0; x < worldConfigurations.width; x++)
        {
            Vector3Int[] positions = new Vector3Int[worldConfigurations.height];
            TileBase[] tiles = new TileBase[worldConfigurations.height];
            //Debug.Log(x+", "+oreMap.Length+", "+oreMap[x].Length);

            for (int y = 0; y < worldConfigurations.height; y++)
            {
                switch (oreMap[x][y]) 
                {
                    case OreTiles.Iron:
                        tiles[y] = oreTiles[0];
                        break;
                    case OreTiles.Bauxite:
                        tiles[y] = oreTiles[1];
                        break;
                    case OreTiles.Oil:
                        tiles[y] = oreTiles[2];
                        break;
                    case OreTiles.Coal:
                        tiles[y] = oreTiles[3];
                        break;
                    case OreTiles.REE:
                        tiles[y] = oreTiles[4];
                        break;
                    case OreTiles.Copper:
                        tiles[y] = oreTiles[5];
                        break;
                    case OreTiles.Uranium:
                        tiles[y] = oreTiles[6];
                        break;
                    default:
                        tiles[y] = null;
                        break;
                }

                positions[y] = new Vector3Int(x, y, 0);
                //tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }

            oreTilemap.SetTiles(positions,tiles);
            //Debug.Log(x);
            yield return null;
        }

        finishedDrawingOres=true;
    }
}

