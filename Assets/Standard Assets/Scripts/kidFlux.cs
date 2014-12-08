using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class kidFlux : MonoBehaviour {

	public GameObject[] kids;
	public List<GameObject> activeKids = new List<GameObject>();
	public List<GameObject> inactiveKids = new List<GameObject>();
	public Texture[] disks = new Texture[3];
	public string[] activatePhrases = new string[10];
	public string[] deactivatePhrases = new string[10];
	public string[] kidNames = new string[10];
	public int delay;
	public int interval;
	
	void Start () {
		InvokeRepeating ("Flux", delay, interval);
	}

	public void SortKidsByActive() {
		activeKids.Clear();
		inactiveKids.Clear();
		
		for(int i = 0; i < kids.Length; i++){
			if (kids[i].activeInHierarchy){
				activeKids.Add(kids[i]);
			}else {
				inactiveKids.Add (kids[i]);
			}
		}
	}

	void Flux () {
		SortKidsByActive ();

		print ("A:" + activeKids.Count + " I:" + inactiveKids.Count);

		if (gameObject.GetComponent<coolness> ().isCool && activeKids.Count < 3) {
			addKid ();
		} else if (activeKids.Count > 0) {
			removeKid ();
		} else {
			print ("not cool enough");
		}
	}

	void addKid(){
		print ("add kid");
		int randomKid = Random.Range (0, inactiveKids.Count - 1);
		string selectedPhrase = activatePhrases [Random.Range (0, activatePhrases.Length)];

		//change color
		inactiveKids [randomKid].transform.Find ("disk").GetComponent<RawImage> ().texture = disks [Random.Range (0, disks.Length)];

		//change kid's name
		inactiveKids [randomKid].GetComponent<kid> ().kidName = kidNames [Random.Range (0, kidNames.Length)];

		//add greeting to notifications
		GameObject.Find("Notifications").GetComponent<notifications> ().phrases.Insert (0, inactiveKids[randomKid].GetComponent<kid>().kidName + ": " + selectedPhrase);

		//activate kid
		inactiveKids [randomKid].SetActive (true);

		//play sound
		GameObject.Find ("Kid Sounds").GetComponent<AudioSource> ().Play ();
	}

	void removeKid(){
		print ("remove kid");
		int randomKid = Random.Range (0, inactiveKids.Count - 1);
		string selectedPhrase = deactivatePhrases [Random.Range (0, deactivatePhrases.Length)];

		//add farewell to notifications
		GameObject.Find("Notifications").GetComponent<notifications> ().phrases.Insert (0, activeKids[randomKid].GetComponent<kid>().kidName + ": " + selectedPhrase);

		//deactivate kid
		activeKids [randomKid].SetActive (false);
	}
}
