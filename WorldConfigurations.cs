using UnityEngine;

public class WorldConfigurations
{
    public int width, height;
    //Sea tile - beach tile - biome tiles
    public float[] waterLevels = { 0.28f, 0.37f, 0.45f };
    //Rock tile - steppe tile - forest tile
    public float[] biomeLevels = { 0.3f, 0.55f };
    //World seed
    public int seed;
    public float waterScale = 0.007f;
    public float biomeScale = 0.06f;
    public float isleScale = 0.04f;
    public float lakeScale = 0.03f;
    public bool advancedGeneration = true;
    //Iron, Bauxite, Oil, Coal, REE, Copper, Uranium
    public float[] oreScales = {0.05f,0.06f,0.08f,0.07f,0.095f,0.09f,0.092f};
    //From Iron to Uranium
    public float[] oreLevels = {0.75f,0.77f,0.83f,0.8f,0.86f,0.85f,0.87f};

    public WorldConfigurations(int width, int height, int seed) 
    {
        this.width = width;
        this.height = height;
        this.seed = seed;
    }
}
