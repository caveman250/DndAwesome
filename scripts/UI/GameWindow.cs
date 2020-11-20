using Godot;
using System;
using System.Linq;
using Camera2D = Godot.Camera2D;

namespace DndAwesome.scripts.UI
{
    public class GameWindow : TextureRect
    {
        public override void _Ready()
        {
            SetProcessInput(true);
            Connect("resized", this, "OnWindowResized");
            SceneObjectManager.SetGameWindow(this);
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (!GetParent<ToolWindow.ToolWindow>().Focused)
            {
                return;
            }

            SceneObjectManager.GetCamera()._Input(@event);

            Scene scene = SceneObjectManager.GetCamera().GetNode<Scene>("Scene");
            if (scene != null)
            {
                foreach (Node node in scene.BackgroundLayer.GetChildren())
                {
                    if (node is BackgroundImage image)
                    {
                        if (image.Input(@event))
                        {
                            return;
                        }
                    }
                    else
                    {
                        node._Input(@event);
                    }
                }
                
                foreach (Token token in scene.TokenLayer.GetChildren().Cast<Token>())
                {
                    if (token.Input(@event))
                    {
                        return;
                    }
                }
                
                foreach (Token token in scene.DmLayer.GetChildren().Cast<Token>())
                {
                    if (token.Input(@event))
                    {
                        return;
                    }
                }
            }
        }

        public bool IsMousePointInBounds(Vector2 mousePos, Control control)    
        {
            Camera2D camera = SceneObjectManager.GetCamera();
            Vector2 mousePosGame = camera.ScreenPosToWorldPos(mousePos);

            return mousePosGame.x > control.RectGlobalPosition.x &&
                   mousePosGame.y > control.RectGlobalPosition.y &&
                   mousePosGame.x < control.RectGlobalPosition.x + control.GetRect().Size.x &&
                   mousePosGame.y < control.RectGlobalPosition.y + control.GetRect().Size.y;
        }

        private void OnWindowResized()
        {
            SceneObjectManager.GetOffscreenViewport().Size = RectSize;
        }
    }
}