using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.DebugUI;


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
            float y = (configurationVals[0] + configurationVals[2])* configurationVals[3];
            float x = (i + configurationVals[1]) * configurationVals[3];
            float sample = Mathf.PerlinNoise(x, y);

            if (sample < levelVals[0])
                values[i] = WaterTiles.Water;
            else if (sample < levelVals[1])
                values[i] = WaterTiles.Beach;
            else
                values[i] = WaterTiles.Land;
        }
    }
}


public class WorldGenerator : MonoBehaviour
{
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
        WaterTiles[][] waterMap = GenerateWaterMap(worldConfigurations);

        for (int i = 0; i < worldConfigurations.height; i++) 
        {
            for (int j = 0; j < worldConfigurations.width; j++)
            {
                switch (waterMap[i][j]) 
                {
                    case WaterTiles.Water:
                        Debug.Log("water");
                        break;
                    case WaterTiles.Beach:
                        Debug.Log("beach");
                        break;
                    case WaterTiles.Land:
                        Debug.Log("land");
                        break;
                }
            }
        }

        //BiomeTiles[] biomeMap = GenerateBiomeMap(worldConfigurations);
    }


    WaterTiles[][] GenerateWaterMap(WorldConfigurations worldConfigurations)
    {
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        List<NativeArray<WaterTiles>> results = new List<NativeArray<WaterTiles>>();

        for (int i = 0; i < worldConfigurations.height; i++)
        {
            NativeArray<WaterTiles> _values = new NativeArray<WaterTiles>(worldConfigurations.width, Allocator.Persistent);
            NativeArray<float> _configVals = new NativeArray<float>(4, Allocator.Persistent);
            System.Random rng = new System.Random(worldConfigurations.seed);
            float offsetX = rng.Next(-100000, 100000);
            float offsetY = rng.Next(-100000, 100000);
            _configVals[0] = i;
            _configVals[1] = offsetX;
            _configVals[2] = offsetY;
            _configVals[3] = worldConfigurations.scale;
            NativeArray<float> _levelVals = new NativeArray<float>(worldConfigurations.waterLevels, Allocator.Persistent);

            WaterMapRowGeneration job = new WaterMapRowGeneration { values = _values,configurationVals=_configVals,levelVals=_levelVals };
            jobHandles.Add(job.Schedule());
            results.Add(_values);
        }

        JobHandle.CompleteAll(jobHandles);

        WaterTiles[][] waterMap = new WaterTiles[worldConfigurations.height][];
        for (int i = 0; i < worldConfigurations.height; i++)
        {
            waterMap[i] = results[i].ToArray();
        }

        return waterMap;
    }
}
