using Godot;

public partial class CharacterBody2d : CharacterBody2D
{
	[Export] public float Speed { get; set; } = 600.0f;
	[Export] public int MaxHealth { get; set; } = 3;
	[Export] public float SlowDuration { get; set; } = 2.0f;
	[Export] public float SlowMultiplier { get; set; } = 0.5f;
	[Export] public float DamageGracePeriod { get; set; } = 1.5f;
	public int Health { get; private set; }
	public int Coins { get; private set; }
	public bool IsDead => Health <= 0;
	public bool IsSlowed => slowRemaining > 0;
	private float slowRemaining;
	private float damageCooldown;

	public override void _Ready() => Health = MaxHealth;

	public void CollectCoin()
	{
		if (!IsDead) Coins++;
	}

	public void ApplySlow()
	{
		if (!IsDead) slowRemaining = SlowDuration;
	}

	public void TakeDamage()
	{
		if (IsDead || damageCooldown > 0) return;
		Health--;
		damageCooldown = DamageGracePeriod;
	}

	public override void _PhysicsProcess(double delta)
	{
		slowRemaining = Mathf.Max(0, slowRemaining - (float)delta);
		damageCooldown = Mathf.Max(0, damageCooldown - (float)delta);
		var sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.Modulate = damageCooldown > 0
			? new Color(1, 0.4f, 0.4f, 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(damageCooldown * 15)))
			: IsSlowed ? new Color(0.6f, 0.8f, 1) : Colors.White;
		if (IsDead)
		{
			Velocity = Vector2.Zero;
			return;
		}
		float direction = Input.GetAxis("up", "down");
		Velocity = new Vector2(0, direction * Speed * (IsSlowed ? SlowMultiplier : 1));
		sprite.Rotation = direction * 0.5f;
		MoveAndSlide();
		Position = new Vector2(Position.X, Mathf.Clamp(Position.Y, -470, 470));
	}
}
