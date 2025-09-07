using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private AnimatedSprite2D sprite;
	private Vector2 lastDirection = Vector2.Down;

	public const float WalkSpeed = 50.0f;
	public const float RunSpeed = 100.0f;
	public const float JumpVelocity = -400.0f;

	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("Idle_Down");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		GD.Print(Input.IsActionJustPressed("ui_accept"));
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		float horizontal = direction.X;
		float vertical = direction.Y;
		bool isRunning = Input.IsActionPressed("ui_shift");

		float moveSpeedX = isRunning ? RunSpeed : WalkSpeed;
		float moveSpeedY = isRunning ? RunSpeed : WalkSpeed;

		velocity.X = horizontal * moveSpeedX;
		velocity.Y = vertical * moveSpeedY;

		if (direction != Vector2.Zero)
		{
			lastDirection = direction;

			if (Mathf.Abs(horizontal) > 0)
			{
				//1 = right
				sprite.FlipH = horizontal > 0;
				sprite.Play(isRunning ? "Run" : "Walk");
			}
			else if (vertical < 0)
			{
				sprite.Play(isRunning ? "Run_Up" : "Walk_Up");
			}
			else
			{
				sprite.Play(isRunning ? "Run_Down" : "Walk_Down");
			}
		}
		else
		{
			if (lastDirection.Y < 0)
			{
				sprite.Play("Idle_Up");
			}
			else if (lastDirection.Y > 0)
			{
				sprite.Play("Idle_Down");
			}
			else
			{
				GD.Print(lastDirection.X);
				//1 = right
				sprite.FlipH = lastDirection.X > 0;
				sprite.Play("Idle");
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
