using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour {

	private int health;
	private Game game;
	private TextMeshPro healthText;
	private SpriteRenderer sprite;

	public int initialHealth;

	public int maxBlockGreyscale = 80;
	public int minBlockGreyscale = 10;

	public int Health {
		get {
			return this.health;
		}
		set {
			if (value <= 0)
				GameObject.Destroy(gameObject);

			this.healthText.text = value.ToString();
			this.health = value;
			SetColor();
		}
	}

	private void Start() {
		this.healthText = gameObject.GetComponentInChildren<TextMeshPro>();
		this.sprite = gameObject.GetComponent<SpriteRenderer>();

		this.initialHealth = 20;
		Health = this.initialHealth;
	}

	private void SetColor() {
		float ratio = (float)this.health / (float)this.initialHealth;
		float greyscale = ((this.maxBlockGreyscale - this.minBlockGreyscale) * ratio) + this.minBlockGreyscale;
		this.sprite.color = new Color(greyscale/100, greyscale/100, greyscale/100);
	}

	public void OnCollisionEnter2D(Collision2D collision) {
		Health -= 1;
	}
}
