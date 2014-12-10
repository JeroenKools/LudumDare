using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generatePins : MonoBehaviour
{
    public int pinsPerTile;    
    public GameObject pinPrefab;
    public float gapRatio;

    private GameObject pinHolder;
    private GameObject[,] pins;
    private float dist, gapWidth, pinWidth;
    

    void Awake ()
    {
        // Delete existing pins
        Transform pinT = transform.FindChild ("Pins");
        if (pinT != null) {
            DestroyImmediate (pinT.gameObject);
        }
        pins = new GameObject[pinsPerTile, pinsPerTile]; 
        pinHolder = new GameObject ("Pins");
        pinHolder.transform.parent = transform;
        Build ();
    }


    void Build ()
    {
        dist = 1.0f / pinsPerTile;
        gapWidth = dist * gapRatio;
        pinWidth = dist - gapWidth;
        
        Vector3 pos = transform.position;        

        for (int x=0; x<pinsPerTile; x++) {
            for (int z=0; z<pinsPerTile; z++) {

                float slope = pinPrefab.name == "WaterPin" ? 0 : GameManager.instance.slopeX / pinsPerTile * (float)x + 
                    GameManager.instance.slopeZ / pinsPerTile * (float)z;

                GameObject pin = (GameObject)Instantiate (pinPrefab, 
                                    new Vector3 (pos.x + x * dist, 2 * pos.y + slope, pos.z + dist * z), 
                                    transform.rotation);                
                pin.transform.localScale = new Vector3 (pinWidth, GameManager.instance.pinHeight * pinWidth, pinWidth);
                
                pin.name = "Pin " + x + ";" + z;
                pin.transform.parent = pinHolder.transform;
                pins [x, z] = pin;
            }
        }

        transform.position = transform.position - transform.position.y * Vector3.up; // remove tile slope once it has been transferred to pins
    }


    void Start ()
    {
        // make sure all pins are height-colored   
        for (int x=0; x<pinsPerTile; x++) {
            for (int z=0; z<pinsPerTile; z++) {
                GameObject pin = pins [x, z]; 
                if (pin != null) {
                    pin.GetComponent<pinManager> ().updateColor ();
                }
            }
        }
    }


    public float getPinWidth ()
    {
        return pinWidth;
    }


    public float getGapWidth ()
    {
        return gapWidth;
    }   


    public GameObject getPin (int x, int z)
    {           
        return pins [x, z];
    }


}

