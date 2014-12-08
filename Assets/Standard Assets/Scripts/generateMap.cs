using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generateMap : MonoBehaviour
{

    public int mapSize;
    public float waterRatio;
    public GameObject tilePrefab, waterTilePrefab;
    public Rect drawBound;
    public float dunesPerTile;
    public float maxDuneHeight;
    public float minDuneWidth;
    public float maxDuneWidth;
    public float duneUniformNoise;
    public float minWaveHeight;
    public float maxWaveHeight;
    public float waveXLength;   // wavelength in pins    
    public float waveZLength;   // space between successive waves
    
    private int _mapSize;
    private GameObject[,] waterTiles, landTiles;
    private GameObject waterTileHolder, landTileHolder;
    private float seafloorY;
    private int pinsPerTile;
    private int nDunes;
    private int mapPins;


    void Start ()
    {
        if (Application.isPlaying) {
            _mapSize = mapSize;
        } else {
            _mapSize = 5;
        }
        pinsPerTile = tilePrefab.GetComponent<generatePins> ().pinsPerTile;

        nDunes = Mathf.RoundToInt (dunesPerTile * _mapSize * _mapSize);
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

        Camera.main.transform.position = new Vector3 (-_mapSize / 2f, Mathf.Sqrt (2.0f * _mapSize * _mapSize), -_mapSize / 2.0f);
        Camera.main.orthographicSize = 1 + _mapSize * 0.15f;
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
                screenPos = Camera.main.WorldToViewportPoint (location + getCorner (x, z));

                // Only actually instantiate the tile if it would be on screen. This clips the corners off of the diamond map.
                if (drawBound.Contains (screenPos)) {
                    GameObject tile = (GameObject)Instantiate (tilePrefab, location, transform.rotation);          
                    tile.name = "Land tile " + x + ";" + z;
                    tile.transform.parent = landTileHolder.transform;
                    landTiles [x, z] = tile;
                }
                
                // Create water
                if (z < landLimit + 2) {
                    location = new Vector3 (x, 0, z);
                    screenPos = Camera.main.WorldToViewportPoint (location + getCorner (x, z));
                    if (drawBound.Contains (screenPos)) {
                        GameObject tile = (GameObject)Instantiate (waterTilePrefab, location, transform.rotation);
                        tile.name = "Water tile " + x + ";" + z;
                        tile.transform.parent = waterTileHolder.transform;
                        waterTiles [x, z] = tile;
                    }
                }
            }
        } 
        mapPins = _mapSize * pinsPerTile;
    }


    Vector3 getCorner (int x, int z)
    {
        // return the corner (vector from center) that is closest to the edge of the map
        float m = _mapSize / 2f;        
        Vector3 v = new Vector3 ();

        Vector3 h = Vector3.up * GameManager.instance.pinHeight / (2 * pinsPerTile);        
        Vector3 r = Vector3.right / 2f;
        Vector3 l = Vector3.left / 2f;
        Vector3 f = Vector3.forward / 2f;
        Vector3 b = Vector3.back / 2f;
        

        if (x <= m && z <= m) {                     // bottom quadrant
            v += h + r + f;
        } else if (x > m && z <= m) {               // right
            v += h + l + f;
        } else if (x <= m && z > m) {               // left
            v += h + r + b;
        } else {                                    // top
            v += -h + l + b;
        }
        
        v += ((z + x) <= 1.5f * m) ? Vector3.up : new  Vector3 (); //Vector3.down;
    
        return v;
    }


    void AddDunes ()
    // Add little dunes and dips and holes to the beach
    {        
        float ZDirection = 0.15f + 0.25f * Random.value;
        if (Random.value > 0.5f) {
            ZDirection *= -1;
        }
        int z0 = Random.Range (0, Mathf.RoundToInt (.15f * mapPins));
        int zInterval = Mathf.Max (1, Mathf.RoundToInt (Random.Range (80, 120) / 100f * (float)mapPins / (float)nDunes));
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
            int startZ = pinZ;            
            float duneWidth = Gaussian.Rand ((maxDuneWidth - minDuneWidth) / 2, (maxDuneWidth - minDuneWidth) / 6, minDuneWidth, maxDuneWidth);            

            // Prevent height from being too close to zero because zero sigma gives funny Gaussians
            float duneHeight = 0;
            while (duneHeight <0.005f && duneHeight > -0.005f) {
                duneHeight = Gaussian.Rand (0, maxDuneHeight / 3, 0, maxDuneHeight);
            }

            //print (string.Format ("Planting a dune at {0};{1}. Width {2}, height {3}, direction {4}, zInterval {5}", pinX, pinZ, duneWidth, duneHeight, ZDirection, zInterval));

            // for every tile along the ridge
            for (int i=0; i<duneLength; i++) {

                PlacePile (pinX, pinZ, duneHeight, duneWidth, duneUniformNoise, landTiles);

                pinX++;
                pinZ = Mathf.RoundToInt (startZ + (i + 1) * ZDirection);
                pin = getGlobalPin (pinX, pinZ, landTiles);               

                if (pin == null) {
                    continue;
                }

            }
        }
    }


    void PlacePile (int pinX, int pinZ, float height, float width, float noise, GameObject[,] tiles)
    {
        // Add a gaussian bump with some uniform noise

        int b = Mathf.CeilToInt (width);
        
        for (int j=-b; j<=b; j++) {
            for (int k=-b; k<=b; k++) {
                
                float dY = Gaussian.GaussNorm (0, width / 3.0f, j) * Gaussian.GaussNorm (0, width / 3.0f, k) * height;                        
                dY += Random.value * duneUniformNoise;                        
                
                GameObject p = getGlobalPin (pinX + j, pinZ + k, landTiles);
                if (p != null) {
                    dY *= 0.333f + Mathf.Clamp (p.transform.position.y, 0, 067f); // lower dunes near the water
                    p.GetComponent<pinManager> ().changeHeight (dY);
                }
            }
        }
    }
    
    
    void MakeWaves ()
        // Initialize the sea with small, standard waves
    {

        float twoPi = Mathf.PI * 2;
        float offsetX = twoPi * Random.value;
        float offsetZ = twoPi * Random.value;
        
        for (int z=0; z<mapPins; z++) {
            for (int x=0; x<mapPins; x++) {
                GameObject pin = getGlobalPin (x, z, waterTiles);
                if (pin == null) {
                    continue;
                }

                float wx = Mathf.Sin ((0.5f * x + z) / waveXLength * twoPi + offsetX);
                float wz = Mathf.Cos (z / waveZLength * twoPi + offsetZ);
                float dY = (wx + wz) * minWaveHeight;
                pin.GetComponent<pinManager> ().changeHeight (dY);
            }
        }
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
        } 

        if (from [tileX, tileZ] == null) {
            return null;
        }

        generatePins p = from [tileX, tileZ].GetComponent<generatePins> ();

        if (p == null) {
            return null;
        } else {
            if (pinX < 0 || pinX >= pinsPerTile || pinZ < 0 || pinZ >= pinsPerTile) {
                return null;
            }
            return p.getPin (pinX, pinZ);
        }
    }
}
