using Godot;

namespace DndAwesome.scripts
{
    public class Map : TextureRect
    {
        public Vector2 Size { get; set; } = new Vector2(750, 750);
        public override void _Ready()
        {
            Camera2D camera = SceneObjectManager.GetCamera();
            SetPosition(camera.Position + camera.GetViewport().Size / 2 - Size / 2);
            SetSize(Size);
            base._Ready();
        }

        public override void _Draw()
        {
           VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
        }
    }
}