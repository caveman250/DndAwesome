using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
using DndAwesome.scripts.UI.ToolWindow;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

namespace DndAwesome.scripts
{
    public class DockingManager : Node2D
    {
        private static readonly List<ToolWindow> s_ToolWindows = new List<ToolWindow>();
        private static ReferenceRect s_DockPlaceholder;
        private static readonly List<DockGuide> s_Guides = new List<DockGuide>();
        private static DockingManager s_Instance;
        private static Vector2 s_CachedWindowAreaSize;
        private static Control s_DockRoot = null;
        private static bool s_ShouldSave;
        private static int s_LastSaveTime = 0;
        private static int s_NumSecondsBetweenSaves = 5;

        public override void _Ready()
        {
            s_Instance = this;
            GetTree().Root.Connect("size_changed", this, "OnWindowResized");

            base._Ready();
        }

        public override void _Process(float delta)
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            if (t.TotalSeconds > s_LastSaveTime + s_NumSecondsBetweenSaves)
            {
                if (s_ShouldSave && s_DockRoot != null)
                {
                    SaveDockLayoutInternal();
                }

                s_LastSaveTime = (int)t.TotalSeconds;
            }

            base._Process(delta);
        }

        public static void SetCurrentWindowAreaSize(Vector2 size)
        {
            s_CachedWindowAreaSize = size;
        }

        public override void _Draw()
        {
            foreach (DockGuide guide in s_Guides)
            {
                DrawRect(guide.GuideRect, Color.Color8(100, 100, 100, 100));
            }

            base._Draw();
        }

        private void OnWindowResized()
        {
            //make sure we dont double resize containers.
            List<SplitContainer> processedContainers = new List<SplitContainer>();
            Vector2 sizeFactor = GetNode<Control>("../Windows").RectSize / s_CachedWindowAreaSize;

            foreach (ToolWindow toolWindow in s_ToolWindows)
            {
                if (toolWindow.GetParent() is SplitContainer container)
                {
                    if (!processedContainers.Contains(container))
                    {
                        container.SetSize(container.GetRect().Size * sizeFactor);
                        container.SetPosition(container.GetRect().Position * sizeFactor);
                        if (container is HSplitContainer)
                        {
                            container.SplitOffset = (int)(container.SplitOffset * sizeFactor.x);
                        }
                        else
                        {
                            container.SplitOffset = (int)(container.SplitOffset * sizeFactor.y);
                        }

                        processedContainers.Add(container);
                    }
                }
                else
                {
                    toolWindow.SetSize(toolWindow.GetRect().Size * sizeFactor);
                    toolWindow.SetPosition(toolWindow.GetRect().Position * sizeFactor);
                }
            }

            if (s_DockPlaceholder != null)
            {
                s_DockPlaceholder.SetSize(s_DockPlaceholder.GetRect().Size * sizeFactor);
                s_DockPlaceholder.SetPosition(s_DockPlaceholder.GetRect().Position * sizeFactor);
            }

            s_CachedWindowAreaSize = GetNode<Control>("../Windows").RectSize;
        }

        public static void RegisterToolWindow(ToolWindow window)
        {
            s_ToolWindows.Add(window);
        }

