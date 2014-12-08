using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kidFlux : MonoBehaviour {

	public GameObject[] kids;
	//public int numKidsPresent;
	public List<GameObject> activeKids = new List<GameObject>();
	public List<GameObject> inactiveKids = new List<GameObject>();
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
			addKid();
		} else if(activeKids.Count > 0) {
			removeKid();
		}
	}

	void addKid(){
		print ("add kid");
		inactiveKids [Random.Range (0, inactiveKids.Count - 1)].SetActive (true);
	}

	void removeKid(){
		print ("remove kid");
		activeKids [Random.Range (0, activeKids.Count - 1)].SetActive (false);
	}
}
