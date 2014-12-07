using UnityEngine;
using System.Collections;

public class defineColor : MonoBehaviour
{
    public Color c1;
    public Color c2;
    private Color c3;

    // Use this for initialization
    void Start ()
    {
        c3 = new Color (Random.Range (c1.r, c2.r), Random.Range (c1.g, c2.g), Random.Range (c1.b, c2.b), Random.Range (c1.a, c2.a));
        renderer.material.color = c3;
    }

    public Color getBaseColor ()
    {
        return c3;
    }
}
