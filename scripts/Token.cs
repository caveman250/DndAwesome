using System;
using Godot;

namespace DndAwesome.scripts
{
    public class Token : Sprite
    {
        private bool m_FollowingMouse;
        private bool m_ShouldSnapToGrid;

        public override void _Ready()
        {
            Grid grid = GetNode<Grid>("/root/Root/Map/Grid");
            Vector2 gridTileSize = grid.GetTileSize();
            Scale = new Vector2(gridTileSize.x / GetRect().Size.x, gridTileSize.y / GetRect().Size.y);
        }

        public override void _Input(InputEvent input)
        {
            if (input is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.Pressed)
                {
                    Camera2D camera = GetNode<Camera2D>("/root/Root/Camera2D");
                    Vector2 worldPos = GetGlobalTransform().Xform(GetRect().Position);
                    Vector2 translatedMousePos = camera.WorldPosToScreenPos(mouseButtonEvent.Position);
                    if (translatedMousePos.x > worldPos.x && translatedMousePos.y > worldPos.y && translatedMousePos.x < worldPos.x + GetRect().Size.x && translatedMousePos.y < worldPos.y + GetRect().Size.y)
                    {
                        m_FollowingMouse = !m_FollowingMouse;
                        if (!m_FollowingMouse)
                        {
                            m_ShouldSnapToGrid = true;
                        }
                    }
                }
            }
        }
        
        public override void _Process(float delta)
        {
            Camera2D camera = GetNode<Camera2D>("/root/Root/Camera2D");
            if (m_FollowingMouse)
            {
                Position = camera.WorldPosToScreenPos(GetViewport().GetMousePosition());
            }
            else if (m_ShouldSnapToGrid)
            {
                Grid grid = GetNode<Grid>("/root/Root/Map/Grid");
                Vector2 gridTileSize = grid.GetTileSize();

                Vector2 realPos = Position;
                Vector2 gridPos = ((Map)grid.GetParent()).GetGlobalTransform().Xform(grid.Position);

                //get the potential snap positions
                float snapNegativeX = gridPos.x + ((float)(Math.Round((realPos.x - (gridTileSize.x / 2)) / gridTileSize.x) * gridTileSize.x) + (gridTileSize.x / 2));
                float snapNegativeY = gridPos.y + ((float)(Math.Round((realPos.y - (gridTileSize.y / 2)) / gridTileSize.y) * gridTileSize.y) + (gridTileSize.y / 2));
                float snapPositiveX = gridPos.x + ((float)(Math.Round(realPos.x / gridTileSize.x) * gridTileSize.x) + (gridTileSize.x / 2));
                float snapPositiveY = gridPos.y + ((float)(Math.Round(realPos.y / gridTileSize.y) * gridTileSize.y) + (gridTileSize.y / 2));

                float newX, newY;
                if (Math.Abs(realPos.x - snapNegativeX) < Math.Abs(realPos.x - snapPositiveX))
                {
                    newX = snapNegativeX;
                }
                else
                {
                    newX = snapPositiveX;
                }

                if (Math.Abs(realPos.y - snapNegativeY) < Math.Abs(realPos.y - snapPositiveY))
                {
                    newY = snapNegativeY;
                }
                else
                {
                    newY = snapPositiveY;
                }

                Position = new Vector2(newX, newY);

                m_ShouldSnapToGrid = false;
            }
        }
    }
}