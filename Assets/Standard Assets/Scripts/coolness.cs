using UnityEngine;
using System.Collections;

public class coolness : MonoBehaviour {

	public bool isCool;
	public float percentCool;
	
	void Update () {
		if (Random.value < percentCool) {
			isCool = true;
		} else {
			isCool = false;		
		}
	}
}
