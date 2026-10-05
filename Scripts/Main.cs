using Godot;
using System;

public partial class Main : Node2D
{
	PackedScene scene = GD.Load<PackedScene>("res://Scenes/testbox.tscn");
	PackedScene coins = GD.Load<PackedScene>("res://Scenes/coin.tscn");
	FastNoiseLite fastNoiseLite = new FastNoiseLite();
	int layer = 0;
	[Export] public float CruiseSpeed { get; set; } = 1300;
	[Export] public float FlowSpeed { get; set; } = 1600;
	[Export] public float MonsterSpeed { get; set; } = 1480;
	[Export] public float StartingMonsterOffset { get; set; } = -400;
	[Export] public float MonsterScreenLimit { get; set; } = 0;
	float chaseOffset;
	float cameraLead;
	float playerRestX;
	float levelspeed;
	float speedgoal;
	int levelaccel = 600;
	bool Decelcase = false;
	CharacterBody2d player;
	Label status;

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (player.IsDead && @event is InputEventKey key && key.Pressed && !key.Echo && key.PhysicalKeycode == Key.R)
			GetTree().ReloadCurrentScene();
	}

	public override void _Ready()
	{
		player = GetNode<CharacterBody2d>("CharacterBody2D");
		levelspeed = speedgoal = CruiseSpeed;
		playerRestX = player.Position.X;
		chaseOffset = StartingMonsterOffset;
		cameraLead = Mathf.Max(0, chaseOffset - MonsterScreenLimit);
		player.Position = new Vector2(playerRestX - cameraLead, player.Position.Y);
		GetNode<Node2D>("monstertotal").Position = new Vector2(chaseOffset - cameraLead, 0);
		var hud = new CanvasLayer();
		AddChild(hud);
		status = new Label { Position = new Vector2(24, 24) };
		status.AddThemeFontSizeOverride("font_size", 32);
		hud.AddChild(status);
		fastNoiseLite.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		GetNode<Line2D>("Level/CurrentLine").AddPoint(new Vector2(0,0));
	}


	public override void _PhysicsProcess(double delta)
	{
		status.Text = $"Health: {player.Health}/{player.MaxHealth}    Coins: {player.Coins}";
		if (player.IsDead)
		{
			status.Text += "    Game over — press R to restart";
			foreach (var child in GetChildren())
				if (child is Timer timer) timer.Stop();
			return;
		}
		if (player.IsSlowed) status.Text += "    Slowed!";
		var Lpos = GetNode<Node2D>("Level").Position;
		float targetSpeed = speedgoal * (player.IsSlowed ? player.SlowMultiplier : 1);
		levelspeed = Mathf.MoveToward(levelspeed, targetSpeed, levelaccel * (float)delta);
		// Keep the chase in world space, but let the camera advance with the
		// monster once it reaches the left-side framing limit. The sub then
		// falls back instead of the monster crossing the middle of the screen.
		chaseOffset += (MonsterSpeed - levelspeed) * (float)delta;
		float nextLead = Mathf.Max(0, chaseOffset - MonsterScreenLimit);
		Lpos.X -= levelspeed * (float)delta + nextLead - cameraLead;
		cameraLead = nextLead;
		GetNode<Node2D>("Level").Position = Lpos;
		GetNode<Node2D>("monstertotal").Position = new Vector2(chaseOffset - cameraLead, 0);
		player.Position = new Vector2(playerRestX - cameraLead, player.Position.Y);

		// Once overtaken offscreen, leaving the finite monster polygon must
		// not make the submarine safe again. TakeDamage preserves its cooldown.
		var camera = GetNode<Camera2D>("Camera2D");
		float leftEdge = camera.Position.X - GetViewportRect().Size.X / (2 * camera.Zoom.X);
		if (player.Position.X + 64 < leftEdge) player.TakeDamage();

	}
	
	public void On_Testboxspawn_Timeout(){
		var instance = scene.Instantiate();
		GetNode<Node2D>("Level").AddChild(instance);
		var temppos = ((Node2D)instance).GlobalPosition;
		temppos.X = 2000;
		Random rnd = new Random();
		temppos.Y = rnd.Next(-400, 400);
		((Node2D)instance).GlobalPosition = temppos;
	}
	
	public void _on_coinspawn_timeout(){
		var instance = coins.Instantiate();
		GetNode<Node2D>("Level").AddChild(instance);
		var temppos = ((Node2D)instance).GlobalPosition;
		temppos.X = 2000;
		Random rnd = new Random();
		temppos.Y = rnd.Next(-400, 400);
		((Node2D)instance).GlobalPosition = temppos;
	}
	
	public void On_Point_Add_Timeout(){
		var Lpos = GetNode<Node2D>("Level").Position;
		float noise1D = fastNoiseLite.GetNoise1D(Lpos.X);
		GetNode<Line2D>("Level/CurrentLine").AddPoint(new Vector2(-Lpos.X+1500,noise1D * 100 + layer*350));
		var points = GetNode<Line2D>("Level/CurrentLine").Points;
		if(points.Length > 1){
			var linecoll = new CollisionShape2D();
			GetNode<Area2D>("Level/CurrentLine/Area2D").AddChild(linecoll);
			var rect = new RectangleShape2D();
			linecoll.Position = (points[points.Length-2] + points[points.Length-1]) / 2;
			var pos = linecoll.Position;
			pos.Y += 75;
			linecoll.Position = pos;
			linecoll.Rotation = points[points.Length-2].DirectionTo(points[points.Length-1]).Angle();
			var length = points[points.Length-2].DistanceTo(points[points.Length-1]);
			rect.Size = new Vector2(length, 160);
			linecoll.Shape = rect;
		}
		
		if (noise1D > 0.3 && layer < 1){
			layer += 1;
		}
		if (noise1D < -0.3 && layer > -1){
			layer -= 1;
		}
		
		for (int i = 0; i < points.Length; i++){
			if (points[i].X<-Lpos.X-2500){
				GetNode<Line2D>("Level/CurrentLine").RemovePoint(i);
			}
		}
	}
	
	public void On_Decelbuffer_Timeout(){
		if (Decelcase){
			speedgoal = CruiseSpeed;
		}
		Decelcase = false;
		//Tween tween = GetTree().CreateTween();
		//tween.TweenProperty(GetNode("Camera2D"), "zoom", new Vector2(1f,1f), 1f);
	}
	
	public void On_Area_2D_Body_Entered(Node2D body){
		if (body != player || player.IsDead) return;
		speedgoal = FlowSpeed;
		Decelcase = false;
		GetNode<Timer>("Decelbuffer").Stop();
		//Tween tween = GetTree().CreateTween();
		//tween.TweenProperty(GetNode("Camera2D"), "zoom", new Vector2(1.05f,1.05f), 0.1f);
	}
	
	public void On_Area_2D_Body_Exited(Node2D body){
		if (body != player || player.IsDead) return;
		var buffer = GetNode<Timer>("Decelbuffer");
		if (!buffer.IsInsideTree()) return;
		buffer.Start();
		Decelcase = true;
	}
}
