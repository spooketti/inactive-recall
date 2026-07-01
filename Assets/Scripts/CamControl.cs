using Godot;
using System;

public partial class CamControl : Camera3D
{
    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
            Node3D parent = (Node3D)GetParent();
			parent.Rotate(Vector3.Up, Mathf.DegToRad(-mouseMotion.Relative.X) * 1);

			Rotate(Vector3.Left, Mathf.DegToRad(mouseMotion.Relative.Y) * 1);
			Rotation = Rotation with { X = Mathf.Clamp(Rotation.X, -1.57f, 1.57f) };
		}

		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Keycode == Key.Escape)
				Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}
}
