using Godot;
using System;
using System.Collections.Generic;

namespace DndAwesome.scripts
{
    public class Scene : Node
    {
        public Node BackgroundLayer { get; set; }
        public Node TokenLayer { get; set; }
        public Node DmLayer { get; set; }
        
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
            BackgroundLayer = GetNode("BackgroundLayer");
            TokenLayer = GetNode("TokenLayer");
            DmLayer = GetNode("DMLayer");

            Node insertBelow = BackgroundLayer.GetNode("ChildEntryPoint");
            foreach (string imagePath in BackgroundImages)
            {
                PackedScene tokenScene = GD.Load<PackedScene>("res://Prefabs/BackgroundImage.tscn");
                BackgroundImage backgroundImage = tokenScene.Instance() as BackgroundImage;
                Texture image = GD.Load<Texture>(imagePath);
                backgroundImage.Texture = image;
                backgroundImage.RectSize = new Vector2(500, 500);
                BackgroundLayer.AddChildBelowNode(insertBelow, backgroundImage);
            }
            
            foreach (string tokenPath in TokenLayerTokens)
            {
                PackedScene tokenScene = GD.Load<PackedScene>(tokenPath);
                Token token = tokenScene.Instance() as Token;
                
                TokenLayer.AddChild(token);
            }
            
            foreach (string tokenPath in DMLayerTokens)
            {
                PackedScene tokenScene = GD.Load<PackedScene>(tokenPath);
                Token token = tokenScene.Instance() as Token;
                
                DmLayer.AddChild(token);
            }
        }
    }
}
