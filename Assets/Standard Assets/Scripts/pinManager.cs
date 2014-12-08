using UnityEngine;
using System.Collections;

public class pinManager : MonoBehaviour
{
    public Color c1;
    public Color c2;
    private Color baseColor;
    public Color highColor;
    public float heightScale;
    private float wetness;

    
    void Awake ()
    {
        baseColor = new Color (Random.Range (c1.r, c2.r), Random.Range (c1.g, c2.g), Random.Range (c1.b, c2.b), Random.Range (c1.a, c2.a));
        renderer.material.color = baseColor;
        wetness = 0;
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


    public void updateColor ()
    {
        if (Application.isPlaying) {
            float h = Mathf.Clamp01 (transform.position.y * heightScale);
            gameObject.renderer.material.color = baseColor + h * highColor;
        }
    }
}
