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
    public float wavePeriod;    // in seconds
    
    private int _mapSize;
    private GameObject[,] waterTiles, landTiles;
    private GameObject waterTileHolder, landTileHolder;
    private float seafloorY;
    private int pinsPerTile;
    private int nDunes;
    private int mapPins;
    private float twoPi = Mathf.PI * 2;
    private float timePassed = 0;
    private float offsetX ;
    private float offsetZ;

    void Start ()
    {
        if (Application.isPlaying) {
            _mapSize = mapSize;
        } else {
            _mapSize = 5;
        }
        pinsPerTile = tilePrefab.GetComponent<generatePins> ().pinsPerTile;
        offsetX = twoPi * Random.value;
        offsetZ = twoPi * Random.value;

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
        
        Transform flag = GameObject.Find ("Game").transform.FindChild ("Flag");
        if (flag != null) {
            DestroyImmediate (flag.gameObject);
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
        PlaceFlag ();
        if (!Application.isPlaying) {
            MakeWaves ();
        }
    }
	

    void Update ()
    {        
        if (Application.isPlaying) {
            PropagateWaves ();
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
                if (z < landLimit + 4) {
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
    
    
    public void PlaceFlag ()
    {
        int x = Random.Range ((int)(0.3 * mapPins), (int)(0.7 * mapPins));
        int z = Random.Range ((int)(0.2 * mapPins), (int)(0.4 * mapPins));
        GameObject p = getGlobalPin (x, z, landTiles);
        PlacePile (x, z, 1, 6, 0, landTiles);
        Vector3 pos = p.GetComponent<pinManager> ().transform.position;
        
        GameObject flag = (GameObject)Instantiate (Resources.Load ("Flag"), pos, transform.rotation);
        flag.transform.Rotate (new Vector3 (0, Random.Range (120, 150), 0));
        
        flag.name = "Flag";
        flag.transform.parent = p.transform;
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
        float ZDirection = 0.05f + 0.3f * Random.value;
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
        
                float dY;
                if (pin == null) {
                    dY = duneHeight;
                } else {
                    dY = duneHeight * (0.25f + 1.0f * pin.transform.position.z / _mapSize); // lower dunes near the water
                }
                PlacePile (pinX, pinZ, dY, duneWidth, duneUniformNoise, landTiles);

                pinX++;
                pinZ = Mathf.RoundToInt (startZ + (i + 1) * ZDirection);
                pin = getGlobalPin (pinX, pinZ, landTiles);               

                if (pin == null) {
                    continue;
                }

            }
        }
    }


    // Add a gaussian bump with some uniform noise
    public void PlacePile (int pinX, int pinZ, float height, float width, float noise, GameObject[,] tiles)
    {        
        int b = Mathf.CeilToInt (width);
        
        for (int j=-b; j<=b; j++) {
            for (int k=-b; k<=b; k++) {                                  
                
                GameObject p = getGlobalPin (pinX + j, pinZ + k, tiles);
                if (p != null) {       
                    float dY = Gaussian.GaussNorm (0, width / 3.0f, j) * Gaussian.GaussNorm (0, width / 3.0f, k) * height;                        
                    dY += Random.value * duneUniformNoise;       
                    p.GetComponent<pinManager> ().changeHeight (dY);
                }
            }
        }
    }
		
    public void Smooth (int pinX, int pinZ, float amount, GameObject[,] tiles)
    {

        GameObject target = getGlobalPin (pinX, pinZ, tiles);
        if (target == null) {
            return;
        }
         
        float oldY = target.GetComponent<pinManager> ().transform.position.y;
        float newY = 0;
        
        //print (string.Format ("current height is {0:F3}", target.GetComponent<pinManager> ().transform.position.y));

        for (int j=-2; j<=2; j++) {
            for (int k=-2; k<=2; k++) {                                  
																		
                GameObject source = getGlobalPin (pinX + j, pinZ + k, tiles);
                if (source != null) {       
                    float posY = source.GetComponent<pinManager> ().transform.position.y;
                    
                    int d = Mathf.Abs (j) + Mathf.Abs (k);
                    float w = 0.16f / Mathf.Pow (2, d);
                    newY += w * posY;
                    float diff = oldY - posY;
                    source.GetComponent<pinManager> ().changeHeight (4.0f * w * diff);
                }
            }
        }
        target.GetComponent<pinManager> ().setHeight (newY);
    }
	
	
    // Initialize the sea with small, standard waves
    void MakeWaves ()
    {        
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
        timePassed += Time.deltaTime;
        float t = timePassed;
        float fx = twoPi / waveXLength;
        float fz = twoPi / waveZLength;

        float[] absorbPower = new float[mapPins];


        for (int z=0; z<mapPins; z++) {

            for (int x=0; x<mapPins; x++) {
                GameObject pin = getGlobalPin (x, z, landTiles);
                if (pin != null) {
                    absorbPower [x] += 0.05f * Mathf.Clamp (pin.transform.position.y, 0, 5);
                }                
            }

            for (int x=0; x<mapPins; x++) {
                GameObject pin = getGlobalPin (x, z, waterTiles);
                if (pin == null) {
                    continue;
                }

                float wx = Mathf.Sin (0.5f * fx * (x + z));
                float wz = (-0.5f + 1.25f * Mathf.Sin (0.1f * (t + fz * -z)) + 0.5f * Mathf.Sin (0.67f * (t + fz * -z)) + 0.25f * Mathf.Sin (2.8f * (t + fz * -z)));
                //wz = wz * wz;
                wz = Mathf.Abs (wz);
                float y = (wx + wz) * maxWaveHeight;
                if (z > waterRatio * 0.5f * mapPins) {
                    y -= 0.002f * (z - waterRatio * 0.5f);  
                }

                GameObject landPin = getGlobalPin (x, z - 1, landTiles);                

                if (landPin != null) {
                    pinManager pinMan = landPin.GetComponent<pinManager> ();
                    float diff = pin.transform.position.y - landPin.transform.position.y;                    

                    // land that's under water gets wet
                    if (diff >= -0.08f) {  
                        pinMan.setWetness (Mathf.Clamp01 (pinMan.wetness + 5 * (diff + 0.08f)));
                        
                    } // land that's not under water slowly dries up
                    else if (diff < -0.10f && pinMan.wetness > 0) {   
                        pinMan.setWetness (Mathf.Clamp01 (pinMan.wetness - 0.005f));
                    }
                    if (diff > 0 && Random.value > 0.9) {
                        Smooth (x, z, 0.8f, landTiles);
                    }
                }

                pin.GetComponent<pinManager> ().setHeight (y - absorbPower [x]);
            }
        }
        
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

        try {
            if (from [tileX, tileZ] == null) {
                return null;
            }
        } catch (System.NullReferenceException) {
            print (string.Format ("getGlobalPin({0},{1}) tried to access tile {2},{3} but that doesn't exist!",
                x, z, tileX, tileZ));
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

    public void destroyTerrain ()
    {
        DestroyImmediate (GameObject.Find ("Water"));
        DestroyImmediate (GameObject.Find ("Land"));
    }

    public GameObject[,] tiles (bool land)
    {
        if (land) {
            return landTiles;
        } else {
            return waterTiles;
        }
    }
}
