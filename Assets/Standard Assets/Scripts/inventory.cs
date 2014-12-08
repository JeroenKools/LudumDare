using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class inventory : MonoBehaviour {

	public GameObject[] menuSlots = new GameObject[3];
	public List<string> items = new List<string>();

	public void PopulateMenu () {
		for (int i = 0; i < 3; i++) {
			menuSlots[i].GetComponent<Text>().text = items[i];
		}
	}
}
