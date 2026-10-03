using Godot;
using System;

public partial class Main : Node2D
{
	PackedScene scene = GD.Load<PackedScene>("res://Scenes/testbox.tscn");

	public override void _Ready()
	{
		
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
}
