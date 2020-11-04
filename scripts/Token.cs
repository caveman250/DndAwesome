using System;
using Godot;

namespace DndAwesome.scripts
{
	public class Token : Sprite
	{
		private bool m_FollowingMouse = false;
		private bool m_ShouldSnapToGrid = false;
	
		public override void _Ready()
		{
			Map map = GetNode<Map>("/root/Root/Grid");
			Vector2 gridTileSize = map.m_TileSize; 
			Scale = new Vector2(gridTileSize.x / GetRect().Size.x, gridTileSize.y / GetRect().Size.y);
		}
	
		public override void _Input(InputEvent input)
		{
			InputEventMouseButton mouseButtonEvent = input as InputEventMouseButton;
			if(mouseButtonEvent != null)
			{
				if (mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.Pressed == true)
				{
					Vector2 worldPos = GetGlobalTransform().Xform(GetRect().Position);
					if (mouseButtonEvent.Position.x > worldPos.x &&
					    mouseButtonEvent.Position.y > worldPos.y &&
					    mouseButtonEvent.Position.x < worldPos.x + GetRect().Size.x &&
					    mouseButtonEvent.Position.y < worldPos.y + GetRect().Size.y)
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

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			if (m_FollowingMouse)
			{
				Position = GetViewport().GetMousePosition();
			}
			else if (m_ShouldSnapToGrid)
			{
				Map map = GetNode<Map>("/root/Root/Grid");
				Vector2 gridTileSize = map.m_TileSize;

				//get the potential snap positions
				float snapNegativeX =
					(float) (Math.Round((Position.x - (gridTileSize.x / 2)) / gridTileSize.x) * gridTileSize.x) +
					(gridTileSize.x / 2);
				float snapNegativeY =
					(float) (Math.Round((Position.y - (gridTileSize.y / 2)) / gridTileSize.y) * gridTileSize.y) +
					+(gridTileSize.y / 2);
				float snapPositiveX = (float) (Math.Round(Position.x / gridTileSize.x) * gridTileSize.x) +
				                      (gridTileSize.x / 2);
				float snapPositiveY = (float) (Math.Round(Position.y / gridTileSize.y) * gridTileSize.y) +
				                      (gridTileSize.y / 2);

				float newX, newY = 0;
				if (Math.Abs(Position.x - snapNegativeX) < Math.Abs(Position.x - snapPositiveX))
				{
					newX = snapNegativeX;
				}
				else
				{
					newX = snapPositiveX;
				}
			
				if (Math.Abs(Position.y - snapNegativeY) < Math.Abs(Position.y - snapPositiveY))
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
