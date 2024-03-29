﻿using UnityEngine;
using System.Collections;

public class waveTimer : MonoBehaviour
{

    public int waveInterval;
    public int waitBeforeFirstWave;
    public string[] wavePhrases = new string[5];
    private string selectedPhrase;
    private int randomPhrase;
	
    void Start ()
    {
        InvokeRepeating ("CreateWave", waitBeforeFirstWave, waveInterval);
    }

    //I added this because it seemed to only get randomPhrase once when i put it in createWave

    void FixedUpdate ()
    {
        randomPhrase = Random.Range (0, wavePhrases.Length);
    }

    //pick a phrase and send it to delegatePhrases

    void CreateWave () {
		gameObject.GetComponent<kidFlux> ().SortKidsByActive ();
		
		if (gameObject.GetComponent<kidFlux> ().activeKids.Count > 0) {
			string kidName = gameObject.GetComponent<kidFlux>().activeKids[Random.Range(0, gameObject.GetComponent<kidFlux> ().activeKids.Count)].GetComponent<kid>().kidName;
			selectedPhrase = wavePhrases [randomPhrase];
			GameObject.Find ("Notifications").GetComponent<notifications> ().phrases.Add (kidName + ": " + selectedPhrase);	
		} else {
			print("no kids - no wave text");
		}
    }
}