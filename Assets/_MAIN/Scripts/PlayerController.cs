using UnityEngine;

public class Player : MonoBehaviour
{
	public float maxSpeed = 7f;
	public float jumpForce = 5f;
	
    Transform mTransform;
	Rigidbody2D mRigidBody;
	SpriteRenderer mSpriteRenderer;
	Animator mAnimator;

	bool mbOnGround = false;
	bool mbJumping = false;
	bool mbFacingRight = true;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		mTransform = GetComponent<Transform>();
		mRigidBody = GetComponent<Rigidbody2D>();
		mSpriteRenderer = GetComponent<SpriteRenderer>();
		mAnimator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update()
	{
		// Stop player
		if (Input.GetButtonUp("Horizontal"))
		{
			mRigidBody.linearVelocityX = 0f;
		}

		// Jumping
		if (!mbJumping && mbOnGround && Input.GetButtonDown("Jump"))
		{
			mRigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			mbJumping = true;
		}
		else if (mbOnGround && mRigidBody.linearVelocityY < 0.0001f)
		{
			mbJumping = false;
		}

		// sprite direction
		if (!mbFacingRight && mRigidBody.linearVelocityX > 0.01f)
		{
			flip();
		}
		else if (mbFacingRight && mRigidBody.linearVelocityX < -0.01f)
		{
			flip();
		}

		// animation
		mAnimator.SetFloat("VelocityX", Mathf.Abs(mRigidBody.linearVelocity.x));
		mAnimator.SetFloat("VelocityY", Mathf.Abs(mRigidBody.linearVelocity.y));
		mAnimator.speed = Mathf.Max(Mathf.Abs(mRigidBody.linearVelocity.x), 1f);
		mAnimator.SetBool("Jump", mbJumping);
	}

	void FixedUpdate()
	{
		mRigidBody.AddForce(Vector2.right * Input.GetAxisRaw("Horizontal"), ForceMode2D.Impulse);

		if (mRigidBody.linearVelocityX > maxSpeed)
		{
			mRigidBody.linearVelocityX = maxSpeed;
		}
		else if (mRigidBody.linearVelocityX < -maxSpeed)
		{
			mRigidBody.linearVelocityX = -maxSpeed;
		}

		RaycastHit2D groundHit = Physics2D.Raycast(mRigidBody.position, Vector2.down, 0.6f, LayerMask.GetMask("Platform"));
		mbOnGround = groundHit.collider != null;
	}

	private void flip()
	{
		mbFacingRight = !mbFacingRight;
		Vector3 localScale = mTransform.localScale;
		localScale.x *= -1;
		mTransform.localScale = localScale;
	}

}