using UnityEngine;
using System.Collections;

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
		selectedPhrase = flavorPhrases [randomPhrase];
		GameObject.Find("Notifications").GetComponent<notifications> ().phrases.Add (selectedPhrase);
	}
}
