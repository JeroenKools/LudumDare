using UnityEngine;
using System.Collections;

public class escapeMenu : MonoBehaviour {
	public GameObject menu;
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.Escape)){
			menu.SetActive(true);
			GameObject.Find ("GUI Sounds").GetComponent<AudioSource>().Play();
		}
	}

	public void QuitTheGame(){
		GameObject.Find ("Kid Sounds").GetComponent<AudioSource> ().PlayOneShot (GameObject.Find ("Game").GetComponent<kidFlux>().byeClips[Random.Range(0, GameObject.Find ("Game").GetComponent<kidFlux>().byeClips.Length)]);
		InvokeRepeating ("ExitApplication", 2, 0);
	}

	void ExitApplication(){
		print ("QUITTT");
		Application.Quit ();
	}
}