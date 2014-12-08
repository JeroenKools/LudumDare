using UnityEngine; 
using UnityEditor; 
using System.Collections;

public class FileModificationWarning : UnityEditor.AssetModificationProcessor
{
    
    static string[] OnWillSaveAssets (string[] paths)
    {
        Debug.Log ("Preventing random terrain from messing up the GitHub!");
        
        GameObject.Find ("Terrain").GetComponent<generateMap> ().destroyTerrain ();

        return paths;
    }
}