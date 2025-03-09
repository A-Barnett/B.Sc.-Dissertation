using UnityEngine;
using Random = UnityEngine.Random;


public class PerlinNoise : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int seed;
    [SerializeField] private float scale;
    [SerializeField] private bool randomSeed;
    private FastNoiseLite fastNoiseLite;
    public float[,] noiseData;
    
    void Start()
    {
        fastNoiseLite = new FastNoiseLite();
        noiseData = new float[width, height];
        if (randomSeed)
        {
            seed = Random.Range(0, 100000);
        }
        CalculateNoise();
    }

    void CalculateNoise()
    {
        fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        fastNoiseLite.SetSeed(seed);
        fastNoiseLite.SetFractalType(FastNoiseLite.FractalType.FBm);
        fastNoiseLite.SetFrequency(scale);
        fastNoiseLite.SetFractalOctaves(5);
        fastNoiseLite.SetFractalLacunarity(1.6f);
        fastNoiseLite.SetFractalGain(0.46f);
        fastNoiseLite.SetFractalWeightedStrength(-0.23f);
        int y = 0;
        while (y < width)
        {
            int x = 0;
            while (x < height)
            {
                float sample = fastNoiseLite.GetNoise(x, y);
                noiseData[x, y] = sample;
                x++;
            }
            y++;
        }
    }

}
