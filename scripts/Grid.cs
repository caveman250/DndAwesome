using System;
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
                Map background = GetParent().GetNode<Map>("Background");
                
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
    }
}