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
    private bool done = false;
    
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
    }
	
    
    void Update ()
    {
        if (!done) {
            float dist = 1.0f / pinsPerTile;
            float gapWidth = dist * gapRatio;
            float pinWidth = dist - gapWidth;
            Vector3 pos = transform.position;
		
            for (int x=0; x<pinsPerTile; x++) {
                for (int z=0; z<pinsPerTile; z++) {
                    Transform pin = (Transform)Instantiate (pinPrefab, 
                                        new Vector3 (pos.x + x * dist, pos.y, pos.z + dist * z),
                                        transform.rotation);
                    pin.localScale = new Vector3 (pinWidth, pinWidth, pinWidth);
										
                    pin.name = "Pin " + x + ";" + z;
                    pin.parent = pinHolder.transform;
                    pins [x, z] = pin;
                }
            }
            done = true;
        }
    }
}
