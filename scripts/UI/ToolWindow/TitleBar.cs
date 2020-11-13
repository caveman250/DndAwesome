using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace DndAwesome.scripts.UI.ToolWindow
{
    public class TitleBar : Panel
    {
        public bool MouseDragging { get; set; }
        private Vector2 DragOffset { get; set; }

        public override void _Ready()
        {
        
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent is InputEventMouse mouseEvent)
            {
                if (mouseEvent is InputEventMouseButton buttonEvent)
                {
                    if (mouseEvent.Position.x >= GetGlobalRect().Position.x &&
                        mouseEvent.Position.y >= GetGlobalRect().Position.y &&
                        mouseEvent.Position.x <= GetGlobalRect().Position.x + GetRect().Size.x &&
                        mouseEvent.Position.y <= GetGlobalRect().Position.y + GetRect().Size.y)
                    {
                        Console.WriteLine(buttonEvent.Pressed);
                        if (buttonEvent.ButtonIndex == (int)ButtonList.Left)
                        {
                            MouseDragging = buttonEvent.Pressed;
                            DragOffset = GetGlobalRect().Position - buttonEvent.GlobalPosition;

                            if (!MouseDragging)
                            {
                                DockingManager.TryDock(GetParent<ToolWindow>(), mouseEvent.GlobalPosition);
                            }
                        }
                    }
                }
                else if (mouseEvent is InputEventMouseMotion motionEvent)
                {
                    if (MouseDragging)
                    {
                        if (GetParent<ToolWindow>().IsDocked)
                        {
                            DockingManager.Undock(GetParent<ToolWindow>());
                        }
                        
                        ToolWindow parent = GetParent<ToolWindow>();
                        parent.SetGlobalPosition(motionEvent.GlobalPosition - RectPosition + DragOffset);
                        DockingManager.SetupGuides(GetParent<ToolWindow>());
                    }
                }
            }

            base._Input(inputEvent);
        }
    }
}
