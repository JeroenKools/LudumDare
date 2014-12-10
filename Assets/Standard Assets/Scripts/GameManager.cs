using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public float slopeX, slopeZ;
    public float pinHeight;
    public float clickDeltaY;
    public string activeTool = "Place";
    private static GameManager _instance;
    public static GameManager instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<GameManager> ();
            }
            return _instance;
        }
    }   
    
    
    void Awake ()
    {
        if (_instance == null) {
            //If I am the first instance, make me the Singleton
            _instance = this;
            
        } else if (this != _instance) {
            //If a Singleton already exists and you find another reference in scene, destroy it!
            Destroy (this.gameObject);
        }
    }
		
    public void setActiveTool (string t)
    {
        activeTool = t;
    }
		
    void OnGUI ()
    {
        if (Input.GetMouseButtonDown (1)) {
            // Right-click to deselect/cancel current tool
            string[] deactivateObjects = {"menuBucket", "menuShovel", "menuChest",
						"Cursor/backgroundBlue","Cursor/backgroundGreen", "Cursor/backgroundGold"};				
            foreach (string s in deactivateObjects) {
                if (GameObject.Find (s) != null) {
                    GameObject.Find (s).SetActive (false);
                }
            }
            GameObject cursorText = GameObject.Find ("Cursor/text");
            if (cursorText != null) {
                cursorText.GetComponent<Text> ().text = "";
            }
	
        }
                
        if (Input.GetKeyDown (KeyCode.L)) { // lose cheat
            generateMap g = GameObject.Find ("Terrain").GetComponent<generateMap> ();
            g.PlacePile (haveIlost.x, haveIlost.z, -0.5f, 4, 0, g.tiles (true));
        }
    }
    
    public void restart ()
    {
        Application.LoadLevel (Application.loadedLevel);
    }
}
