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
	public int delay;
	public int interval;
	
	void Start () {
		InvokeRepeating ("Flux", delay, interval);
	}

	void Flux () {
		activeKids.Clear();
		inactiveKids.Clear();

		for(int i = 0; i < kids.Length; i++){
			if (kids[i].activeInHierarchy){
				activeKids.Add(kids[i]);
			}else {
				inactiveKids.Add (kids[i]);
			}
		}

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

		inactiveKids [randomKid].transform.Find ("disk").GetComponent<RawImage> ().texture = disks [Random.Range (0, disks.Length)];
		inactiveKids [randomKid].SetActive (true);
		GameObject.Find("Notifications").GetComponent<notifications> ().phrases.Add (selectedPhrase);

	}

	void removeKid(){
		print ("remove kid");
		int randomKid = Random.Range (0, inactiveKids.Count - 1);
		string selectedPhrase = deactivatePhrases [Random.Range (0, deactivatePhrases.Length)];

		GameObject.Find("Notifications").GetComponent<notifications> ().phrases.Add (selectedPhrase);
		activeKids [randomKid].SetActive (false);
	}
}
