using Godot;
using System;

public class PanelButton : Button
{
    private bool _panelOpen = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
       
        Connect("pressed", this, "OnButtonPress");
    }

    private void OnButtonPress()
    {
        DatabasePanel panel = GetNode<DatabasePanel>("../");
        panel.TogglePanel();
        
    }
}
