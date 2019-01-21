using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class Block : MonoBehaviour {

	private int health;
	private bool isSteping;
	private bool setToOxygen = false;
	private Game game;
	private TextMeshPro healthText;
	private SpriteRenderer sprite;
	private Vector3 nextStep;
	private new BoxCollider2D collider;

	public Vector2 dimension = new Vector2(8, 8);
	public int maxHealth;
	public float maxBlockGreyscale = 35f;
	public float minBlockGreyscale = 5f;
	public bool isOxygen;

	public int Health {
		get {
			return this.health;
		}
		set {
			if (value <= 0)
				GameObject.Destroy(gameObject);

			this.health = value;

			if (!this.isOxygen) {
				this.healthText.text = value.ToString();
				SetColor();
			}
		}
	}

	private void Awake() {
		this.game = GameObject.FindObjectOfType<Game>();
		this.healthText = gameObject.GetComponentInChildren<TextMeshPro>();
		this.sprite = gameObject.GetComponent<SpriteRenderer>();
		this.collider = gameObject.GetComponent<BoxCollider2D>();
	}

	private void Start() {
		this.maxHealth = 20;
		Health = this.maxHealth;
	}

	private void Update() {
		if (this.isSteping && transform.position != this.nextStep) {
			transform.position = Vector3.MoveTowards(transform.position, this.nextStep, 25f * Time.deltaTime);
		} else if (this.isSteping) {
			this.isSteping = false;
		}

		if (this.isOxygen != this.setToOxygen)
			SetOxygen(this.isOxygen);
	}

	private void SetOxygen(bool set) {
		if (set) {
			this.maxHealth = 1;
			this.healthText.alpha = 0f;
			this.health = this.maxHealth;
			this.sprite.color = new Color(0f, 0.8f, 1f);
			this.collider.isTrigger = true;
		} else {
			this.maxHealth = 20;
			this.healthText.alpha = 1f;
			this.health = this.maxHealth;
			SetColor();
			this.collider.isTrigger = false;
		}

		this.isOxygen = set;
		this.setToOxygen = set;
	}

	private void SetColor() {
		float ratio = (float)this.health / (float)this.maxHealth;
		float greyscale = ((this.maxBlockGreyscale - this.minBlockGreyscale) * ratio) + this.minBlockGreyscale;
		this.sprite.color = new Color(greyscale/100f, greyscale/100f, greyscale/100f);
	}

	public void StepDown() {
		this.nextStep = new Vector2(transform.position.x, transform.position.y - this.dimension.y);
		this.isSteping = true;
	}

	public void StepUp(int steps) {
		this.nextStep = new Vector2(transform.position.x, transform.position.y + (this.dimension.y * steps));
		this.isSteping = true;
	}

	public void OnCollisionEnter2D(Collision2D collision) {
		if (this.isOxygen)
			return;

		Health -= 1;
	}

	public void OnTriggerEnter2D(Collider2D collider) {
		if (!this.isOxygen)
			return;

		this.game.playerController.moveOxygenCollected += 1;
		Health -= 1;
	}
}
