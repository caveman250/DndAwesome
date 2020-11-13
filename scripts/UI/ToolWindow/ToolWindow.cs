using Godot;

namespace DndAwesome.scripts.UI.ToolWindow
{
    public class ToolWindow : Panel
    {
        public bool IsDocked { get; set; }

        private enum ResizeDirection
        {
            Top,
            Left,
            Bottom,
            Right,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            None
        }

        public bool Focused { get; set; }

        public void OnMouseEnter()
        {
            Focused = true;
            m_ResizeDirection = ResizeDirection.None;
        }

        public void OnMouseExit()
        {
            Focused = false;
            m_ResizeDirection = ResizeDirection.None;
        }

        private ResizeDirection m_ResizeDirection = ResizeDirection.None;

        public override void _Ready()
        { 
            DockingManager.RegisterToolWindow(this);
            Connect("mouse_entered", this, "OnMouseEnter");
            Connect("mouse_exited", this, "OnMouseExit");
        }

        private void SetCursorTypeForResizeDirection(ResizeDirection direction)
        {
            switch (direction)
            {
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:
                    MouseDefaultCursorShape = CursorShape.Fdiagsize;
                    break;
                case ResizeDirection.BottomLeft:
                case ResizeDirection.TopRight:
                    MouseDefaultCursorShape = CursorShape.Bdiagsize;
                    break;
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    MouseDefaultCursorShape = CursorShape.Hsize;
                    break;
                case ResizeDirection.Top:
                case ResizeDirection.Bottom:
                    MouseDefaultCursorShape = CursorShape.Vsize;
                    break;
                case ResizeDirection.None:
                    MouseDefaultCursorShape = CursorShape.Arrow;
                    break;
            }
        }

        private ResizeDirection CalculateResizeDirection(Vector2 mousePos)
        {
            if (IsDocked || GetNode<TitleBar>("TitleBar").MouseDragging)
            {
                 return ResizeDirection.None;
            }
            
            float distanceFromTop = Mathf.Abs(mousePos.y - GetGlobalRect().Position.y);
            float distanceFromLeft = Mathf.Abs(mousePos.x - GetGlobalRect().Position.x);
            float distanceFromBottom = Mathf.Abs(mousePos.y - (GetGlobalRect().Position.y + GetRect().Size.y));
            float distanceFromRight = Mathf.Abs(mousePos.x - (GetGlobalRect().Position.x + GetRect().Size.x));

            if ((distanceFromTop < 5 && distanceFromLeft < 5))
            {
                return ResizeDirection.TopLeft;
            }
            else if (distanceFromBottom < 5 && distanceFromRight < 5)
            {
                return ResizeDirection.BottomRight;
            }
            else if ((distanceFromTop < 5 && distanceFromRight < 5))
            {
                return ResizeDirection.TopRight;
            }
            else if (distanceFromBottom < 5 && distanceFromLeft < 5)
            {
                return ResizeDirection.BottomLeft;
            }
            else if (distanceFromLeft < 5)
            {
                return ResizeDirection.Left;
            }
            else if (distanceFromRight < 5)
            {
                return ResizeDirection.Right;
            }
            else if (distanceFromTop < 5)
            {
                return ResizeDirection.Top;
            }
            else if (distanceFromBottom < 5)
            {
                return ResizeDirection.Bottom;
            }
            else
            {
                return ResizeDirection.None;
            }
        }

        private void DoResize(Vector2 mousePos)
        {
            Vector2 topLeft = RectGlobalPosition;
            Vector2 bottomRight = RectGlobalPosition + RectSize;
            
            switch (m_ResizeDirection)
            {
                case ResizeDirection.Top:
                    topLeft = new Vector2(RectGlobalPosition.x, mousePos.y);
                    break;
                case ResizeDirection.Left:
                    topLeft = new Vector2(mousePos.x, RectGlobalPosition.y);
                    break;
                case ResizeDirection.Bottom:
                    bottomRight = new Vector2(bottomRight.x, mousePos.y);
                    break;
                case ResizeDirection.Right:
                    bottomRight = new Vector2(mousePos.x, bottomRight.y);
                    break;
                case ResizeDirection.TopLeft:
                    topLeft = mousePos;
                    break;
                case ResizeDirection.TopRight:
                    topLeft = new Vector2(topLeft.x, mousePos.y);
                    bottomRight = new Vector2(mousePos.x, bottomRight.y);
                    break;
                case ResizeDirection.BottomLeft:
                    topLeft = new Vector2(mousePos.x, topLeft.y);
                    bottomRight = new Vector2(bottomRight.x, mousePos.y);
                    break;
                case ResizeDirection.BottomRight:
                    bottomRight = mousePos;
                    break;
            }
            
            SetGlobalPosition(topLeft);
            SetSize(bottomRight - topLeft);
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (!Focused)
            {
                return;
            }

            if (inputEvent is InputEventMouse mouseEvent)
            {
                ResizeDirection direction = CalculateResizeDirection(mouseEvent.Position);
                SetCursorTypeForResizeDirection(direction);

                if (mouseEvent is InputEventMouseButton buttonEvent)
                {
                    if (buttonEvent.ButtonIndex == (int)ButtonList.Left)
                    {
                        m_ResizeDirection = buttonEvent.Pressed ? direction : ResizeDirection.None;
                    }
                }

                if (mouseEvent is InputEventMouseMotion motionEvent)
                {
                    if (m_ResizeDirection != ResizeDirection.None)
                    {
                        DoResize(mouseEvent.Position);
                    }
                }
            }

            base._Input(inputEvent);
        }
    }
}
