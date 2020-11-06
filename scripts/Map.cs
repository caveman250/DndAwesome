using Godot;

namespace DndAwesome.scripts
{
    public class Map : TextureRect
    {

        public Vector2 m_Size = new Vector2(750, 750);
        public override void _Ready()
        {
            Camera2D camera = GetNode<Camera2D>("/root/Root/Camera2D");
            SetPosition(camera.Position + camera.GetViewport().Size / 2 - m_Size / 2);
            SetSize(m_Size);
            GetNode<Grid>("Grid").SetGridSize(m_Size);
            base._Ready();
        }

        public override void _Draw()
        {
           VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
        }
    }
}