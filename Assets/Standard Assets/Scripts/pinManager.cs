using UnityEngine;
using System.Collections;

public class pinManager : MonoBehaviour
{
    public Color c1;
    public Color c2;
    private Color baseColor;
    public Color highColor;
    public Color wetColor;
    public float heightScale;    
    public float wetness;
    private generateMap gm;
    
    void Awake ()
    {
        baseColor = new Color (Random.Range (c1.r, c2.r), Random.Range (c1.g, c2.g), Random.Range (c1.b, c2.b), Random.Range (c1.a, c2.a));
        renderer.material.color = baseColor;
        wetness = 0;
        gm = GameObject.Find ("Terrain").GetComponent<generateMap> ();
    }


    public Color getBaseColor ()
    {
        return baseColor;
    }


    public void changeHeight (float deltaY)
    {
        transform.position = transform.position + Vector3.up * deltaY;        
        updateColor ();        
    }

    public void setHeight (float y)
    {
        transform.position = new Vector3 (transform.position.x, y, transform.position.z);
        updateColor ();        
    }


    public void updateColor ()
    {
        if (Application.isPlaying) {           
            renderer.material.color = baseColor + heightColor ();
        }
    }

    public Color heightColor ()
    {
        float h = Mathf.Clamp01 (transform.position.y * heightScale);
        return h * highColor;
    }

    public void setWetness (float w)
    {
        wetness = w;
        renderer.material.color = wetness * wetColor + (1 - wetness) * (baseColor + heightColor ());
    }


    public void OnMouseOver ()
    {   
        int x = Mathf.FloorToInt (transform.position.x / transform.localScale.x);
        int z = Mathf.FloorToInt (transform.position.z / transform.localScale.z);        

        if (Input.GetMouseButton (0)) {       
            gm.PlacePile (x, z, GameManager.instance.clickDeltaY, 3, 0, gm.tiles (true));
        } else if (Input.GetMouseButton (1)) {
            gm.PlacePile (x, z, -GameManager.instance.clickDeltaY, 3, 0, gm.tiles (true));
        }

    }
}
