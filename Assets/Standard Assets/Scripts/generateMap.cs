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
    public float smoothFactor;
    
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
    private float fx;
    private float fz;
    private GameObject escapeMnu, loseMnu;

    public float[,] smoothFilter = {
        {0,1,2,1,0},
        {1,2,4,2,1},
        {1,4,8,4,1},
        {1,2,4,2,1},    
        {0,1,2,1,0},
    };

    
    void Start ()
    {
        // set the map size depending on whether you are in play or edit mode
        if (Application.isPlaying) {
            _mapSize = mapSize;
        } else {
            _mapSize = 5;
        }

        // initialize a bunch of values
        pinsPerTile = tilePrefab.GetComponent<generatePins> ().pinsPerTile;
        offsetX = twoPi * Random.value;
        offsetZ = twoPi * Random.value;
        nDunes = Mathf.RoundToInt (dunesPerTile * _mapSize * _mapSize);
        waterTiles = new GameObject[_mapSize, _mapSize];
        landTiles = new GameObject[_mapSize, _mapSize];
        seafloorY = -(GameManager.instance.slopeZ + GameManager.instance.slopeX) * waterRatio * _mapSize;
        fx = twoPi / waveXLength;
        fz = twoPi / waveZLength;
        escapeMnu = GameObject.Find ("Canvas").transform.FindChild ("Escape Menu").gameObject;
        loseMnu = GameObject.Find ("Canvas").transform.FindChild ("Lose Menu").gameObject;

        // normalize the smoothing filter
        float filterSum = 0;
        foreach (float v in smoothFilter) {
            filterSum += v;
        }
        for (int x = 0; x < smoothFilter.GetLength(0); x ++) {
            for (int y = 0; y < smoothFilter.GetLength(1); y ++) {
                smoothFilter [x, y] /= filterSum;
            }
        }
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

        //Camera.main.transform.position = new Vector3 (-_mapSize / 2f, Mathf.Sqrt (2.0f * _mapSize * _mapSize), -_mapSize / 2.0f);
        Camera.main.transform.position = new Vector3 (0, _mapSize + 0.5f, 0);
        Camera.main.orthographicSize = 0.65f + _mapSize * 0.19f;
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
        if (Application.isPlaying && !escapeMnu.activeInHierarchy & !loseMnu.activeSelf) {
            PropagateWaves ();
        }
    }


    void CreateTerrain ()
    {
        print (string.Format ("Generating {0}x{1} beach...", _mapSize, _mapSize));
        float landLimit = - seafloorY / GameManager.instance.slopeZ;
        Vector3 location, screenPos;
        
        for (int x=0; x<_mapSize; x++) {
            for (int z=0; z<_mapSize; z++) {
                
                // Create land
                location = new Vector3 (x, baseHeight (x, z), z);
                screenPos = Camera.main.WorldToViewportPoint (location + getCorner (x, z));

                // Only actually instantiate the tile if it would be on screen. This clips the corners off of the diamond map.
                if (drawBound.Contains (screenPos)) {
                    GameObject tile = (GameObject)Instantiate (tilePrefab, location, transform.rotation);          
                    tile.name = "Land tile " + x + ";" + z;
                    tile.transform.parent = landTileHolder.transform;
                    landTiles [x, z] = tile;
                }
                
                // Create water
                if (z < landLimit + 3) {
                    location = new Vector3 (x, 0, z);
                    screenPos = Camera.main.WorldToViewportPoint (location + getCorner (x, z));
                    if (true) { //}drawBound.Contains (screenPos)) {
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


    // returns the height of a land *tile* based on the beach's slope only, no dunes
    public float baseHeight (float x, float z)
    {
        return seafloorY + GameManager.instance.slopeX * x + GameManager.instance.slopeZ * z;
    }

    // returns the height of a land *tile* based on the beach's slope only, no dunes
    public float basePinHeight (float x, float z)
    {
        return baseHeight (x / pinsPerTile, z / pinsPerTile);
    }
    
    
    public void PlaceFlag ()
    {
        int x = Random.Range ((int)(0.3 * mapPins), (int)(0.7 * mapPins));
        int z = Random.Range ((int)(0.25 * mapPins), (int)(0.4 * mapPins));
        GameObject p = getGlobalPin (x, z, landTiles);
        PlacePile (x, z, 1, 6, 0, landTiles);
        Vector3 pos = p.GetComponent<pinManager> ().transform.position;
        
        GameObject flag = (GameObject)Instantiate (Resources.Load ("Flag"), pos, transform.rotation);
        flag.transform.Rotate (new Vector3 (0, Random.Range (120, 150), 0));
        
        flag.name = "Flag";
        flag.transform.parent = p.transform;
        flag.GetComponent<haveIlost> ().Set (x, z);        
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

        GameObject source = null;
        for (int j=-2; j<=2; j++) {
            for (int k=-2; k<=2; k++) {                                  
																		
                source = getGlobalPin (pinX + j, pinZ + k, tiles);
                if (source != null) {       
                    float posY = source.GetComponent<pinManager> ().transform.position.y;
                    
                    float w = smoothFilter [j + 2, k + 2];
                    newY += w * posY;
                    float diff = oldY - posY;
                    if (diff < 0) {
                        source.GetComponent<pinManager> ().changeHeight (amount * w * diff, false);
                    }
                }
            }
        }
        if (source != null) {
            source.GetComponent<pinManager> ().updateColor ();
        }
        if (newY < oldY) {
            target.GetComponent<pinManager> ().setHeight (newY);
        }
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
        float[] absorbPower = new float[mapPins];

        for (int z=0; z<mapPins; z++) {

            // find the sum of heights of the pins in this row (perpendicular to the beach)
            // the more land a wave encounters, the more it gets absorbed
            for (int x=0; x<mapPins; x++) {
                GameObject pin = getGlobalPin (x, z, landTiles);
                if (pin != null) {
                    absorbPower [x] += 0.05f * Mathf.Clamp (pin.transform.position.y, 0, 5);
                }                
            }

            // calculate the wave height for each pin based mostly on a sum of sines of x,z and time
            for (int x=0; x<mapPins; x++) {
                GameObject pin = getGlobalPin (x, z, waterTiles);
                if (pin == null) {
                    continue;
                }

                // Low frequency wave parallel to the beach
                float wx = Mathf.Sin (0.5f * fx * (Mathf.Cos (t) + x + z));

                // Compound sine wave perpendicular to the beach
                float wz = (-0.5f + 
                    1.25f * Mathf.Sin (0.1f * (t + fz * -z)) + 
                    0.5f * Mathf.Sin (0.67f * (t + fz * -z)) + 
                    0.25f * Mathf.Sin (2.8f * (t + fz * -z)) +
                    2.0f * Mathf.Sin (0.01f * (t + fz * -z))); // killer wave!
                //wz = wz * wz;
                
                float y = (wx + wz) * maxWaveHeight;

                // Water level is lowered the further inland in gets
                if (z > waterRatio * 0.5f * mapPins) {
                    y -= 0.003f * (z - waterRatio * 0.5f * mapPins);  
                }

                // Set height
                pin.GetComponent<pinManager> ().setHeight (y - absorbPower [x]);

                // Get matching land pin 
                GameObject landPin = getGlobalPin (x, z, landTiles);                

                if (landPin != null) {
                    pinManager pinMan = landPin.GetComponent<pinManager> ();
                    float diff = pin.transform.position.y - landPin.transform.position.y;                    
                    // negative: land is higher, positive: water is higher

                    // land that's in the surf gets eroded
                    if (diff > 0 && diff < 0.1f && Random.value > 0.9) {
                        Smooth (x, z, smoothFactor, landTiles);
                        diff = pin.transform.position.y - landPin.transform.position.y;   
                    }                    

                    // land under or close to the water level gets wet
                    if (diff >= -0.08f) {  
                        pinMan.setWetness (Mathf.Clamp01 (pinMan.wetness + 5.0f * (diff + 0.08f)));
                        
                    } // land sufficiently higher than the water slowly dries up
                    else if (diff < -0.08f && pinMan.wetness > 0) {   
                        pinMan.setWetness (Mathf.Clamp01 (pinMan.wetness + 0.06f * diff));
                    }                   
                }

                
            }
        }
        
    }


    public GameObject getGlobalPin (int x, int z, GameObject[,] from)
    // Get a pin using global, nonstop coordinates.
    {
        int tileX = x / pinsPerTile;
        int tileZ = z / pinsPerTile;

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
            int pinX = x % pinsPerTile;
            int pinZ = z % pinsPerTile;
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
