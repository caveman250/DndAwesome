using System.Collections.Generic;
using Godot;

namespace DndAwesome.scripts
{
    public static class SceneObjectManager
    {
        private static Camera2D s_Camera;
        private static Viewport s_OffscreenViewport;
        private static Grid s_Grid;
        private static GameWindow s_GameWindow;
        private static List<Token> s_Tokens = new List<Token>();

        public static void RegisterToken(Token token)
        {
            s_Tokens.Add(token);
        }

        public static void DeRegisterToken(Token token)
        {
            s_Tokens.Remove(token);
        }

        public static List<Token> GetTokens()
        {
            return s_Tokens;
        }

        public static void SetCamera(Camera2D camera)
        {
            s_Camera = camera;
        }
        public static Camera2D GetCamera()
        {
            return s_Camera;
        }

        public static void SetOffscreenViewport(Viewport viewport)
        {
            s_OffscreenViewport = viewport;
        }
        public static Viewport GetOffscreenViewport()
        {
            return s_OffscreenViewport;
        }

        public static void SetGrid(Grid grid)
        {
            s_Grid = grid;
        }

        public static Grid GetGrid()
        {
            return s_Grid;
        }
        
        public static void SetGameWindow(GameWindow window)
        {
            s_GameWindow = window;
        }
        public static GameWindow GetGameWindow()
        {
            return s_GameWindow;
        }
        
        
    }
}