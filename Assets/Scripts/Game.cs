using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

	[SerializeField]
	private Transform world;

	[Header("World configuration")]
	public float baseY = -52f;
	[Header("Player")]
	public int initialBalls = 1;
	public PlayerController playerController;
	[Header("Blocks")]
	public int initialBlockColour = 0x808080;
	public int finalBlockCOlour = 0x111111;

	public Transform World {
		get { return this.world; }
	}

	private void Start() {
		Debug.Log("Starting game");
		this.playerController.Initialise(this, this.initialBalls);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;
	}
}
