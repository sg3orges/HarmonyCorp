using Godot;
using System;

public partial class multi_bouton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_button_down()
	{
		var new_scene = ResourceLoader.Load<PackedScene>("res://multiplayer_controller.tscn").Instantiate<Control>();
		GetTree().Root.AddChild(new_scene);
		this.Hide();
	}
}
