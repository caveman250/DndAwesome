using Godot;
using System;

public class Map : Node2D
{
    public Vector2 m_GridSize;
    public Vector2 m_TileSize;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        m_TileSize = new Vector2(40, 40);
        m_GridSize = new Vector2(GetViewportRect().Size.x / m_TileSize.x, GetViewportRect().Size.y / m_TileSize.y);
        
    }
    
    public override void _Draw()
    {
        for (int x = 0; x < m_GridSize.x; ++x)
        {
            for (int y = 0; y < m_GridSize.y; ++y)
            {
                DrawRect(new Rect2(new Vector2(GetViewportRect().Position.x + (x * m_TileSize.x), GetViewportRect().Position.y + (y * m_TileSize.y)), 
                    new Vector2(m_TileSize.x, m_TileSize.y)), 
                    Color.Color8(50, 50, 50), 
                    false);
            }
        }
        
        base._Draw();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
