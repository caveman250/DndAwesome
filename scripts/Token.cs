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

        public override void _Input(InputEvent input)
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
                        }
                    }
                }
            }
        }

        public override void _Process(float delta)
        {
            GameWindow window = SceneObjectManager.GetGameWindow();
            Camera2D camera = SceneObjectManager.GetCamera();
            if (m_FollowingMouse)
            {
                SetPosition(window.GetGameViewPosFromScreenPos(camera.WorldPosToScreenPos(GetViewport().GetMousePosition())) - RectSize / 2);
            }
            else if (m_ShouldSnapToGrid)
            {
                Grid grid = SceneObjectManager.GetGrid();
                Vector2 gridTileSize = grid.GetTileSize();

                Vector2 gridPosWindow = window.GetGameViewPosFromScreenPos(grid.GlobalPosition);
                Vector2 tokenPosWindow = window.GetGameViewPosFromScreenPos(RectGlobalPosition) + RectSize / 2;
                
                Vector2 relativePosition = tokenPosWindow - gridPosWindow;
                Vector2 halfGridTile = gridTileSize / 2;

                //get the potential snap positions
                double snapNegativeX = gridPosWindow.x +
                                       (Math.Round((relativePosition.x - halfGridTile.x) / gridTileSize.x) *
                                        gridTileSize.x +
                                        gridTileSize.x / 2);
                double snapNegativeY = gridPosWindow.y +
                                       (Math.Round((relativePosition.y - halfGridTile.y) / gridTileSize.y) *
                                        gridTileSize.y +
                                        gridTileSize.y / 2);
                double snapPositiveX = gridPosWindow.x +
                                       (Math.Round(relativePosition.x / gridTileSize.x) * gridTileSize.x +
                                        halfGridTile.x);
                double snapPositiveY = gridPosWindow.y +
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

                SetPosition(window.GetScreenPosFromGameViewPos(new Vector2(newX, newY)) - RectSize / 2);

                m_ShouldSnapToGrid = false;
            }
        }
    }
}