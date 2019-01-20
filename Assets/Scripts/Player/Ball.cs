using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Ball : MonoBehaviour {

	public float xCollision;

	private bool shot;
	private PlayerController controller;
	private new Rigidbody2D rigidbody;
	private new CircleCollider2D collider;
	private Collider2D lastBorderCollider;
	private Vector2 reflectVector;

	public bool Shot => this.shot;

	public static Ball Spawn(PlayerController controller, float xPos) {
		GameObject ballObject = Resources.Load<GameObject>("Ball");
		Ball ball = GameObject.Instantiate(ballObject, controller.game.World).GetComponent<Ball>();
		ball.controller = controller;
		ball.gameObject.transform.position = new Vector2(xPos, controller.game.baseY + 1);
		return ball;
	}

    private void Start() {
	    this.rigidbody = gameObject.GetComponent<Rigidbody2D>();
	    this.collider = gameObject.GetComponent<CircleCollider2D>();
    }

	private void Update() {
    }

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "BottomBorder" && this.shot) {
			this.rigidbody.velocity = Vector2.zero;
			this.rigidbody.angularVelocity = 0;

			this.shot = false;
			this.xCollision = transform.position.x;
			this.controller.CallShotFinished(this);
			transform.position = new Vector3(this.controller.XCollision ?? this.controller.XOffset, this.controller.game.baseY, 0);
			return;
		}

		if (other.tag != "Border")
			return;

		Debug.Log("Shooting towards " + this.reflectVector.ToString());
		this.lastBorderCollider = other;
		this.ShootTowards(this.reflectVector);
	}

	public void ShootTowards(Vector2 direction) {
		if (this.rigidbody == null)
			this.rigidbody = gameObject.GetComponent<Rigidbody2D>();

		this.shot = true;

		this.rigidbody.velocity = Vector2.zero;
		this.rigidbody.angularVelocity = 0;

		this.rigidbody.AddForce(direction.normalized * this.controller.aimStrength);

		int lm = 1 << 8;
		lm = ~lm;

		Reflect(direction, this.lastBorderCollider ?? Physics2D.Raycast(transform.position, direction.normalized, Mathf.Infinity, lm).collider);
	}

	public void Reflect(Vector2 direction, Collider2D collider) {
		if (collider == null) {
			Debug.LogError("Something went wrong, no hit found");
			return;
		}

		Vector2 incomingVector = this.rigidbody.velocity;
		Vector2 normal = contacts[0].normal;
		Vector2 reflected = Vector2.Reflect(incomingVector, normal);

		// Find the line from the ball to the shooting direction
		Debug.Log($"Hit {collider.tag} at point {contacts[0].point.ToString()}");

		Debug.Log($"Incoming V: {incomingVector.ToString()} | Reflect V: {reflected.ToString()}, position: {transform.position.ToString()}");

		if (incomingVector == Vector2.zero || reflected == Vector2.zero) {
			Debug.LogError("Could not calculate incoming vector or reflect vector!");
			return;
		}

		// Draw lines to show the incoming "beam" and the reflection.
		Debug.DrawLine(transform.position, contacts[0].point, Color.red);
		Debug.DrawRay(contacts[0].point, reflected, Color.green);

		this.reflectVector = reflected.normalized;
		this.lastBorderCollider = null;
	}
}
