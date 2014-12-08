using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class generatesFoundStuff : MonoBehaviour {

	public int delay;
	public int interval;
	private GameObject[] kids;
	public string[] findableStuff = new string[3];
	public AudioClip itemSound;
	
	void Start () {
		kids = gameObject.GetComponent<kidFlux> ().kids;
		InvokeRepeating ("FindStuff", delay, interval);
	}

	void FindStuff () {
		if (gameObject.GetComponent<chanceOfFind> ().isGonnaFind && gameObject.GetComponent<inventory>().items.Count < 3) {
			List<GameObject> searchingKids = new List<GameObject>();

			//Find kids that are active and have duty: collect
			for(int i = 0; i < kids.Length; i++){
				if (kids[i].activeInHierarchy && kids[i].GetComponent<kid>().kidDuty == "collect"){
					searchingKids.Add (kids[i]);
				}
			}

			//pick an eligible kid
			GameObject randomKid = searchingKids [Random.Range (0, searchingKids.Count)];

			//pick an item
			string foundItem = findableStuff[Random.Range(0, findableStuff.Length)];

			//generate a message
			string messageText = randomKid.GetComponent<kid>().kidName + " found a " + foundItem;

			//add notification
			GameObject.Find("Notifications").GetComponent<notifications> ().phrases.Insert (0, messageText);

			//add to inventory
			gameObject.GetComponent<inventory>().items.Add(foundItem);

			//play a sound
			GameObject.Find ("Gui Sounds").GetComponent<AudioSource>().PlayOneShot(itemSound);
		}
	}
}