        private static void DoDockToScreenEdge(ToolWindow window, DockAction action)
        {
            Control dockArea = s_Instance.GetNode<Control>("../Windows/DockArea");
            Rect2 dockAreaRect = dockArea.GetGlobalRect();

            SplitContainer newContainer = null;
            int splitOffset;
            bool flipChildOrder = false;
            switch (action)
            {
                case DockAction.DockToScreenLeft:
                    splitOffset = 400;
                    newContainer = new HSplitContainer();
                    break;
                case DockAction.DockToScreenRight:
                    splitOffset = (int)(s_Instance.GetViewportRect().Size.x - 400);
                    flipChildOrder = true;
                    newContainer = new HSplitContainer();
                    break;
                case DockAction.DockToScreenTop:
                    splitOffset = 200;
                    newContainer = new VSplitContainer();
                    break;
                case DockAction.DockToScreenBottom:
                    splitOffset = (int)(s_Instance.GetViewportRect().Size.y - 200);
                    flipChildOrder = true;
                    newContainer = new VSplitContainer();
                    break;
                default:
                    Console.WriteLine("Error - DockingManager::DoDockToScreenEdge Invalid DockAction");
                    return;
            }

            newContainer.SetPosition(new Vector2(0, 0));
            newContainer.SetSize(dockAreaRect.Size);
            dockArea.AddChild(newContainer);
            window.GetParent().RemoveChild(window);
            PackedScene placeholderRect = GD.Load<PackedScene>("res://Layouts/DockPlaceholder.tscn");
            s_DockPlaceholder = placeholderRect.Instance() as ReferenceRect;

            if (flipChildOrder)
            {
                newContainer.AddChild(s_DockPlaceholder);
                newContainer.AddChild(window);
            }
            else
            {
                newContainer.AddChild(window);
                newContainer.AddChild(s_DockPlaceholder);
            }

            newContainer.SplitOffset = splitOffset;
            s_DockRoot = newContainer;
            
            SaveDockLayout();
        }

        private static void DoDockToOtherWindow(Node window, Node otherWindow, DockAction action)
        {
            Rect2 otherWindowRect = ((Control)otherWindow).GetGlobalRect();

            SplitContainer newContainer = null;
            int splitOffset = 0;
            bool flipChildOrder = false;
            switch (action)
            {
                case DockAction.DockToWindowLeft:
                case DockAction.DockToWindowRight:
                    newContainer = new HSplitContainer();
                    splitOffset = (int)(otherWindowRect.Size.x / 2);
                    flipChildOrder = action == DockAction.DockToWindowRight;
                    break;
                case DockAction.DockToWindowTop:
                case DockAction.DockToWindowBottom:
                    newContainer = new VSplitContainer();
                    splitOffset = (int)(otherWindowRect.Size.y / 2);
                    flipChildOrder = action == DockAction.DockToWindowBottom;
                    break;
            }

            if (otherWindow == s_DockRoot)
            {
                s_DockRoot = newContainer;
            }

            otherWindow.GetParent().AddChildBelowNode(otherWindow, newContainer);
            otherWindow.GetParent().RemoveChild(otherWindow);
            window.GetParent().RemoveChild(window);

            newContainer.SetGlobalPosition(otherWindowRect.Position);
            newContainer.SetSize(otherWindowRect.Size);
            
            if (flipChildOrder)
            {
                newContainer.AddChild(otherWindow);
                newContainer.AddChild(window);
            }
            else
            {
                newContainer.AddChild(window);
                newContainer.AddChild(otherWindow);
            }

            newContainer.SplitOffset = splitOffset;

            if (otherWindow is ToolWindow otherToolWindow)
            {
                otherToolWindow.IsDocked = true;
            }

            if (window is ToolWindow toolWindow)
            {
                toolWindow.IsDocked = true;
            }

            SaveDockLayout();
        }

        private static void PerformDockingAction(ToolWindow window, DockGuide guide)
        {
            switch (guide.DockAction)
            {
                case DockAction.DockToScreenCenter:
                {
                    Control dockArea = s_Instance.GetNode<Control>("../Windows/DockArea");
                    window.GetParent().RemoveChild(window);
                    window.SetSize(dockArea.RectSize);
                    dockArea.AddChild(window);
                    window.SetPosition(new Vector2(0, 0));
                    s_DockRoot = window;
                    break;
                }
                case DockAction.DockToScreenLeft:
                case DockAction.DockToScreenRight:
                case DockAction.DockToScreenTop:
                case DockAction.DockToScreenBottom:
                {
                    DoDockToScreenEdge(window, guide.DockAction);
                    break;
                }
                case DockAction.DockToWindowLeft:
                case DockAction.DockToWindowRight:
                case DockAction.DockToWindowTop:
                case DockAction.DockToWindowBottom:
                {
                    DoDockToOtherWindow(window, guide.m_OwningWindow, guide.DockAction);
                    break;
                }
                case DockAction.ReplaceWindow:
                {
                    Node parent = guide.m_OwningWindow.GetParent();
                    window.GetParent().RemoveChild(window);
                    parent.AddChildBelowNode(guide.m_OwningWindow, window);
                    parent.RemoveChild(guide.m_OwningWindow);
                    guide.m_OwningWindow.QueueFree();
                    s_DockPlaceholder = null;
                    break;
                }
            }

            window.IsDocked = true;
        }

