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
            Grid grid = GetNode<Grid>("/root/Root/Scene/BackgroundLayer/Background/Grid");
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
                    if (translatedMousePos.x > worldPos.x &&
                        translatedMousePos.y > worldPos.y &&
                        translatedMousePos.x < worldPos.x + GetRect().Size.x &&
                        translatedMousePos.y < worldPos.y + GetRect().Size.y)
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
                Grid grid = GetNode<Grid>("/root/Root/Scene/BackgroundLayer/Background/Grid");
                Vector2 gridTileSize = grid.GetTileSize();

                Vector2 gridPos = grid.GlobalPosition;
                Vector2 relativePosition = GlobalPosition - gridPos;
                Vector2 halfGridTile = gridTileSize / 2;

                //get the potential snap positions
                double snapNegativeX = gridPos.x +
                                       (Math.Round((relativePosition.x - halfGridTile.x) / gridTileSize.x) *
                                        gridTileSize.x +
                                        gridTileSize.x / 2);
                double snapNegativeY = gridPos.y +
                                       (Math.Round((relativePosition.y - halfGridTile.y) / gridTileSize.y) *
                                        gridTileSize.y +
                                        gridTileSize.y / 2);
                double snapPositiveX = gridPos.x -
                                       (Math.Round(relativePosition.x / gridTileSize.x) * gridTileSize.x +
                                        halfGridTile.x);
                double snapPositiveY = gridPos.y -
                                       (Math.Round(relativePosition.y / gridTileSize.y) * gridTileSize.y +
                                        halfGridTile.x);

                float newX, newY;
                if (Math.Abs(relativePosition.x - snapNegativeX) < Math.Abs(relativePosition.x - snapPositiveX))
                {
                    newX = (float)snapNegativeX;
                }
                else
                {
                    newX = (float)snapPositiveX;
                }

                if (Math.Abs(relativePosition.y - snapNegativeY) < Math.Abs(relativePosition.y - snapPositiveY))
                {
                    newY = (float)snapNegativeY;
                }
                else
                {
                    newY = (float)snapPositiveY;
                }

                GlobalPosition = new Vector2(newX, newY);

                m_ShouldSnapToGrid = false;
            }
        }
    }
}