using Godot;
using System;
using DndAwesome.scripts;

public class HUDPlayer : Control
{
    public override void _Ready()
    {
        DockingManager.SetCurrentWindowAreaSize(GetNode<Control>("Windows").RectSize);
    }
}
