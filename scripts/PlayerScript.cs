using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

	// timeToJumpApex = how long player takes to hit peak of jump
	// jumpHeight = peak jump in units
	public float jumpHeight = 4;
	public float timeToJumpApex = 4f;
	float moveSpeed = 6;
	// used to calculate acceleration when in the air or on-ground
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	// Values are determined by jumpHeight, timeToApex, and moveSpeed;
	float gravity;
	float jumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	// Grabs controller from Controller2D.cs
	Controller2D controller;

	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		print ("Gravity: " + gravity + "  jumpVelocity: " + jumpVelocity);
	}
	
	// Update is called once per frame
	void Update () {

		// when collisions occurs above or below, reset vertical velocy.
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below) {
			velocity.y = jumpVelocity;
		}

		// calculates rate of velocity and gravity with time
		float targetVelocityX = input.x * moveSpeed;
		// calculates horitontal velocity. Calls velocityXSmoothing - if grounded, used TimeGrounded otherwise uses TimeAirborne
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;

		// uses velocity * Time.deltaTime (velocity * movement over time) to move character
		controller.Move (velocity * Time.deltaTime);
	}
}
