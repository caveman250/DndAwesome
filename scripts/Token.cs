using System;
using DndAwesome.scripts.UI;
using Godot;

namespace DndAwesome.scripts
{
    public class Token : TextureRect
    {
        private bool m_FollowingMouse;
        private bool m_ShouldSnapToGrid;

        public override void _Ready()
        {
            Grid grid = SceneObjectManager.GetGrid();
            SetSize(grid.GetTileSize());
            SceneObjectManager.RegisterToken(this);
        }

        public bool Input(InputEvent input)
        {
            GameWindow window = SceneObjectManager.GetGameWindow();
            
            if (input is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.Pressed)
                {
                    if (window.IsMousePointInBounds(mouseButtonEvent.Position, this))
                    {
                        m_FollowingMouse = !m_FollowingMouse;
                        if (!m_FollowingMouse)
                        {
                            m_ShouldSnapToGrid = true;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void _Process(float delta)
        {
            Camera2D camera = SceneObjectManager.GetCamera();
            if (m_FollowingMouse)
            {
                SetPosition(camera.ScreenPosToWorldPos(GetViewport().GetMousePosition()) - RectSize / 2);
            }
            else if (m_ShouldSnapToGrid)
            {
                SetPosition(Grid.SnapPointToGrid(RectGlobalPosition));

                m_ShouldSnapToGrid = false;
            }
        }
    }
}