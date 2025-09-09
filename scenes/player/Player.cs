using Godot;

public partial class Player : CharacterBody2D
{
	private AnimatedSprite2D sprite;
	private Vector2 lastDirection = Vector2.Down;

	public const float WalkSpeed = 50.0f;
	public const float RunSpeed = 100.0f;
	public const float JumpVelocity = -400.0f;

	[Signal]
    public delegate void ChangeMapEventHandler(string nextMapPath);
	public Vector2 CurrentMapSize = Vector2.Zero;


	public override void _Ready()
	{
		GD.Print($"[Player] Ready = {GetInstanceId()}");
		// ใช้ global singleton
		var mapManager = MapManager.Instance;
		if (MapManager.Instance == null)
		{
			GD.PushError("[Player] ❌ MapManager.Instance is null!");
			return;
		}

		var mapContainer = GetTree().CurrentScene.GetNodeOrNull("MapContainer");
		if (mapContainer == null)
		{
			GD.PushError("[Player] ❌ Cannot find MapContainer!");
			return;
		}
		MapManager.Instance.RegisterPlayer(this, mapContainer);

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

			//1 = right
			sprite.FlipH = horizontal > 0;
			sprite.Play(isRunning ? "Run" : "Walk");
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
				//1 = right
				sprite.FlipH = lastDirection.X > 0;
				sprite.Play("Idle");
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
