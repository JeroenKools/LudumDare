using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class kid : MonoBehaviour {

	public string kidName;
	public string kidDuty;
	private Texture disk;

	void Update () {
		disk = this.transform.Find ("disk").GetComponent<RawImage> ().texture;

		if (disk == GameObject.Find ("Game").GetComponent<kidFlux>().disks [0]) {
			kidDuty = "build";
		}else if (disk == GameObject.Find ("Game").GetComponent<kidFlux>().disks [1]){
			kidDuty = "repair";
		}else if (disk == GameObject.Find ("Game").GetComponent<kidFlux>().disks [2]){
			kidDuty = "collect";
		}
	}
}
