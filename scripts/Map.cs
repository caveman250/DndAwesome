using Godot;

namespace DndAwesome.scripts
{
    public class Map : Sprite
    {
        public override void _Draw()
        {
            VisualServer.CanvasItemSetClip(GetCanvasItem(), true);
        }
    }
}
