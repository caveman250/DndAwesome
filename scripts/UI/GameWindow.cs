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

        //only applies to children of GameWindow.
        public Vector2 GetGameViewPosFromScreenPos(Vector2 globalPos)
        {
            return globalPos - SceneObjectManager.GetGameWindow().RectGlobalPosition * SceneObjectManager.GetCamera().Zoom;
        }
        
        public Vector2 GetScreenPosFromGameViewPos(Vector2 screenPos)
        {
            return screenPos + SceneObjectManager.GetGameWindow().RectGlobalPosition * SceneObjectManager.GetCamera().Zoom;
        }

        public bool IsMousePointInBounds(Vector2 mousePos, Control control)
        {
            Camera2D camera = SceneObjectManager.GetCamera();
            Vector2 controlScreenPos = GetScreenPosFromGameViewPos(control.RectGlobalPosition);
            Vector2 mousePosGame = camera.WorldPosToScreenPos(mousePos);
            
            return mousePosGame.x > controlScreenPos.x &&
                   mousePosGame.y > controlScreenPos.y &&
                   mousePosGame.x < controlScreenPos.x + control.GetRect().Size.x &&
                   mousePosGame.y < controlScreenPos.y + control.GetRect().Size.y;
        }

        private void OnWindowResized()
        {
            SceneObjectManager.GetOffscreenViewport().Size = RectSize;
        }
    }
}