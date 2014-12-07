using UnityEngine;
using System.Collections;

public class placeMenus : MonoBehaviour {
	public bool isOnRightSide;
	private int multiplier;
	private RectTransform rt;
	private Vector3 pos;
	private float anchor;
	
	void Start () {
		if (isOnRightSide) {
			multiplier = 1;
		} else {
			multiplier = -1;		
		}

		anchor = multiplier * ((Screen.width / 2) - 100);

		print (anchor);

		//rt = (RectTransform)gameObject.transform;
		//pos = rt.
		//rt.position = pos;
	}
}
