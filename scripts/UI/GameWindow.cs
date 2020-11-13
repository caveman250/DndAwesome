using Godot;
using System;
using DndAwesome.scripts;
using Camera2D = Godot.Camera2D;

public class GameWindow : TextureRect
{
    public override void _Ready()
    {
        SetProcessInput(true);
        Connect("resized", this, "OnWindowResized");
        SceneObjectManager.SetGameWindow(this);
    }

    public override void _Input(InputEvent @event)
    {
        SceneObjectManager.GetCamera()._Input(@event);
        foreach (Token token in SceneObjectManager.GetTokens())
        {
            token._Input(@event);
        }

        base._Input(@event);
    }

    private void OnWindowResized()
    {
        SceneObjectManager.GetOffscreenViewport().Size = RectSize;
    }
}
