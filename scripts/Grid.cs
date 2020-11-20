using System;
using DndAwesome.scripts.UI;
using Godot;

namespace DndAwesome.scripts
{
    public class Grid : Node2D
    {
        private Vector2 m_GridSize = new Vector2(-1, -1);
        public Vector2 m_TileSize = new Vector2(40, 40);
        
        public override void _Ready()
        {
            SceneObjectManager.SetGrid(this);
        }

        public void SetGridSize(Vector2 size)
        {
            m_GridSize = size / m_TileSize;
        }

        public override void _Draw()
        {
            if (m_GridSize.x > 0 && m_GridSize.y > 0)
            {
                Map background = GetParent().GetParent().GetNode<Map>("Background");
                
                for (int x = 0; x < m_GridSize.x; ++x)
                {
                    for (int y = 0; y < m_GridSize.y; ++y)
                    {
                        Vector2 localPos = background.RectGlobalPosition +
                                           new Vector2(x * m_TileSize.x, y * m_TileSize.y);
                        
                        DrawRect(new Rect2(GetGlobalTransform().XformInv(localPos),
                                           m_TileSize),
                                 Color.Color8(50, 50, 50),
                                 false);
                    }
                }
            }

            base._Draw();
        }

        public Vector2 GetTileSize()
        {
            return m_TileSize;
        }
        
        public static Vector2 SnapPointToGrid(Vector2 point)
        {
            Camera2D camera = SceneObjectManager.GetCamera();
            Grid grid = SceneObjectManager.GetGrid();
            Vector2 gridTileSize = grid.GetTileSize();
            Vector2 halfGridTile = gridTileSize / 2;
            Vector2 gridPosWindow = grid.GlobalPosition;
            Vector2 tokenPosWindow = point + halfGridTile;
            
            Vector2 relativePosition = tokenPosWindow - gridPosWindow;
          

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
            Vector2 offset = new Vector2(0, 0);
            if (Math.Abs(tokenPosWindow.x - snapNegativeX) < Math.Abs(tokenPosWindow.x - snapPositiveX))
            {
                newX = (float)snapNegativeX;
            }
            else
            {
                newX = (float)snapPositiveX;
            }

            if (Math.Abs(tokenPosWindow.y - snapNegativeY) < Math.Abs(tokenPosWindow.y - snapPositiveY))
            {
                newY = (float)snapNegativeY;
            }
            else
            {
                newY = (float)snapPositiveY;
            }

            return new Vector2(newX, newY) - halfGridTile;
        }
    }
}