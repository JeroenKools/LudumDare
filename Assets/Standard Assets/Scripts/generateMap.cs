using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generateMap : MonoBehaviour
{

    public int mapSize;
    public float waterRatio;
    public GameObject tilePrefab, waterTilePrefab;
    public Rect drawBound;
    public float nBumps; // number of bumps as a fraction of the number of tiles
    public float maxBumpHeight;
    public float minBumpWidth;
    public float maxBumpWidth;
    public float bumpUniformNoise;
    
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
        print ("Done building tiles!");
        AddBumps (); 
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

    void AddBumps ()
    // Add little hills and bumps and dips and holes to the beach
    {
        int n = Mathf.FloorToInt (nBumps * _mapSize * _mapSize);
        
        for (int i=1; i<n; i++) {            
            GameObject tile = null;
            int tileX = 0, tileZ = 0;
            
            while (tile == null) { //} || tile.transform.position.y < 0) {
                tileX = Random.Range (0, _mapSize);
                tileZ = Random.Range (0, _mapSize);
                tile = landTiles [tileX, tileZ];                
            }

            // Tile-wide height change
            //tile.transform.localScale = tile.transform.localScale + Vector3.up * yScale;
            //tile.transform.position = tile.transform.position + Vector3.up * yScale / 2;

            // Determine bump's location and dimensions
            int pinX, pinZ;
            pinX = Random.Range (0, tile.GetComponent<generatePins> ().pinsPerTile - 1);
            pinZ = Random.Range (0, tile.GetComponent<generatePins> ().pinsPerTile - 1);
            float bumpWidth = Gaussian.Rand ((maxBumpWidth - minBumpWidth) / 2, (maxBumpWidth - minBumpWidth) / 6, minBumpWidth, maxBumpWidth);
            print (bumpWidth);

            // Prevent from being too close to zero
            float bumpHeight = 0;
            while (bumpHeight <0.005f && bumpHeight > -0.005f) {
                bumpHeight = Gaussian.Rand (0, maxBumpHeight / 3, -maxBumpHeight, maxBumpHeight);
            }

            //print ("Bump w/h " + bumpWidth + "," + bumpHeight);

            // Add a gaussian bump with some uniform noise
            int b = Mathf.CeilToInt (bumpWidth);
            for (int j=-b; j<=b; j++) {
                for (int k=-b; k<=b; k++) {
                
                    int x = tileX;
                    int z = tileZ;
                    int offsetX = 0, offsetZ = 0;

                    // Find the correct pin, in this tile or an adjacent one!
                    if (pinX + j < 0) {
                        x--;
                        offsetX += pinsPerTile;
                        if (x < 0) {
                            continue;
                        }
                    } else if (pinX + j > pinsPerTile - 1) {
                        x++;
                        offsetX -= pinsPerTile;
                        if (x > _mapSize - 1) {
                            continue;
                        }
                    } 

                    if (pinZ + k < 0) {
                        z--;
                        offsetZ += pinsPerTile;
                        if (z < 0) {
                            continue;
                        }
                    } else if (pinZ + k > pinsPerTile - 1) {
                        z++;
                        offsetZ -= pinsPerTile;
                        if (z > _mapSize - 1) {
                            continue;
                        }
                    } 

                    if (landTiles [x, z] == null) {
                        continue;
                    }

                    float dY = Gaussian.GaussNorm (0, bumpWidth / 3.0f, j) * Gaussian.GaussNorm (0, bumpWidth / 3.0f, k) * bumpHeight;
                    dY += Random.value * bumpUniformNoise;

                   
                    GameObject p = landTiles [x, z].GetComponent<generatePins> ().getPin (pinX + offsetX + j, pinZ + offsetZ + k);
                    //p.transform.localScale = p.transform.localScale + Vector3.up * dY;
                    p.transform.position = p.transform.position + Vector3.up * dY;
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
}
