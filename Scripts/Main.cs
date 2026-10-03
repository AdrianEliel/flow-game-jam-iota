using Godot;
using System;

public partial class Main : Node2D
{
	PackedScene scene = GD.Load<PackedScene>("res://Scenes/testbox.tscn");
	FastNoiseLite fastNoiseLite = new FastNoiseLite();

	public override void _Ready()
	{
		fastNoiseLite.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		GetNode<Line2D>("Level/CurrentLine").AddPoint(new Vector2(0,0));
	}


	public override void _Process(double delta)
	{
		var Lpos = GetNode<Node2D>("Level").Position;
		Lpos.X -= 500 * (float)delta;
		
		
		
		GetNode<Node2D>("Level").Position = Lpos;
		
		
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
		GD.Print(noise1D);
		GetNode<Line2D>("Level/CurrentLine").AddPoint(new Vector2(0-Lpos.X,noise1D * 500));
	}
}
