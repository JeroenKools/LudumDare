using UnityEngine;
using System.Collections;

public class followMouse : MonoBehaviour {
	private int offsetX = 100;
	private int offsetY = 0;
	
	// Update is called once per frame
	void FixedUpdate () {
		this.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (Input.mousePosition.x - (Screen.width/2) + offsetX, Input.mousePosition.y - (Screen.height/2) + offsetY);
	}
}
