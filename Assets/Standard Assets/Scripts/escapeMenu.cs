using UnityEngine;
using System.Collections;

public class escapeMenu : MonoBehaviour {
	public GameObject menu;
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.Escape)){
			menu.SetActive(true);
		}
	}

	public void QuitTheGame(){
		Application.Quit ();
	}
}