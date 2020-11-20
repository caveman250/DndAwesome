using Godot;
using System;

public class DmWindow : Control
{
    public static DmWindow Instance { get; set; }
    public override void _Ready()
    {
        Instance = this;
    }

    public enum DmWindowType
    {
        None,
        EditBackgroundImage
    }

    public static void SetContentsType(DmWindowType type)
    {
        foreach (Node child in Instance.GetChildren())
        {
            Instance.RemoveChild(child);
            child.QueueFree();
        }

        switch (type)
        {
            case DmWindowType.EditBackgroundImage:
                PackedScene packedScene = GD.Load<PackedScene>("res://Layouts/DmScreens/EditBackgroundImageScreen.tscn");
                Node sceneNode = packedScene.Instance();
                Instance.AddChild(sceneNode);
                break;
            case DmWindowType.None:
                break;
        }
    }
}
