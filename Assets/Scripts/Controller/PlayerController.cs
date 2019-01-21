using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private bool ongoingMove;
	private float? xFirstCollision = null;
	private float xOffset = 0;
	private bool initialised;
	private bool isRetracting;
	private int ballsReadyCount = 0;
	private LineRenderer lineRenderer;
	private Vector2 aim = Vector2.zero;
	private Vector2 nextShotSecondDirection;

	public int moveOxygenCollected = 0;
	public bool autoShooter = true;
	public bool lockAfterShooting = true;
	public float autoShooterDelay = 0.25f;
	public float aimStrength = 150;
	public float aimLength = 25f;
	public Game game;
	public Block[] blocks;
	public List<Ball> balls;

	public float? XCollision {
		get {
			return this.xFirstCollision;
		}
	}

	public bool OngoingMove => this.ongoingMove;
	public float XOffset => this.xOffset;

	public bool CanRetract {
		get {
			foreach (Ball ball in this.balls) {
				if (!ball.Retracting && (ball.Moving || ball.moveCollisions > 0))
					continue;
				
				return false;
			}

			return true;
		}
	}

    private void Start() {
	    this.lineRenderer = gameObject.GetComponent<LineRenderer>();
	    this.blocks = GameObject.FindObjectsOfType<Block>();
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

	    if (Input.GetMouseButtonDown(1) && !this.isRetracting && CanRetract) {
		    this.isRetracting = true;
		    foreach (Ball ball in this.balls) {
			    ball.Retract();
		    }
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

		//float magnitude = mouse.magnitude < this.aimLength ? mouse.magnitude : this.aimLength;
		float angle = Mathf.Atan(mouse.y / mouse.x) * (180 / Mathf.PI); // Convert to degrees
		float xAim = Mathf.Cos(angle * (Mathf.PI / 180)) * 1;
		float yAim = Mathf.Sin(angle * (Mathf.PI / 180)) * 1;

		if (angle <= 0 && xAim > 0)
			xAim *= -1;

		if (yAim < 0)
			yAim *= -1;

		this.aim = new Vector2(xAim, yAim);
		//Debug.Log($"Mouse: {mouse.ToString()} Angle: {angle} | x: {xAim} | y: {yAim}");

		// Draw debug lines
		RenderNextDirection(this.aim, 0);
	}

	/// <summary>
	/// Render first move shoot through controller. For some reason,
	/// if it is done through the Ball class it will glitch.
	/// </summary>
	public Vector2 RenderNextDirection(Vector2 shootingAt, int ballIndex) {
		// Simulate shooting
		int lm = 1 << 8 | 1 << 0;
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

		// Draw debug lines
		Debug.DrawLine(this.balls[ballIndex].transform.position, hitPoint, Color.red);
		Debug.DrawRay(hitPoint, reflectVector, Color.green);

		//Debug.Log($"Hitpoint: {hitPoint.ToString()} | Position: {transform.position} | From: {shootingAt} | Reflect: {reflectVector.ToString()} | Incoming: {incomingVector.ToString()}");

		// Draw game lines
		Vector2 reflectionLine = -(hit.point - reflectVector).normalized * this.aimLength;
		this.lineRenderer.SetPosition(0, new Vector3(reflectionLine.x, reflectionLine.y, 0)); // Top line point
		this.lineRenderer.SetPosition(1, new Vector3(incomingVector.x, incomingVector.y, 0)); // Middle line point
		this.lineRenderer.SetPosition(2, new Vector3(this.xOffset, 0, 0)); // Bottom line point
																		   //Debug.Log($"Hitpoint: {hitPoint.ToString()} | Reflect: {reflectVector.ToString()} | Incoming: {incomingVector.ToString()}");
		return reflectVector;
	}

	private void ResetMove() {
		//Debug.Log("Shot ended");
		this.xOffset = this.xFirstCollision ?? 0;
		this.xFirstCollision = null;
		this.ballsReadyCount = 0;
		this.ongoingMove = false;
		this.isRetracting = false;

		foreach (Block block in this.blocks) {
			if (block == null)
				continue;

			if (this.moveOxygenCollected == 0) {
				block.StepDown();
			} else {
				block.StepUp(this.moveOxygenCollected);
			}
		}

		this.moveOxygenCollected = 0;
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
