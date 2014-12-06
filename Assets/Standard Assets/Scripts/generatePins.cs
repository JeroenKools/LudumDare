using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class generatePins : MonoBehaviour
{
    public int pinsPerTile;    
    public Transform pinPrefab;
    public float gapRatio;

    private GameObject pinHolder;
    private Transform[,] pins;
    private float dist, gapWidth, pinWidth;
    
    void Start ()
    {
        pins = new Transform[pinsPerTile, pinsPerTile]; 

        // Delete existing pins
        Transform pinT = transform.FindChild ("Pins");
        if (pinT != null) {
            DestroyImmediate (pinT.gameObject);
        }
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
                Transform pin = (Transform)Instantiate (pinPrefab, 
                                                        new Vector3 (pos.x + x * dist, pos.y, pos.z + dist * z),
                                                        transform.rotation);
                pin.localScale = new Vector3 (pinWidth, 3 * pinWidth, pinWidth);
                
                pin.name = "Pin " + x + ";" + z;
                pin.parent = pinHolder.transform;
                pins [x, z] = pin;
            }
        }
    }
	
    
    void Update ()
    {

    }

    public float getPinWidth ()
    {
        return pinWidth;
    }

    public float getGapWidth ()
    {
        return gapWidth;
    }   


}

