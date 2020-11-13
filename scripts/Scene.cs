using Godot;
using System;
using System.Collections.Generic;

namespace DndAwesome.scripts
{
    public class Scene : Node
    {
        private Node m_BackgroundLayer;
        private Node m_TokenLayer;
        
        public List<string> BackgroundImages = new List<string>();
        public List<string> TokenLayerTokens = new List<string>();
        public List<string> DMLayerTokens = new List<string>();

        public override void _Ready()
        {
            LoadScene();
            base._Ready();
        }

        public void LoadScene()
        {
            m_BackgroundLayer = GetNode("BackgroundLayer");
            m_TokenLayer = GetNode("TokenLayer");
            
            foreach (string imagePath in BackgroundImages)
            {
                TextureRect textureRect = new TextureRect();
                Texture image = GD.Load<Texture>(imagePath);
                textureRect.Texture = image;
                textureRect.RectSize = new Vector2(500, 500);
                
                m_BackgroundLayer.AddChild(textureRect);
            }
            
            foreach (string tokenPath in TokenLayerTokens)
            {
                PackedScene tokenScene = GD.Load<PackedScene>(tokenPath);
                Token token = tokenScene.Instance() as Token;
                
                m_TokenLayer.AddChild(token);
            }
        }
    }
}