        public static void TryDock(ToolWindow window, Vector2 mouseGlobalPos)
        {
            foreach (DockGuide guide in s_Guides)
            {
                if (guide.GuideRect.HasPoint(mouseGlobalPos))
                {
                    PerformDockingAction(window, guide);
                }
            }

            s_Guides.Clear();
            s_Instance.Update();
        }

        public static void Undock(ToolWindow window)
        {
            if (window.GetParent() is SplitContainer)
            {
                SplitContainer parent = window.GetParent<SplitContainer>();
                Node floatArea = s_Instance.GetNode<Control>("../Windows/FloatingArea");
                
                if (parent == s_DockRoot && s_DockPlaceholder == null)
                {
                    bool shouldAddNewPlaceholder = true;
                    //only add a placeholder if there are no child split containers. otherwise they take over as the new root.
                    foreach (Node child in parent.GetChildren())
                    {
                        if (child is SplitContainer)
                        {
                            shouldAddNewPlaceholder = false;
                            break;
                        }
                    }

                    if (shouldAddNewPlaceholder)
                    {
                        PackedScene placeholderRect = GD.Load<PackedScene>("res://Layouts/DockPlaceholder.tscn");
                        s_DockPlaceholder = placeholderRect.Instance() as ReferenceRect;

                        parent.AddChildBelowNode(window, s_DockPlaceholder);
                        parent.RemoveChild(window);
                        floatArea.AddChild(window);
                        return;
                    }
                }
                
                Node containerParent = parent.GetParent();
               
                for (int i = parent.GetChildren().Count - 1; i > -1; --i)
                {
                    Control child = parent.GetChild<Control>(i);
                    parent.RemoveChild(child);
                    if (child is ToolWindow toolWindow)
                    {
                        if (child == window || (!(containerParent is SplitContainer)))
                        {
                            toolWindow.IsDocked = false;
                            floatArea.AddChild(toolWindow);
                            if (parent is HSplitContainer)
                            {
                                child.SetGlobalPosition(parent.GetGlobalRect().Position +
                                                  new Vector2(parent.SplitOffset * i, 0));
                            }
                            else
                            {
                                child.SetGlobalPosition(parent.GetGlobalRect().Position +
                                                  new Vector2(0, parent.SplitOffset * i));
                            }
                        }
                        else
                        {
                            containerParent.AddChildBelowNode(parent, child);
                        }
                    }
                    else if (child is SplitContainer)
                    {
                        containerParent.AddChildBelowNode(parent, child);
                        child.SetGlobalPosition(parent.RectGlobalPosition);
                        child.SetSize(parent.RectSize);

                        if (parent == s_DockRoot)
                        {
                            s_DockRoot = child;
                        }
                    }
                    else
                    {
                        child.QueueFree();
                        s_DockPlaceholder = null;
                    }
                }
                
                containerParent.RemoveChild(parent);
                if (parent == s_DockRoot)
                {
                    s_DockRoot = null;
                }

                parent.Free();
            }
            else if (s_DockRoot == window)
            {
                s_DockRoot = null;
            }

            SaveDockLayout();
        }

        public enum DockAction
        {
            DockToScreenLeft,
            DockToScreenRight,
            DockToScreenTop,
            DockToScreenBottom,
            DockToScreenCenter,
            DockToWindowLeft,
            DockToWindowRight,
            DockToWindowTop,
            DockToWindowBottom,
            ReplaceWindow,
        }

        public struct DockGuide
        {
            public DockGuide(Rect2 rect, DockAction action, Node owningWindow = null)
            {
                this.GuideRect = rect;
                this.DockAction = action;
                this.m_OwningWindow = owningWindow;
            }

