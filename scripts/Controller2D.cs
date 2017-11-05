using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;
	// constant set skinWidth so it never alters
	const float skinWidth = .015f;

	// decale how many collider rays are being fired
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	public float maxClimbAngle = 80;

	// declare size of of bounds between ray and object
	float horizontalRaySpacing;
	float verticalRaySpacing;

	public CollisionInfo collisions;
	BoxCollider2D collider;
	RaycastOrigins raycastOrigins;

	void Start () {
		collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();
	}

	public void Move(Vector3 velocity) {
		UpdateRaycastOrigins ();
		collisions.Reset ();

		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}
		

	// moves transform in direction with translated velocity
		transform.Translate (velocity);
	}

	void HorizontalCollisions (ref Vector3 velocity) {
	// stores vertical velocity
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

	// uses raycast with velocity * gravity to calculate VerticalCollision
		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			// calculates the vert collision distance between objects using rayOrigin with rayLength
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

		// if Horitonal collision is detected
			if (hit) {
				// detects the angle of the slope that is being climbed
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle) {
					float distanceToSlopeStart = 0;
					// if new slope is being climbed
					// subtract velocity by the velocity generated without the skinwidth, so velocity is accurate when ClimbSlope is called
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance-skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}
				// doesnt use horitontal raycasts when climbing
				// reduce velocity.x so unit doesn't go through collision
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
				velocity.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;

				// transfers velocity.x to velocity.y when moving up a slopeAngle instead of resetting to 0
				// prevents bouncing when colliding with objects when moving up slopes
				if (collisions.climbingSlope) {
					velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}
				}
			// on collision in respective direction, reset physics
				collisions.left = directionX == -1;
				collisions.right = directionX == 1;
			}
		}
	}


	void VerticalCollisions (ref Vector3 velocity) {
	// stores vertical velocity
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

	// uses raycast with velocity * gravity to calculate VerticalCollision
		for (int i = 0; i < verticalRayCount; i ++) {
			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			// calculates the vert collision distance between objects using rayOrigin with rayLength
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

	// display visable red lines coming from Player for debugging raycasts
			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

	// 
			if (hit) {
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
	}


	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		// calculates velocity going up slope to determine if it can be climbed
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		// if move velocity is faster than climb velocity, assume jumping
		// tells game youre grounded while climbing
		if (velocity.y <= climbVelocityY) {
		velocity.y = climbVelocityY;
		velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
		collisions.below = true;
		collisions.climbingSlope = true;
		collisions.slopeAngle = slopeAngle;
		}
	}

	void UpdateRaycastOrigins () {
		Bounds bounds = collider.bounds;
		// shrink collider to skinWidth so sprites sit naturally
		bounds.Expand (skinWidth * -2);
		
		// calculates where to cast rays from, uses Vector2 to determine and use edges of the bounds box
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing() {
		// creates a Bounds box around the object to shoot rays from for calculations
		// Sets the bounds box to skinWidth * -2 so it is slightly within the target object
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);
		
		// Verifies at least 2 rays are fired from the object for calculations
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		// calculates spacing between each ray and colliders
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount -1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount -1);
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public float slopeAngle, slopeAngleOld;

		public void Reset () {
			above = below = false;
			left = right = false;
			climbingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

}
