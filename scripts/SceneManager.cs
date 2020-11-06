using Godot;
using System;
using System.Collections.Generic;

namespace DndAwesome.scripts
{
    public class SceneManager : Node
    {
        public override void _Ready()
        {
            // Image image = new Image();
            // image.Load("/home/jason/Pictures/2020-11-05_15-13_1.png");
            //
            // image.SavePng("user://test.png");

            var scenePrefab = GD.Load<PackedScene>("res://Scenes/Scene.tscn");
            Scene scene = scenePrefab.Instance() as Scene;
            scene.BackgroundImages.Add("res://images/ham.png");
            scene.TokenLayerTokens.Add("res://Scenes/PlayerToken.tscn");
         
            GetNode<Node2D>("/root/Root").CallDeferred("add_child", scene);
        }
    }
}
