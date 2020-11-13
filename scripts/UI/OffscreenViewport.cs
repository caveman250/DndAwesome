using Godot;

namespace DndAwesome.Layouts
{
    public class OffscreenViewport : Viewport
    {
        public override void _Ready()
        {
            scripts.SceneObjectManager.SetOffscreenViewport(this);
        }
    }
}