            public Rect2 GuideRect;
            public DockAction DockAction;
            public Node m_OwningWindow;
        }

        public static void SetupGuides(ToolWindow window)
        {
            s_Guides.Clear();

            Control dockArea = s_Instance.GetNode<Control>("../Windows/DockArea");
            Vector2 guideSize = new Vector2(50, 50);
            if (s_DockRoot == null)
            {
                //screen edges
                s_Guides.Add(new DockGuide(new Rect2(dockArea.RectGlobalPosition + new Vector2(0,
                                                                 dockArea.RectSize.y / 2 - guideSize.y / 2),
                                                     guideSize),
                                           DockAction.DockToScreenLeft));

                s_Guides.Add(new DockGuide(new Rect2(dockArea.RectGlobalPosition + new Vector2(dockArea.RectSize.x - guideSize.x,
                                                                                               dockArea.RectSize.y / 2 - guideSize.y / 2),
                                                     guideSize),
                                           DockAction.DockToScreenRight));

                s_Guides.Add(new DockGuide(new Rect2(dockArea.RectGlobalPosition + new Vector2(dockArea.RectSize.x / 2 - guideSize.x / 2,
                                                                                               0),
                                                     guideSize),
                                           DockAction.DockToScreenTop));
                s_Guides.Add(new DockGuide(new Rect2(dockArea.RectGlobalPosition + new Vector2(dockArea.RectSize.x / 2 - guideSize.x / 2,
                                                                                               dockArea.RectSize.y - guideSize.y),
                                                     guideSize),
                                           DockAction.DockToScreenBottom));

                s_Guides.Add(new DockGuide(new Rect2(dockArea.RectGlobalPosition + new Vector2(dockArea.RectSize.x / 2 - guideSize.x / 2,
                                                                                               dockArea.RectSize.y / 2 - guideSize.y / 2),
                                                     guideSize),
                                           DockAction.DockToScreenCenter));
            }

            foreach (ToolWindow otherWindow in s_ToolWindows)
            {
                if (otherWindow != window && otherWindow.IsDocked)
                {
                    //screen edges
                    s_Guides.Add(new DockGuide(new Rect2(new Vector2(otherWindow.RectGlobalPosition.x + otherWindow.RectSize.x / 2 - guideSize.x * 2,
                                                                     otherWindow.RectGlobalPosition.y + otherWindow.RectSize.y / 2 - guideSize.y / 2),
                                                         guideSize),
                                               DockAction.DockToWindowLeft, otherWindow));

                    s_Guides.Add(new DockGuide(new Rect2(new Vector2(otherWindow.RectGlobalPosition.x + otherWindow.RectSize.x / 2 + guideSize.x,
                                                                     otherWindow.RectGlobalPosition.y + otherWindow.RectSize.y / 2 - guideSize.y / 2),
                                                         guideSize),
                                               DockAction.DockToWindowRight, otherWindow));

                    s_Guides.Add(new DockGuide(new Rect2(new Vector2(otherWindow.RectGlobalPosition.x + otherWindow.RectSize.x / 2 - guideSize.x / 2,
                                                                     otherWindow.RectGlobalPosition.y + otherWindow.RectSize.y / 2 - guideSize.y * 2),
                                                         guideSize),
                                               DockAction.DockToWindowTop, otherWindow));
                    
                    s_Guides.Add(new DockGuide(new Rect2(new Vector2(otherWindow.RectGlobalPosition.x + otherWindow.RectSize.x / 2 - guideSize.x / 2,
                                                                     otherWindow.RectGlobalPosition.y + otherWindow.RectSize.y / 2 + guideSize.y),
                                                         guideSize),
                                               DockAction.DockToWindowBottom, otherWindow));
                }
            }

            if (s_DockPlaceholder != null)
            {
                //screen edges
                s_Guides.Add(new DockGuide(new Rect2(new Vector2(s_DockPlaceholder.RectGlobalPosition.x + s_DockPlaceholder.RectSize.x / 2 - guideSize.x * 2,
                                                                 s_DockPlaceholder.RectGlobalPosition.y + s_DockPlaceholder.RectSize.y / 2 - guideSize.y / 2),
                                                     guideSize),
                                           DockAction.DockToWindowLeft, s_DockPlaceholder));

                s_Guides.Add(new DockGuide(new Rect2(new Vector2(s_DockPlaceholder.RectGlobalPosition.x + s_DockPlaceholder.RectSize.x / 2 + guideSize.x,
                                                                 s_DockPlaceholder.RectGlobalPosition.y + s_DockPlaceholder.RectSize.y / 2 - guideSize.y / 2),
                                                     guideSize),
                                           DockAction.DockToWindowRight, s_DockPlaceholder));

                s_Guides.Add(new DockGuide(new Rect2(new Vector2(s_DockPlaceholder.RectGlobalPosition.x + s_DockPlaceholder.RectSize.x / 2 - guideSize.x / 2,
                                                                 s_DockPlaceholder.RectGlobalPosition.y + s_DockPlaceholder.RectSize.y / 2 - guideSize.y * 2),
                                                     guideSize),
                                           DockAction.DockToWindowTop, s_DockPlaceholder));
                    
                s_Guides.Add(new DockGuide(new Rect2(new Vector2(s_DockPlaceholder.RectGlobalPosition.x + s_DockPlaceholder.RectSize.x / 2 - guideSize.x / 2,
                                                                 s_DockPlaceholder.RectGlobalPosition.y + s_DockPlaceholder.RectSize.y / 2 + guideSize.y),
                                                     guideSize),
                                           DockAction.DockToWindowBottom, s_DockPlaceholder));

                s_Guides.Add(new DockGuide(new Rect2(s_DockPlaceholder.RectGlobalPosition +
                                                     s_DockPlaceholder.RectSize / 2 -
                                                     guideSize / 2,
                                                     guideSize),
                                           DockAction.ReplaceWindow,
                                           s_DockPlaceholder));
            }

            s_Instance.Update();
        }

