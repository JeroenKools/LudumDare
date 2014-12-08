using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class notifications : MonoBehaviour {

	public List<string> phrases = new List<string>();
	public int interval;
	public GameObject text;

	void Start () {
		InvokeRepeating ("Notify", 0, interval);
	}
	
	void Notify () {
		if (phrases.Count > 0) { //and kid menus are not active!!!
			//print (phrases[0]);
			text.GetComponent<Text>().text = phrases[0];
			phrases.RemoveAt(0);
		}
	}
}
