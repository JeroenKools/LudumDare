using UnityEngine;
using System.Collections;

public class waveTimer : MonoBehaviour {

	public int waveInterval;
	public int waitBeforeFirstWave;
	public GameObject manager;
	public string[] wavePhrases = new string[5];
	private string selectedPhrase;
	private int randomPhrase;
	
	void Start () {
		InvokeRepeating ("CreateWave", waitBeforeFirstWave, waveInterval);
	}

	//I added this because it seemed to only get randomPhrase once when i put it in createWave

	void FixedUpdate () {
		randomPhrase = Random.Range (0, wavePhrases.Length);
	}

	//pick a phrase and send it to delegatePhrases

	void CreateWave () {
		selectedPhrase = wavePhrases [randomPhrase];
		GameObject.Find("Game").GetComponent<delegatePhrases> ().phrases.Add (selectedPhrase);
	}
}