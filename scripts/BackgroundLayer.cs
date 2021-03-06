using Godot;

namespace DndAwesome.scripts
{
    public class BackgroundLayer : Node
    {
        public override void _Ready()
        {
            Map map = GetNode<Map>("Background");
            SceneObjectManager.GetGrid().SetGridSize(map.Size);
            GetNode<Control>("GridContainer").SetSize(map.Size);
            GetNode<Control>("GridContainer").SetPosition(map.RectPosition);
        }
    }
}
