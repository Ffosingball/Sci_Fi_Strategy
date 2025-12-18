using UnityEngine;

public class WorldConfigurations
{
    //public int width, height;
    //Sea tile - beach tile - biome tiles
    public float[] waterLevels = { 0.28f, 0.37f, 0.45f };
    //Rock tile - steppe tile - forest tile
    public float[] biomeLevels = { 0.3f, 0.55f };
    //World seed
    public int seed;
    public float waterScale = 0.02f;
    public float biomeScale = 0.1f;
    public float isleScale = 0.08f;
    public float lakeScale = 0.06f;
    public bool advancedGeneration = true;

    public WorldConfigurations(int seed) 
    {
        this.seed = seed;
    }
}
