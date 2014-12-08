using UnityEngine;
using System.Collections;

public class chanceOfFind : MonoBehaviour {

	private int eligibleKids;
	private GameObject[] kids;
	public bool isGonnaFind;
	private float chance;
	public float baseChance;
	public float additionalChance;
	
	void Start () {
		kids = gameObject.GetComponent<kidFlux> ().kids;
	}

	void Update () {
		eligibleKids = 0;

		for (int i = 0; i < kids.Length; i++) {
			if (kids[i].activeInHierarchy && kids[i].GetComponent<kid>().kidDuty == "collect"){
				eligibleKids++;
			}	
		}

		chance = baseChance + (additionalChance * eligibleKids);

		if (Random.value < chance) {
			isGonnaFind = true;
		} else {
			isGonnaFind = false;
		}
	}
}
