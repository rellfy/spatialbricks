using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour {

	private int score = 0;
	private bool matchEverEnded = false;
	private bool matchStarted;
	private float menuAlpha = 1.0f;
	private TextMeshProUGUI[] menuTexts;

	[Header("World configuration")]
	public float baseY = -51.6f;
	public float newRowY = 56f;
	[Header("Player")]
	public int initialBalls = 1;
	public PlayerController playerController;
	public LimitBorder limitBorder;
	public Transform world;
	public GameObject menu;
	public GameObject blockContainer;
	public GameObject startingMap;
	public TextMeshProUGUI scoreText;

	public int Score {
		get {
			return this.score;
		}
		set {
			this.score = value;
			this.scoreText.text = $"score: {value}";
		}
	}

	public Transform World {
		get { return this.world; }
	}

	private void Start() {
		Debug.Log("Starting game");
		
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;
		
		this.playerController.Initialise(this, this.initialBalls);
		this.menuTexts = this.menu.GetComponentsInChildren<TextMeshProUGUI>();

		InitialiseMenu();
	}

	private void Update() {
		if (!this.matchStarted && this.playerController.OngoingMove) {
			InitialiseMatch();
		}

		if (!this.matchStarted && Math.Round(this.menuAlpha, 3) != 1) {
			this.menuAlpha = Mathf.Lerp(this.menuAlpha, 1, 10f * Time.deltaTime);
			SetMenuItemAlpha();
		} else if (this.matchStarted && (Math.Round(this.menuAlpha, 3) != 0)) {
			this.menuAlpha = Mathf.Lerp(this.menuAlpha, 0, 10f * Time.deltaTime);
			SetMenuItemAlpha();
		}

		// End match
		if (this.limitBorder.trespassed) {
			this.matchEverEnded = true;
			InitialiseMenu();
		}
	}

	private void InitialiseMenu() {
		if (this.matchEverEnded) {
			foreach (Transform child in this.blockContainer.transform) {
				Destroy(child.gameObject);
			}

			GameObject.Instantiate(this.startingMap, this.blockContainer.transform);
		}

		this.limitBorder.trespassed = false;
		this.matchStarted = false;
		this.menu.SetActive(true);
	}

	private void InitialiseMatch() {
		this.matchStarted = true;

	}

	private void SetMenuItemAlpha() {
		foreach (TextMeshProUGUI text in this.menuTexts) {
			text.alpha = this.menuAlpha;
		}
	}
}
