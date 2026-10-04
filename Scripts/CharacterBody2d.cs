using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
	public const float Speed = 600.0f;
	public const float JumpVelocity = -400.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.

		// Handle Jump.

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float direction = Input.GetAxis("up", "down");
		if (direction != 0)
		{
			velocity.Y = direction * Speed;
		}
		else
		{
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}
		
		if (direction > 0){
			GetNode<Sprite2D>("Sprite2D").Rotation = 0.5f;
		}
		else if (direction < 0){
			GetNode<Sprite2D>("Sprite2D").Rotation = -0.5f;
		}
		else{
			GetNode<Sprite2D>("Sprite2D").Rotation = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
