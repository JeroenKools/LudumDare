using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generateMap : MonoBehaviour
{

    public int mapSize;
    public float waterRatio;
    public Transform tilePrefab, waterTilePrefab;
    public float slopeX, slopeZ;

    private Transform[,] waterTiles, landTiles;
    private GameObject waterTileHolder, landTileHolder;
    private bool done = false;
    private float seafloorY;

    void Start ()
    {

        waterTiles = new Transform[mapSize, mapSize];
        landTiles = new Transform[mapSize, mapSize];
        seafloorY = -(slopeZ + slopeX) * waterRatio * mapSize;

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

    }
	

    void Update ()
    {
        if (!done) {                        
            
            float landLimit = mapSize * waterRatio;

            for (int x=0; x<mapSize; x++) {
                for (int z=0; z<mapSize; z++) {

                    // Create land
                    Transform tile = (Transform)Instantiate (tilePrefab, 
                                                        new Vector3 (x, seafloorY + slopeX * x + slopeZ * z, z),
                                                        transform.rotation);
                
                    tile.name = "Land tile " + x + ";" + z;
                    tile.parent = landTileHolder.transform;
                    landTiles [x, z] = tile;
                    
                    // Create water
                    if (z < landLimit + 2) {
                        tile = (Transform)Instantiate (waterTilePrefab, 
                                                                 new Vector3 (x, 0, z),
                                                                 transform.rotation);
                        
                        tile.name = "Water tile " + x + ";" + z;
                        tile.parent = waterTileHolder.transform;
                        waterTiles [x, z] = tile;

                    }
                }
            }

            transform.position = Camera.main.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, mapSize * 2)); //transform.position;

            done = true;
        }
    }
}
