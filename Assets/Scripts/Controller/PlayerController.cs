using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private float? xFirstCollision = null;
	private float xOffset = 0;
	private bool initialised;
	private bool ongoingMove;
	private int ballsReadyCount = 0;
	private LineRenderer lineRenderer;
	private Vector2 aim = Vector2.zero;
	private Vector2 nextShotSecondDirection;

	public bool autoShooter = true;
	public bool lockAfterShooting = true;
	public float autoShooterDelay = 0.25f;
	public float aimStrength = 150;
	public float aimLength = 50f;
	public Game game;
	public List<Ball> balls;

	public float? XCollision {
		get {
			return this.xFirstCollision;
		}
	}

	public bool OngoingMove => this.ongoingMove;
	public float XOffset => this.xOffset;

    private void Start() {
	    this.lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    private void Update() {
	    if (!this.initialised)
		    return;
		
		//if (this.aim != Vector2.zero || !this.lockAfterShooting)
	    if (!this.ongoingMove) {
			RenderAim();
	    } else {
		    this.lineRenderer.SetPosition(0, Vector3.zero);
		    this.lineRenderer.SetPosition(1, Vector3.zero);
	    }

	    if (Input.GetMouseButtonDown(0) && !this.ongoingMove) {
		    this.ongoingMove = true;
		    StartCoroutine(HandleShooting());
	    }			
    }

	private IEnumerator HandleShooting() {
		int i = 0;

		foreach (Ball ball in this.balls) {
			//ball.nextDirection = RenderNextDirection(this.aim, i);
			ball.ShootTowards(this.aim);

			if (!this.autoShooter)
				break;

			i++;
			yield return new WaitForSeconds(this.autoShooterDelay);
		}

		yield return null;
	}

	private void RenderAim() {
		Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, -Camera.main.transform.position.z)) - new Vector3(this.xOffset, this.game.baseY, 0);

		float magnitude = mouse.magnitude < this.aimLength ? mouse.magnitude : this.aimLength;
		float angle = Mathf.Atan(mouse.y / mouse.x) * (180 / Mathf.PI); // Convert to degrees
		float xAim = Mathf.Cos(angle * (Mathf.PI / 180)) * magnitude;
		float yAim = Mathf.Sin(angle * (Mathf.PI / 180)) * magnitude;

		if (angle <= 0 && xAim > 0)
			xAim *= -1;

		if (yAim < 0)
			yAim *= -1;

		this.aim = new Vector2(xAim, yAim);
		//Debug.Log($"Mouse: {mouse.ToString()} Angle: {angle} | x: {xAim} | y: {yAim}");
		this.lineRenderer.SetPosition(0, new Vector3(xAim + this.xOffset, yAim, 0)); // Top line point
		this.lineRenderer.SetPosition(1, new Vector3(this.xOffset, 0, 0)); // Bottom line point

		// Draw debug lines
		RenderNextDirection(this.aim, 0);
	}

	/// <summary>
	/// Render first move shoot through controller. For some reason,
	/// if it is done through the Ball class it will glitch.
	/// </summary>
	public Vector2 RenderNextDirection(Vector2 shootingAt, int ballIndex) {
		// Simulate shooting
		int lm = 1 << 8;
		lm = ~lm;

		Vector2 hitPoint;
		Vector2 hitNormal;

		RaycastHit2D hit = Physics2D.Raycast(this.balls[ballIndex].transform.position, shootingAt.normalized, Mathf.Infinity, lm);

		if (hit.collider == null) {
			Debug.LogError("Something went wrong, no hit found [PlayerController]");
			return Vector2.zero;
		}

		hitPoint = hit.point;
		hitNormal = hit.normal;

		// Find the line from the ball to the shooting direction
		Vector2 incomingVector = hitPoint - new Vector2(this.balls[ballIndex].transform.position.x, this.balls[ballIndex].transform.position.y);

		// Use the point's normal to calculate the reflection vector.
		Vector2 reflectVector = Vector2.Reflect(incomingVector, hitNormal);

		// Draw lines to show the incoming "beam" and the reflection.
		Debug.DrawLine(this.balls[ballIndex].transform.position, hitPoint, Color.red);
		Debug.DrawRay(hitPoint, reflectVector, Color.green);

		//Debug.Log($"Hitpoint: {hitPoint.ToString()} | Reflect: {reflectVector.ToString()} | Incoming: {incomingVector.ToString()}");
		return reflectVector;
	}

	private void ResetMove() {
		//Debug.Log("Shot ended");
		this.xOffset = this.xFirstCollision ?? 0;
		this.xFirstCollision = null;
		this.ballsReadyCount = 0;
		this.ongoingMove = false;
	}

	public void Initialise(Game game, int startingBalls) {
		this.initialised = true;
		this.game = game;
		this.balls = new List<Ball>();

		for (int i = 0; i < startingBalls; i++) {
			this.balls.Add(Ball.Spawn(this, 0));
		}
	}

	public void CallShotFinished(Ball ball) {
		this.ballsReadyCount += 1;

		if (this.xFirstCollision == null)
			this.xFirstCollision = ball.xCollision;

		if (this.ballsReadyCount == this.balls.Count)
			ResetMove();
	}
}
