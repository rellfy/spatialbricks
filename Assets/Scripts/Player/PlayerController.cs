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
		foreach (Ball ball in this.balls) {
			if (ball.Shot)
				continue;

			ball.ShootTowards(this.aim);

			if (!this.autoShooter)
				break;

			yield return new WaitForSeconds(this.autoShooterDelay);
		}

		yield return null;
	}

	private void RenderAim() {
		Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 47.3f)) - new Vector3(this.xOffset, this.game.baseY, 0);

		float magnitude = mouse.magnitude < this.aimLength ? mouse.magnitude : this.aimLength;
		float angle = Mathf.Atan(mouse.y / mouse.x) * (180/Mathf.PI); // Convert to degrees
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

		// Simulate shooting
		int lm = 1 << 8;
		lm = ~lm;

		RaycastHit2D hit = Physics2D.Raycast(this.balls[0].transform.position, new Vector2(xAim, yAim).normalized, Mathf.Infinity, lm);

		if (hit.collider == null) {
			Debug.LogError("Something went wrong, no hit found");
			return;
		}

		// Find the line from the ball to the shooting direction
		Vector2 incomingVector = hit.point - new Vector2(this.balls[0].transform.position.x, this.balls[0].transform.position.y);

		// Use the point's normal to calculate the reflection vector.
		Vector2 reflectVector = Vector2.Reflect(incomingVector, hit.normal);

		// Draw lines to show the incoming "beam" and the reflection.
		Debug.DrawLine(this.balls[0].transform.position, hit.point, Color.red);
		Debug.DrawRay(hit.point, reflectVector, Color.green);

		/// SECOND !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		RaycastHit2D hit2 = Physics2D.Raycast(hit.point, reflectVector.normalized, Mathf.Infinity, lm);

		if (hit2.collider == null) {
			Debug.LogError("Something went wrong, no hit found [2]");
			return;
		}

		// Find the line from the ball to the shooting direction
		Vector2 incomingVector2 = hit2.point - reflectVector.normalized - hit.point;

		// Use the point's normal to calculate the reflection vector.
		Vector2 reflectVector2 = Vector2.Reflect(incomingVector2, hit2.normal);

		// Draw lines to show the incoming "beam" and the reflection.
		Debug.DrawLine(hit.point, hit2.point, Color.yellow);
		Debug.DrawRay(hit2.point, reflectVector2, Color.blue);
	}

	private void ResetMove() {
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
