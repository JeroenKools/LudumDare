using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generateMap : MonoBehaviour
{

    public int mapSize;
    public float waterRatio;
    public GameObject tilePrefab, waterTilePrefab;
    public Rect drawBound;
    public float nDunes;
    public float maxDuneHeight;
    public float minDuneWidth;
    public float maxDuneWidth;
    public float duneUniformNoise;
    
    private int _mapSize;
    private GameObject[,] waterTiles, landTiles;
    private GameObject waterTileHolder, landTileHolder;
    private float seafloorY;
    private int pinsPerTile = 0;

    void Start ()
    {
        if (Application.isPlaying) {
            _mapSize = mapSize;
        } else {
            _mapSize = 6;
        }

        waterTiles = new GameObject[_mapSize, _mapSize];
        landTiles = new GameObject[_mapSize, _mapSize];
        seafloorY = -(GameManager.instance.slopeZ + GameManager.instance.slopeX) * waterRatio * _mapSize;

        // Delete existing water tiles
        Transform waterTilesT = transform.FindChild ("Water");
        if (waterTilesT != null) {
            DestroyImmediate (waterTilesT.gameObject);
        }
        waterTileHolder = new GameObject ("Water");
        waterTileHolder.transform.parent = transform;

        // Delete existing land tiles
        Transform landTilesT = transform.FindChild ("Land");
        if (landTilesT != null) {
            DestroyImmediate (landTilesT.gameObject);
        }

        landTileHolder = new GameObject ("Land");
        landTileHolder.transform.parent = transform;

        Camera.main.transform.position = new Vector3 (-_mapSize / 2, _mapSize * 1.75f, -_mapSize / 2);
        Build ();
    }

    void Build ()
    {
        CreateTerrain ();
        AddDunes (); 
        MakeWaves ();
    }
	

    void Update ()
    {        
        PropagateWaves ();
    }


    void CreateTerrain ()
    {
        print (string.Format ("Generating {0}x{1} beach...", _mapSize, _mapSize));
        float landLimit = _mapSize * waterRatio;
        Vector3 location, screenPos;
        
        for (int x=0; x<_mapSize; x++) {
            for (int z=0; z<_mapSize; z++) {
                
                // Create land
                location = new Vector3 (x, seafloorY + GameManager.instance.slopeX * x + GameManager.instance.slopeZ * z, z);
                screenPos = Camera.main.WorldToViewportPoint (location);
                // Only actually instantiate the tile if it would be on screen. This is in order to clip the points off of the diamond map.
                if (drawBound.Contains (screenPos)) {
                    GameObject tile = (GameObject)Instantiate (tilePrefab, location, transform.rotation);
                    if (pinsPerTile == 0) {
                        pinsPerTile = tile.GetComponent<generatePins> ().pinsPerTile;
                    }                    
                    tile.name = "Land tile " + x + ";" + z;
                    tile.transform.parent = landTileHolder.transform;
                    landTiles [x, z] = tile;
                }
                
                // Create water
                if (z < landLimit + 2) {
                    location = new Vector3 (x, 0, z);
                    screenPos = Camera.main.WorldToViewportPoint (location);
                    if (drawBound.Contains (screenPos)) {
                        GameObject tile = (GameObject)Instantiate (waterTilePrefab, location, transform.rotation);
                        tile.name = "Water tile " + x + ";" + z;
                        tile.transform.parent = waterTileHolder.transform;
                        waterTiles [x, z] = tile;
                    }
                }
            }
        } 
    }

    void AddDunes ()
    // Add little dunes and dips and holes to the beach
    {
        int mapPins = _mapSize * pinsPerTile;
        float ZDirection = 0.15f + 0.35f * Random.value;
        if (Random.value > 0.5f) {
            ZDirection *= -1;
        }
        int z0 = Random.Range (0, Mathf.RoundToInt (.15f * mapPins));
        int zInterval = Mathf.RoundToInt (Random.Range (80, 120) / 100f * mapPins / (float)nDunes);
        int pinX = 0;
        float xPhase = Random.value * 2 * Mathf.PI;        

        for (int d=0; d<nDunes; d++) {
            int duneLength = Mathf.FloorToInt (.1f * mapPins + Random.value * .4f * mapPins);
            ZDirection += -0.1f + 0.2f * Random.value;

            pinX = Mathf.RoundToInt (mapPins / 2f + Mathf.Sin (xPhase) * mapPins / 2f);
            int pinZ = z0 + d * zInterval;
            xPhase += Random.value * Mathf.PI;
            GameObject pin = getGlobalPin (pinX, pinZ, landTiles);
            
            // Determine dune's location and dimensions
            int startX = pinX;
            int startZ = pinZ;            
            float duneWidth = Gaussian.Rand ((maxDuneWidth - minDuneWidth) / 2, (maxDuneWidth - minDuneWidth) / 6, minDuneWidth, maxDuneWidth);
            int b = Mathf.CeilToInt (duneWidth);

            // Prevent height from being too close to zero because zero sigma gives funny Gaussians
            float duneHeight = 0;
            while (duneHeight <0.005f && duneHeight > -0.005f) {
                duneHeight = Gaussian.Rand (0, maxDuneHeight / 3, 0, maxDuneHeight);
            }

            print (string.Format ("Building dune starting from {0};{1}. Height {2:F3}, width {3:F3}, length {4}, direction {5}", 
                startX, startZ, duneHeight, duneWidth, duneLength, ZDirection));

            // for every tile along the ridge
            for (int i=0; i<duneLength; i++) {

                // Add a gaussian bump with some uniform noise
                
                for (int j=-b; j<=b; j++) {
                    for (int k=-b; k<=b; k++) {

                        float dY = Gaussian.GaussNorm (0, duneWidth / 3.0f, j) * Gaussian.GaussNorm (0, duneWidth / 3.0f, k) * duneHeight;
                        dY += Random.value * duneUniformNoise;

                        GameObject p = getGlobalPin (pinX + j, pinZ + k, landTiles);
                        if (p != null) {
                            p.transform.position = p.transform.position + Vector3.up * dY;
                        }
                    }
                }

                pinX++;
                pinZ = Mathf.RoundToInt (startZ + (i + 1) * ZDirection);
                pin = getGlobalPin (pinX, pinZ, landTiles);               

                if (pin == null) {
                    continue;
                }

            }
        }
    }

    void MakeWaves ()
    // Initialize a wave near the edge of the map
    {
    
    }

    void PropagateWaves ()
    // Make the waves move towards the land and break
    {

    }

    GameObject getGlobalPin (int x, int z, GameObject[,] from)
    // Get a pin using global, nonstop coordinates.
    {
        int tileX = x / pinsPerTile;
        int tileZ = z / pinsPerTile;

        int pinX = x % pinsPerTile;
        int pinZ = z % pinsPerTile;

        if ((tileX < 0) || (tileX >= _mapSize) || (tileZ < 0) || (tileZ >= _mapSize)) {
            //print (string.Format ("Pin {0};{1} ({2}.{3};{4}.{5}) is off the map!", x, z, tileX, pinX, tileZ, pinZ));
            return null;
        } else {
            //print (string.Format ("Pin {0};{1} ({2}.{3};{4}.{5}) should be on the map!", x, z, tileX, pinX, tileZ, pinZ));
        }

        generatePins p = from [tileX, tileZ].GetComponent<generatePins> ();

        if (p == null) {
            //print (string.Format ("Pin {0};{1} ({2}.{3};{4}.{5}) is null!", x, z, tileX, pinX, tileZ, pinZ));
            return null;
        } else {
            if (pinX < 0 || pinX >= pinsPerTile || pinZ < 0 || pinZ >= pinsPerTile) {
                return null;
            }
            return p.getPin (pinX, pinZ);
        }
    }
}
