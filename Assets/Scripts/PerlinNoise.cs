using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    [Header("Tiles")]
    public GameObject deepWaterTile;
    public GameObject waterTile;
    public GameObject sandTile;
    public GameObject grassTile;
    public GameObject mountainTile;
    public GameObject highMountainTile;

    [SerializeField] private float deepWaterRange = 0.1f;
    [SerializeField] private float waterRange = 0.2f;
    [SerializeField] private float mountainRange = 0.6f;

    [SerializeField] private List<GameObject> tiles = new List<GameObject>();

    [Header("Custom Triggers")]
    [Tooltip("What makes the sand trigger near the water, higher number makes it take more grass.")] public float sandTrigger = 0.04f;
    [Tooltip("What makes the high mountains trigger, higher number makes it less likely.")] public float highMountainTrigger = 0.1f;
    private float sandRange = 0.1f;
    private float deepMountainRange = 0f;


    [Header("Variables")]
    public int width = 256;
    public int height = 256;
    public float scale = 5;

    [Header("Offset")]
    [SerializeField] private int offsetX;

    [SerializeField] private int offsetY;

    void Awake()
    {
        sandRange = waterRange + sandTrigger;
        deepMountainRange = mountainRange + highMountainTrigger;
    }

    void Start()
    {
        OffsetPosition();
        GenerateMap();
    }

    private void OffsetPosition()
    {
        offsetX = Random.Range(0, 99999);
        offsetY = Random.Range(0, 99999);
    }

    private void GenerateMap()
    {
        for(int i = 0; i < width; ++i)
        {
            for(int b  = 0; b < height; ++b)
            {
                float xCord = (float)i / width * scale + offsetX;
                float yCord = (float)b / height * scale + offsetY;

                float noise = Mathf.PerlinNoise(xCord, yCord);
                GameObject tileToPlace;

                if(noise < deepWaterRange)
                {
                    tileToPlace = deepWaterTile;
                }
                else if(noise > deepWaterRange && noise < waterRange)
                {
                    tileToPlace = waterTile;
                }
                else if(noise > waterRange && noise < sandRange)
                {
                    tileToPlace = sandTile;
                }
                else if (noise > mountainRange && noise < deepMountainRange)
                {
                    tileToPlace = mountainTile;
                }
                else if(noise > deepMountainRange)
                {
                    tileToPlace = highMountainTile;
                }
                else
                {
                    tileToPlace = grassTile;
                }

                GameObject tile = Instantiate(tileToPlace, new Vector2(i, b), Quaternion.identity);
                tiles.Add(tile);
            }
        }
    }


}
