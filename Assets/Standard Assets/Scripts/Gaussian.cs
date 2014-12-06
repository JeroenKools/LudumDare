using UnityEngine;
using System.Collections;

public class Gaussian : MonoBehaviour
{
    
    private static float sqrt2Pi = Mathf.Sqrt (2 * Mathf.PI);


    public static float Gauss (float mean, float sigma, float x)
    {
        return 1.0f / (sigma * sqrt2Pi) * Mathf.Exp (-0.5f * Mathf.Pow ((x - mean) / sigma, 2f));
    }

    public static float GaussNorm (float mean, float sigma, float x)
    // Normalized to have a peak value of 1
    {
        return Gauss (mean, sigma, x) / Gauss (mean, sigma, mean);
    }

    public static float Rand (float  mean, float  sigma, float min, float max)
    {   
        // Box-Muller algorithm, with rejection sampling
        float r = Mathf.Infinity;

        while (r > max || r < min) {
            float u1 = Random.value;
            float u2 = Random.value;
            float theta = 2.0f * Mathf.PI * u2;
            r = Mathf.Sqrt (-2.0f * Mathf.Log (u1));            
            r = r * Mathf.Sin (theta);
            r = r * sigma + mean;
        }
        return r;
    }

    public static float[] Filter (float mean, float  sigma, int width)
    {
        float[] filter = new float[width];
        float min, max;
           
        if (width % 2 == 0) {
            max = Mathf.Floor (width / 2.0f);            
        } else {            
            max = width / 2.0f - 0.5f;
        }
        min = -max;
        int i = 0;
        for (float x=min; x<=max; x++) {
            filter [i] = Gauss (mean, sigma, x);
            i++;
        }
        return filter;
    }
}


