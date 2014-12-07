using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public float slopeX, slopeZ;

    private static GameManager _instance;
    public static GameManager instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<GameManager> ();
                
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
            //If a Singleton already exists and you find another reference in scene, destroy it!
            Destroy (this.gameObject);
        }
    }
	
}
