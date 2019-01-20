using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEngine;

public class Ball : MonoBehaviour {

	private bool moving;
	private bool horizontalGlitch;
	private PlayerController controller;
	private new Rigidbody2D rigidbody;
	private new CircleCollider2D collider;
	private Vector2 previousDirection;

	public int collisions = 0;
	public float xCollision;

	public bool Moving {
		get {
			return this.moving;
		}
		private set {
			this.moving = value;
		}
	}
	public Vector2 nextDirection = Vector2.zero;

	public static Ball Spawn(PlayerController controller, float xPos) {
		GameObject ballObject = Resources.Load<GameObject>("Ball");
		Ball ball = GameObject.Instantiate(ballObject, controller.game.World).GetComponent<Ball>();
		ball.controller = controller;
		ball.gameObject.transform.position = new Vector2(xPos, controller.game.baseY - 0.5f);
		return ball;
	}

    private void Start() {
	    this.rigidbody = gameObject.GetComponent<Rigidbody2D>();
	    this.collider = gameObject.GetComponent<CircleCollider2D>();
    }

	private void Update() {
		// If the magnitude is not correct, the ball is probably glitching horizontally.
		if (this.horizontalGlitch && this.rigidbody.velocity.magnitude < this.controller.aimStrength - 0.1f) {
			this.horizontalGlitch = false;
			this.ShootTowards(Vector2.down);
		}
	}	

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.tag == "BottomBorder" && this.controller.OngoingMove) {
			this.rigidbody.velocity = Vector2.zero;
			this.rigidbody.angularVelocity = 0;

			Moving = false;
			this.collisions = 0;
			this.xCollision = transform.position.x;
			this.controller.CallShotFinished(this);
			transform.position = new Vector3(this.controller.XCollision ?? this.controller.XOffset, this.controller.game.baseY, 0);
			return;
		}

		if (collision.collider.tag != "Border")
			return;

		if (!this.controller.OngoingMove)
			return;

		this.collisions += 1;
		this.nextDirection = NextDirection(this.previousDirection, collision);

		ShootTowards(this.nextDirection);
	}

	public void ShootTowards(Vector2 direction) {
		Moving = true;

		//Debug.Log("Shooting at " + direction.ToString());
		this.previousDirection = direction;

		this.rigidbody.velocity = Vector2.zero;
		this.rigidbody.angularVelocity = 0;

		// Add force in direction
		this.rigidbody.AddForce(direction.normalized * this.controller.aimStrength);
	}

	Vector2 NextDirection(Vector2 shootingFrom, Collision2D collision) {
		int lm = 1 << 8;
		lm = ~lm;

		Vector2 hitPoint = collision.contacts[0].point; ;
		Vector2 hitNormal = collision.contacts[0].normal;;

		/*RaycastHit2D hit = Physics2D.Raycast(transform.position, shootingFrom.normalized, Mathf.Infinity, lm);

		if (hit.collider == null) {
			Debug.LogWarning("Something went wrong, no hit found [PlayerController]");
			return Vector2.zero;
		}*/


		// Find the line from the ball to the shooting direction
		Vector2 incomingVector = hitPoint - new Vector2(transform.position.x, transform.position.y);

		// Use the point's normal to calculate the reflection vector.
		Vector2 reflectVector = Vector2.Reflect(shootingFrom, hitNormal);

		//Debug.Log($"Hitpoint: {hitPoint.ToString()} | Position: {transform.position} | From: {shootingFrom} | Reflect: {reflectVector.ToString()} | Incoming: {incomingVector.ToString()}");
		Debug.DrawLine(transform.position, hitPoint, Color.red);
		Debug.DrawRay(hitPoint, reflectVector, Color.green);

		//Debug.Log($"Reflected magnitude: {reflectVector.magnitude} | Velocity magnitude: {this.rigidbody.velocity.magnitude}");

		if (this.rigidbody.velocity.magnitude <= 0.5f)
			reflectVector = -incomingVector;

		return reflectVector;
	}
}
