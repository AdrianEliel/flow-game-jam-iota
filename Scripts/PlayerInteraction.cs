using Godot;

// Trigger areas detect the player without blocking its movement.
public partial class PlayerInteraction : Area2D
{
	public enum InteractionKind { Coin, Puffer, Monster }
	[Export] public InteractionKind Kind { get; set; }
	private bool consumed;

	public override void _Ready() => BodyEntered += OnBodyEntered;

	private void OnBodyEntered(Node2D body)
	{
		if (consumed || body is not CharacterBody2d player || player.IsDead) return;
		switch (Kind)
		{
			case InteractionKind.Coin:
				consumed = true;
				player.CollectCoin();
				Hide();
				QueueFree();
				break;
			case InteractionKind.Puffer:
				player.ApplySlow();
				break;
			case InteractionKind.Monster:
				player.TakeDamage();
				break;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Continued contact can hurt again only after the player's grace period.
		if (Kind == InteractionKind.Monster)
		{
			foreach (var body in GetOverlappingBodies())
				if (body is CharacterBody2d player) player.TakeDamage();
		}
		else if (GlobalPosition.X < -2200) QueueFree();
	}
}