        private enum SerialisedDockItemType
        {
            SplitContainer,
            Window,
            Placeholder
        }

        class SerialisedDockItem
        {
            public SerialisedDockItemType Type { get; set; } 
        }

        class SerialisedDockContainer : SerialisedDockItem
        {
            public SerialisedDockContainer()
            {
                Type = SerialisedDockItemType.SplitContainer;
            }

            public bool Vertical { get; set; }
            public SerialisedDockItem[] Children { get; set; }
            public int SplitOffset { get; set; }
        }
        
        class SerialisedDockWindow : SerialisedDockItem
        {
            public SerialisedDockWindow()
            {
                Type = SerialisedDockItemType.Window;
            }
            public string WindowName { get; set; }
        }
        
        class SerialisedDockPlaceholder : SerialisedDockItem
        {
            public SerialisedDockPlaceholder()
            {
                Type = SerialisedDockItemType.Placeholder;
            }
        }

        private static void SaveDockLayoutInternal()
        {
            SerialisedDockItem rootItem = null;
            if (s_DockRoot != null)
            {
                if (s_DockRoot is ToolWindow window)
                {
                    rootItem = SerialiseDockWindow(window);
                }
                else if (s_DockRoot is SplitContainer container)
                {
                    rootItem = SerialiseSplitContainer(container);
                }
            }

            string jsonValue = JsonConvert.SerializeObject(rootItem);
            if (jsonValue == "null")
            {
                int lol = 1;
            }

            File.WriteAllText("dockstate.sav",jsonValue);
            s_ShouldSave = false;
        }

        
        public static void SaveDockLayout()
        {
            s_ShouldSave = true;
        }

        public static void LoadDockState()
        {
            if (File.Exists("dockstate.sav"))
            {
                string savedDockState = File.ReadAllText("dockstate.sav");
                JObject currNode = JsonConvert.DeserializeObject<JObject>(savedDockState);
                SerialisedDockItem rootItem = DeserialiseDockItem(currNode);
                RestoreDockItemSavedState(rootItem, HUDPlayer.DockArea);
            }
        }

