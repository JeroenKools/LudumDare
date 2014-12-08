using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class delegatePhrases : MonoBehaviour {


	public GameObject[] kids;
	public List<string> phrases = new List<string>();
	private List<GameObject> eligibleKids = new List<GameObject>();
	private int randomKid;
	
	void Update () {

		//find kids that are active and put them in eligibleKids

		for (int i = 0; i < kids.Length - 1; i++){
			if(kids[i].activeInHierarchy){
				eligibleKids.Add(kids[i]);
			}
		}

		//assign phrases to eligible kids until all phrases are accounted for

		while (phrases.Count > 0 && eligibleKids.Count > 0) {
			//print (eligibleKids.Count);
			randomKid = Random.Range(0, eligibleKids.Count);
			eligibleKids[randomKid].GetComponent<kid>().phrases.Add(phrases[0]);
			phrases.RemoveAt(0);
		}
	}
}
