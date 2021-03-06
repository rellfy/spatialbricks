﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEngine;

public class Ball : MonoBehaviour {

	private bool moving;
	private bool horizontalGlitch;
	private bool isRetracting = false;
	private PlayerController controller;
	private new Rigidbody2D rigidbody;
	private new CircleCollider2D collider;
	private Vector2 previousDirection;

	public int moveCollisions = 0;
	public float xCollision;

	public bool Retracting => this.isRetracting;

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

		if (this.isRetracting && transform.position != new Vector3(this.controller.XCollision ?? this.controller.XOffset, this.controller.game.baseY, 0)) {
			transform.position = Vector3.MoveTowards(transform.position, new Vector3(this.controller.XCollision ?? this.controller.XOffset, this.controller.game.baseY, 0), 400f * Time.deltaTime);
		} else if (this.isRetracting) {
			this.isRetracting = false;
			this.collider.enabled = true;
			EndMove();
		}
	}

	private void EndMove() {
		this.rigidbody.velocity = Vector2.zero;
		this.rigidbody.angularVelocity = 0;
		this.moveCollisions = 0;
		Moving = false;

		this.xCollision = transform.position.x;
		this.controller.CallShotFinished(this);
		transform.position = new Vector3(this.controller.XCollision ?? this.controller.XOffset, this.controller.game.baseY, 0);
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.tag == "BottomBorder" && this.controller.OngoingMove) {
			EndMove();
			return;
		}

		// Check if "border" layer
		if (collision.gameObject.layer != 9)
			return;

		if (!this.controller.OngoingMove)
			return;

		if (collision.collider.tag == "Block")
			this.controller.game.Score = this.controller.game.Score + 1;

		this.moveCollisions += 1;
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

	public Vector2 NextDirection(Vector2 shootingFrom, Collision2D collision) {
		Vector2 hitPoint = collision.contacts[0].point; ;
		Vector2 hitNormal = collision.contacts[0].normal;;

		// Find the line from the ball to the shooting direction
		Vector2 incomingVector = hitPoint - new Vector2(transform.position.x, transform.position.y);

		// Use the point's normal to calculate the reflection vector.
		Vector2 reflectVector = Vector2.Reflect(shootingFrom, hitNormal);

		//Debug.Log($"Hitpoint: {hitPoint.ToString()} | Position: {transform.position} | From: {shootingFrom} | Reflect: {reflectVector.ToString()} | Incoming: {incomingVector.ToString()}");
		Debug.DrawLine(transform.position, hitPoint, Color.red);
		Debug.DrawRay(hitPoint, reflectVector, Color.green);

		//Debug.Log($"Reflected magnitude: {reflectVector.magnitude} | Velocity magnitude: {this.rigidbody.velocity.magnitude}");

		if (Mathf.Abs(this.rigidbody.velocity.y) <= 0.1f && Mathf.Abs(this.rigidbody.velocity.x) >= 0.5f) {
			reflectVector = Vector2.down;
		} else if (this.rigidbody.velocity.magnitude <= 0.5f) {
			reflectVector = -incomingVector;
		}

		return reflectVector;
	}

	public void Retract() {
		if (this.isRetracting)
			return;

		this.rigidbody.velocity = Vector2.zero;
		this.rigidbody.angularVelocity = 0;

		this.collider.enabled = false;
		this.isRetracting = true;
	}
}
