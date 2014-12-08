using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class generateFlavorText : MonoBehaviour {

	public int waitBeforeFirstFlavor;
	public int flavorInterval;
	public string[] flavorPhrases = new string[10];
	private string selectedPhrase;
	private int randomPhrase;
	
	void Start () {
		InvokeRepeating ("CreateFlavor", waitBeforeFirstFlavor, flavorInterval);
	}
	
	//I added this because it seemed to only get randomPhrase once when i put it in createWave
	
	void FixedUpdate () {
		randomPhrase = Random.Range (0, flavorPhrases.Length);
	}
	
	//pick a phrase and send it to delegatePhrases
	
	void CreateFlavor () {
		//Runs sorting function to figure out where kids are
		gameObject.GetComponent<kidFlux> ().SortKidsByActive ();

		print ("active kids for flavors: " + gameObject.GetComponent<kidFlux> ().activeKids.Count);

		if (gameObject.GetComponent<kidFlux> ().activeKids.Count > 0) {
			string kidName = gameObject.GetComponent<kidFlux>().activeKids[Random.Range(0, gameObject.GetComponent<kidFlux> ().activeKids.Count)].GetComponent<kid>().kidName;
			selectedPhrase = flavorPhrases [randomPhrase];
			GameObject.Find ("Notifications").GetComponent<notifications> ().phrases.Add (kidName + ": " + selectedPhrase);	
		} else {
			print("no kids - no flavor text");
		}
	}
}
