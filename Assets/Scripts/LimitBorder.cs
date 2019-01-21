using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitBorder : MonoBehaviour {

	public bool trespassed = false;

	private void Start() {
		Debug.Log("Started");
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		Debug.Log("Tag: " + collision.collider.tag);
		if (collision.collider.tag != "Border")
			return;

		this.trespassed = true;
	}
}