        private static void RestoreDockItemSavedState(SerialisedDockItem item, Control parent)
        {
            if (item is SerialisedDockWindow serialisedWindow)
            {
                ToolWindow window = HUDPlayer.FloatingArea.GetNode<ToolWindow>(serialisedWindow.WindowName);
                window.GetParent().RemoveChild(window);
                parent.AddChild(window);
                if (!(parent is SplitContainer))
                {
                    window.SetSize(parent.RectSize);
                    window.SetPosition(new Vector2(0, 0));
                }

                window.IsDocked = true;

                if (s_DockRoot == null)
                {
                    s_DockRoot = window;
                }
            }
            else if (item is SerialisedDockContainer serialisedContainer)
            {
                SplitContainer container = null;
                if (serialisedContainer.Vertical)
                {
                    container = new VSplitContainer();
                }
                else
                {
                    container = new HSplitContainer();
                }

                parent.AddChild(container);

                if (!(parent is SplitContainer))
                {
                    container.SetSize(HUDPlayer.DockArea.RectSize);
                }

                foreach (SerialisedDockItem child in serialisedContainer.Children)
                {
                    RestoreDockItemSavedState(child, container);
                }

                container.SplitOffset = serialisedContainer.SplitOffset;

                if (s_DockRoot == null)
                {
                    s_DockRoot = container;
                }
            }
            else if (item is SerialisedDockPlaceholder serialisedDockPlaceholder)
            {
                PackedScene placeholderRect = GD.Load<PackedScene>("res://Layouts/DockPlaceholder.tscn");
                s_DockPlaceholder = placeholderRect.Instance() as ReferenceRect;
                parent.AddChild(s_DockPlaceholder);
            }
        }
        
        

        static SerialisedDockItem DeserialiseDockItem(JObject item)
        {
            SerialisedDockItem dockItem = null;

            int typInt = (int)item["Type"];
            SerialisedDockItemType type = (SerialisedDockItemType)typInt;
            switch (type)
            {
                case SerialisedDockItemType.SplitContainer:
                {
                    SerialisedDockContainer container = new SerialisedDockContainer();
                    container.Vertical = (bool)item["Vertical"];
                    List<SerialisedDockItem> children = new List<SerialisedDockItem>();
                    foreach (JObject child in item["Children"])
                    {
                        children.Add(DeserialiseDockItem(child));
                    }
            
                    container.Children = children.ToArray();
                    container.SplitOffset = (int)item["SplitOffset"];
                    dockItem = container;

                    break;
                }
                case SerialisedDockItemType.Window:
                {
                    SerialisedDockWindow window = new SerialisedDockWindow();
                    window.WindowName = (string)item["WindowName"];
                    dockItem = window;
                    break;
                }
                case SerialisedDockItemType.Placeholder:
                {
                    SerialisedDockPlaceholder placeholder = new SerialisedDockPlaceholder();
                    dockItem = placeholder;
                    break;
                }
            }

            return dockItem;
        }

        private static SerialisedDockWindow SerialiseDockWindow(ToolWindow toolWindow)
        {
            SerialisedDockWindow window = new SerialisedDockWindow();
            window.WindowName = toolWindow.Name;

            return window;
        }
        private static SerialisedDockContainer SerialiseSplitContainer(SplitContainer splitContainer)
        {
            SerialisedDockContainer container = new SerialisedDockContainer();
            container.Vertical = splitContainer is VSplitContainer;
            container.Children = SerialiseContainerChildren(splitContainer);
            container.SplitOffset = splitContainer.SplitOffset;

            return container;
        }

        private static SerialisedDockItem[] SerialiseContainerChildren(SplitContainer container)
        {
            List<SerialisedDockItem> dockItems = new List<SerialisedDockItem>();
            foreach (Node child in container.GetChildren())
            {
                if (child is SplitContainer splitContainer)
                {
                    dockItems.Add(SerialiseSplitContainer(splitContainer));
                }
                else if (child is ToolWindow window)
                {
                    dockItems.Add(SerialiseDockWindow(window));
                }
                else
                {
                    dockItems.Add(new SerialisedDockPlaceholder());
                }
            }

            return dockItems.ToArray();
        }
    }
}