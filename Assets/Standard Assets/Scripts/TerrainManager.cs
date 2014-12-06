using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour
{
    public float slopeX, slopeZ;

    private static TerrainManager _instance;
    public static TerrainManager instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<TerrainManager> ();
                
                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad (_instance.gameObject);
            }
            
            return _instance;
        }
    }   
    
    
    void Awake ()
    {
        if (_instance == null) {
            //If I am the first instance, make me the Singleton
            _instance = this;
            DontDestroyOnLoad (this);
            
        } else if (this != _instance) {
            //If a Singleton already exists and you find
            //another reference in scene, destroy it!
            Destroy (this.gameObject);
            //print ("Destroyed " + this + ", as " + _instance + " is already the singleton.");
        }
    }
	
}
