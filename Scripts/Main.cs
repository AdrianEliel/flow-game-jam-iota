using Godot;
using System;

public partial class Main : Node2D
{
	PackedScene scene = GD.Load<PackedScene>("res://Scenes/testbox.tscn");
	FastNoiseLite fastNoiseLite = new FastNoiseLite();
	int layer = 0;
	float levelspeed = 500;
	int speedgoal = 500;
	int levelaccel = 600;
	bool Decelcase = false;
	bool forwardmonster = false;
	bool monsterin = true;

	public override void _Ready()
	{
		fastNoiseLite.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		GetNode<Line2D>("Level/CurrentLine").AddPoint(new Vector2(0,0));
	}


	public override void _Process(double delta)
	{
		var Lpos = GetNode<Node2D>("Level").Position;
		if (levelspeed < speedgoal){
			levelspeed += levelaccel * (float)delta;
		}
		if (levelspeed > speedgoal){
			levelspeed -= levelaccel * (float)delta;
		}
		Lpos.X -= levelspeed * (float)delta;
		
		
		
		GetNode<Node2D>("Level").Position = Lpos;
		
		var monsterpos = GetNode<Sprite2D>("monstertotal/seamonster").Position;
		
		if (forwardmonster && monsterpos.X < -20){
			monsterpos.X += 300 * (float)delta;
		}
		if (!forwardmonster && monsterpos.X > -300){
			monsterpos.X -= 300 * (float)delta;
		}
		
		GetNode<Sprite2D>("monstertotal/seamonster").Position = monsterpos;
		
		var monsterpos2 = GetNode<Node2D>("monstertotal").Position;
		
		if (monsterin && monsterpos2.X < 0){
			monsterpos2.X += 600 * (float)delta;
		}
		if (!monsterin && monsterpos2.X > -1000){
			monsterpos2.X -= 600 * (float)delta;
		}
		
		GetNode<Node2D>("monstertotal").Position = monsterpos2;
				
	}
	
	public void On_Testboxspawn_Timeout(){
		var instance = scene.Instantiate();
		GetNode<Node2D>("Level").AddChild(instance);
		var temppos = ((Node2D)instance).GlobalPosition;
		temppos.X = 0;
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
		
		if (noise1D > 0.5 && layer < 1){
			layer += 1;
		}
		if (noise1D < -0.5 && layer > -1){
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
			speedgoal = 500;
			monsterin = true;
		}
		Decelcase = false;
		//Tween tween = GetTree().CreateTween();
		//tween.TweenProperty(GetNode("Camera2D"), "zoom", new Vector2(1f,1f), 1f);
	}
	
	public void _on_monsteranim_timeout(){
		if (forwardmonster){
			forwardmonster = false;
		}
		else {
			forwardmonster = true;
		}
	}
	
	public void On_Area_2D_Body_Entered(Node2D body){
		speedgoal = 1500;
		Decelcase = false;
		monsterin = false;
		//Tween tween = GetTree().CreateTween();
		//tween.TweenProperty(GetNode("Camera2D"), "zoom", new Vector2(1.05f,1.05f), 0.1f);
	}
	
	public void On_Area_2D_Body_Exited(Node2D body){
		GetNode<Timer>("Decelbuffer").Start();
		Decelcase = true;
	}
}
