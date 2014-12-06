using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generateMap : MonoBehaviour
{

    public int mapSize;
    public float waterRatio;
    public Transform tilePrefab, waterTilePrefab;
    public float slopeX, slopeZ;
    public Rect drawBound;
    public int nBumps;
    public float bumpHeight;
    public float bumpWidth;
    
    private int _mapSize;
    private Transform[,] waterTiles, landTiles;
    private GameObject waterTileHolder, landTileHolder;
    private bool done = false;
    private float seafloorY;

    void Start ()
    {
        if (Application.isPlaying) {
            _mapSize = 10;
        } else {
            _mapSize = mapSize;
        }

        waterTiles = new Transform[_mapSize, _mapSize];
        landTiles = new Transform[_mapSize, _mapSize];
        seafloorY = -(slopeZ + slopeX) * waterRatio * _mapSize;

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

        Camera.main.transform.position = new Vector3 (-_mapSize / 2, _mapSize * 0.8f, -_mapSize / 2);

    }
	

    void Update ()
    {
        if (!done) {                        
                            
            CreateTerrain ();
            AddBumps ();

            done = true;
        }
    }


    void CreateTerrain ()
    {
        print (string.Format ("Generating {0}x{1} beach...", _mapSize, _mapSize));
        float landLimit = _mapSize * waterRatio;
        Vector3 location, screenPos;
        
        for (int x=0; x<_mapSize; x++) {
            for (int z=0; z<_mapSize; z++) {
                
                // Create land
                location = new Vector3 (x, seafloorY + slopeX * x + slopeZ * z, z);
                screenPos = Camera.main.WorldToViewportPoint (location);
                // Only actually instantiate the tile if it would be on screen. This is in order to clip the points off of the diamond map.
                if (drawBound.Contains (screenPos)) {
                    Transform tile = (Transform)Instantiate (tilePrefab, location, transform.rotation);
                    
                    tile.name = "Land tile " + x + ";" + z;
                    tile.parent = landTileHolder.transform;
                    landTiles [x, z] = tile;
                }
                
                // Create water
                if (z < landLimit + 2) {
                    location = new Vector3 (x, 0, z);
                    screenPos = Camera.main.WorldToViewportPoint (location);
                    if (drawBound.Contains (screenPos)) {
                        Transform tile = (Transform)Instantiate (waterTilePrefab, location, transform.rotation);
                        
                        tile.name = "Water tile " + x + ";" + z;
                        tile.parent = waterTileHolder.transform;
                        waterTiles [x, z] = tile;
                    }
                }
            }
        } 
    }

    void AddBumps ()
    // Add little hills and bumps and dips and holes to the beach
    {
        for (int i=1; i<nBumps; i++) {
            float yScale = Gaussian.Rand (0, bumpWidth, -bumpHeight, bumpHeight);
            Transform tile = null;
            int x = 0, z = 0;
            
            while (tile == null) {
                x = Random.Range (0, _mapSize);
                z = Random.Range (0, _mapSize);
                try {
                    tile = landTiles [x, z];    
                } catch (System.IndexOutOfRangeException) {
                    // do nothing
                }                
            }
            print (string.Format ("Planting bump with yScale {0:F3} on tile {1},{2}", yScale, x, z));

            tile.transform.localScale = tile.transform.localScale + Vector3.up * yScale;
            tile.transform.position = tile.transform.position + Vector3.up * yScale / 2;
        }

    }

    void MakeWaves ()
    {
    
    }

    void PropagateWaves ()
    {

    }
}
