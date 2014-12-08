using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kid : MonoBehaviour {

	public List<string> phrases = new List<string>();
	public string kidName;
	public int speakInterval;
	
	void Start () {
		InvokeRepeating ("speak", 0, speakInterval);
	}

	void Update () {
	
	}

	void arrive () {

	}

	void leave () {

	}

	void speak () {
		if (phrases.Count > 0) {
			print (phrases[0] + this.kidName);
			phrases.RemoveAt(0);
		}
	}
}
