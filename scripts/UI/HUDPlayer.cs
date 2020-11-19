using Godot;
using System;
using DndAwesome.scripts;

public class HUDPlayer : Control
{
    public static Control WindowArea { get; private set; }
    public static Control DockArea { get; private set; }
    public static Control FloatingArea { get; private set; }

    public override void _Ready()
    {
        WindowArea = GetNode<Control>("Windows");
        DockArea = WindowArea.GetNode<Control>("DockArea");
        FloatingArea = WindowArea.GetNode<Control>("FloatingArea");
        
        DockingManager.SetCurrentWindowAreaSize(WindowArea.RectSize);
        DockingManager.LoadDockState();
    }
}
