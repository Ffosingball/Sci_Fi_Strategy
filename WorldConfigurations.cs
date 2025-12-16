using UnityEngine;

public class WorldConfigurations
{
    public int width, height;
    //Sea tile - beach tile - biome tiles
    public float[] waterLevels = { 0.35f, 0.4f, 0.43f };
    //Rock tile - steppe tile - forest tile
    public float[] biomeLevels = { 0.35f, 0.65f };
    //World seed
    public int seed;
    public float waterScale = 0.02f;
    public float biomeScale = 0.1f;

    public WorldConfigurations(int width, int height, int seed) 
    {
        this.width = width;
        this.height = height;
        this.seed = seed;
    }
}
