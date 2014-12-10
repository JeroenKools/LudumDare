using UnityEngine;
using System.Collections;

public class haveIlost : MonoBehaviour
{
    public static int x, z;      // flag location
    private GameObject waterPin; // the water pin that could cost you the game
    private bool haveLost;

    // Use this for initialization
    void Start ()
    {
	
    }
    
    public void Set (int X, int Z)
    {
        x = X;
        z = Z;
        haveLost = false;
        
        generateMap g = GameObject.Find ("Terrain").GetComponent<generateMap> ();
        waterPin = g.getGlobalPin (x, z, g.tiles (false));
    }
	
    // Update is called once per frame
    void Update ()
    {
        float flagBase = transform.position.y;
        float waterLevel = waterPin.transform.position.y;
        
        if (waterLevel > flagBase && !haveLost) {
            youLose ();
            haveLost = true;
        }
    }
    
    void youLose ()
    {
        
        GameObject.Find ("Canvas").transform.Find ("Lose Menu").gameObject.SetActive (true);
    }
}
