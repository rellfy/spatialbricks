using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	[SerializeField]
	private Transform world;

	public float baseY = -52f;
	public PlayerController playerController;

	public Transform World {
		get { return this.world; }
	}

	private void Start() {
		Debug.Log("Starting game");
		this.playerController.Initialise(this, 50);
	}
}
