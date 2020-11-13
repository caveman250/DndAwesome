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
            Grid grid = SceneObjectManager.GetGrid();
            Vector2 gridTileSize = grid.GetTileSize();
            Scale = new Vector2(gridTileSize.x / GetRect().Size.x, gridTileSize.y / GetRect().Size.y);
            
            SceneObjectManager.RegisterToken(this);
        }

        public override void _Input(InputEvent input)
        {
            if (input is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.Pressed)
                {
                    Camera2D camera = SceneObjectManager.GetCamera();
                    Vector2 worldPos = GetGlobalTransform().Xform(GetRect().Position);
                    Vector2 translatedMousePos = camera.WorldPosToScreenPos(mouseButtonEvent.Position);
                    Vector2 realMousePos = translatedMousePos - SceneObjectManager.GetGameWindow().RectGlobalPosition * camera.Zoom;
                    if (realMousePos.x > worldPos.x &&
                        realMousePos.y > worldPos.y &&    
                        realMousePos.x < worldPos.x + GetRect().Size.x &&
                        realMousePos.y < worldPos.y + GetRect().Size.y)
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
            Camera2D camera = SceneObjectManager.GetCamera();
            if (m_FollowingMouse)
            {
                Position = camera.WorldPosToScreenPos(GetViewport().GetMousePosition()) - SceneObjectManager.GetGameWindow().RectGlobalPosition * camera.Zoom;
            }
            else if (m_ShouldSnapToGrid)
            {
                Grid grid = SceneObjectManager.GetGrid();
                Vector2 gridTileSize = grid.GetTileSize();
                
                Vector2 gridPosWindow = grid.GlobalPosition - SceneObjectManager.GetGameWindow().RectGlobalPosition * camera.Zoom;
                Vector2 tokenPosWindow = GlobalPosition - SceneObjectManager.GetGameWindow().RectGlobalPosition * camera.Zoom;
                
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

                Position = new Vector2(newX, newY) + SceneObjectManager.GetGameWindow().RectGlobalPosition * camera.Zoom;

                m_ShouldSnapToGrid = false;
            }
        }
    }
}