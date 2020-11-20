using DndAwesome.scripts.UI;
using Godot;

namespace DndAwesome.scripts
{
    public class BackgroundImage : TextureRect
    {
        private Control TopLeftDrag { get; set; }
        private Control MidTopDrag { get; set; }
        private Control TopRightDrag { get; set; }
        private Control MidRightDrag { get; set; }
        private Control BottomRightDrag { get; set; }
        private Control MidBottomDrag { get; set; }
        private Control BottomLeftDrag { get; set; }
        private Control MidLeftDrag { get; set; }
        
        private enum ResizeDirectionEnum
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
        
        private bool IsSelected { get; set; }
        private bool IsMoving { get; set; }
        private Vector2 MoveMouseOffset { get; set; }
        private ResizeDirectionEnum ResizeDirection { get; set; } = ResizeDirectionEnum.None;
        public bool Focused { get; set; }
        
        public void OnMouseEnter()
        {
            Focused = true;
            ResizeDirection = ResizeDirectionEnum.None;
        }

        public void OnMouseExit()
        {
            Focused = false;
            ResizeDirection = ResizeDirectionEnum.None;
        }

        public override void _Ready()
        {
            Connect("mouse_entered", this, "OnMouseEnter");
            Connect("mouse_exited", this, "OnMouseExit");
            IsSelected = false;
            Focused = false;
            
            TopLeftDrag = GetNode<Control>("ResizeGroup/TopLeftDrag");
            MidTopDrag = GetNode<Control>("ResizeGroup/MidTopDrag");
            TopRightDrag = GetNode<Control>("ResizeGroup/TopRightDrag");
            MidRightDrag = GetNode<Control>("ResizeGroup/MidRightDrag");
            BottomRightDrag = GetNode<Control>("ResizeGroup/BottomRightDrag");
            MidBottomDrag = GetNode<Control>("ResizeGroup/MidBottomDrag");
            BottomLeftDrag = GetNode<Control>("ResizeGroup/BottomLeftDrag");
            MidLeftDrag = GetNode<Control>("ResizeGroup/MidLeftDrag");
            
            TopLeftDrag.MouseDefaultCursorShape = CursorShape.Fdiagsize;
            BottomRightDrag.MouseDefaultCursorShape = CursorShape.Fdiagsize;
            TopRightDrag.MouseDefaultCursorShape = CursorShape.Bdiagsize;
            BottomLeftDrag.MouseDefaultCursorShape = CursorShape.Bdiagsize;
            MidLeftDrag.MouseDefaultCursorShape = CursorShape.Hsize;
            MidRightDrag.MouseDefaultCursorShape = CursorShape.Hsize;
            MidTopDrag.MouseDefaultCursorShape = CursorShape.Vsize;
            MidRightDrag.MouseDefaultCursorShape = CursorShape.Vsize;
        }

        public bool Input(InputEvent inputEvent)
        {
            GameWindow window = SceneObjectManager.GetGameWindow();

            if (inputEvent is InputEventMouse mouseEvent)
            {
                ResizeDirectionEnum potentialResizeDirection = ResizeDirectionEnum.None;
                
                if (IsMoving)
                {
                    SetPosition(window.GetGameViewPosFromScreenPos(mouseEvent.Position) - MoveMouseOffset);
                }
                else
                {
                    if (window.IsMousePointInBounds(mouseEvent.Position, TopLeftDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.TopLeft;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, MidTopDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.Top;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, TopRightDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.TopRight;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, MidRightDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.Right;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, BottomRightDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.BottomRight;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, MidBottomDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.Bottom;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, BottomLeftDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.BottomLeft;
                    }
                    else if (window.IsMousePointInBounds(mouseEvent.Position, MidLeftDrag))
                    {
                        potentialResizeDirection = ResizeDirectionEnum.Left;
                    }
                    else
                    {
                        potentialResizeDirection = ResizeDirectionEnum.None;
                    }
                }

                bool canMove = false;
                if (IsSelected && 
                    potentialResizeDirection == ResizeDirectionEnum.None && 
                    window.IsMousePointInBounds(mouseEvent.Position, this))
                {
                    SceneObjectManager.GetGameWindow().MouseDefaultCursorShape = CursorShape.Move;
                    canMove = true;
                }
                else
                {
                    SetCursorTypeForResizeDirection(potentialResizeDirection);
                }

                if (inputEvent is InputEventMouseButton buttonEvent)
                {
                    if (buttonEvent.ButtonIndex == (int) ButtonList.Left)
                    {
                        if (buttonEvent.Pressed)
                        {
                            if (IsSelected && potentialResizeDirection != ResizeDirectionEnum.None)
                            {
                                ResizeDirection = potentialResizeDirection;
                            }
                            else
                            {
                                if (window.IsMousePointInBounds(buttonEvent.Position, this))
                                {
                                    if (!IsSelected)
                                    {
                                        IsSelected = true;
                                        GetNode<Control>("ResizeGroup").Visible = true;
                                    }

                                    if (canMove)
                                    {
                                        IsMoving = true;
                                        MoveMouseOffset = window.GetGameViewPosFromScreenPos(mouseEvent.Position) - RectPosition;
                                    }
                                    
                                    return true;
                                }
                                else if (IsSelected)
                                {
                                    IsSelected = false;
                                    GetNode<Control>("ResizeGroup").Visible = false;
                                }
                            }
                        }
                        else
                        {
                            ResizeDirection = ResizeDirectionEnum.None;
                            IsMoving = false;
                        }
                    }
                }

                if (mouseEvent is InputEventMouseMotion motionEvent)
                {
                    if (ResizeDirection != ResizeDirectionEnum.None)
                    {
                        DoResize(mouseEvent.Position);
                        return true;
                    }
                }
            }
            
            base._Input(inputEvent);
            return false;
        }
        
        private void SetCursorTypeForResizeDirection(ResizeDirectionEnum direction)
        {
            CursorShape desiredCursorShape = CursorShape.Arrow;
            switch (direction)
            {
                case ResizeDirectionEnum.TopLeft:
                case ResizeDirectionEnum.BottomRight:
                    desiredCursorShape = CursorShape.Fdiagsize;
                    break;
                case ResizeDirectionEnum.BottomLeft:
                case ResizeDirectionEnum.TopRight:
                    desiredCursorShape = CursorShape.Bdiagsize;
                    break;
                case ResizeDirectionEnum.Left:
                case ResizeDirectionEnum.Right:
                    desiredCursorShape = CursorShape.Hsize;
                    break;
                case ResizeDirectionEnum.Top:
                case ResizeDirectionEnum.Bottom:
                    desiredCursorShape = CursorShape.Vsize;
                    break;
                case ResizeDirectionEnum.None:
                    desiredCursorShape = CursorShape.Arrow;
                    break;
            }

            SceneObjectManager.GetGameWindow().MouseDefaultCursorShape = desiredCursorShape;
        }
        
        private void DoResize(Vector2 mousePos)
        {
            GameWindow window = SceneObjectManager.GetGameWindow();
            Vector2 translatedMousePos = window.GetGameViewPosFromScreenPos(SceneObjectManager.GetCamera().WorldPosToScreenPos(mousePos));
            Vector2 topLeft = RectGlobalPosition;
            Vector2 bottomRight = topLeft + RectSize;
            
            switch (ResizeDirection)
            {
                case ResizeDirectionEnum.Top:
                    topLeft = new Vector2(RectGlobalPosition.x, translatedMousePos.y);
                    break;
                case ResizeDirectionEnum.Left:
                    topLeft = new Vector2(translatedMousePos.x, RectGlobalPosition.y);
                    break;
                case ResizeDirectionEnum.Bottom:
                    bottomRight = new Vector2(bottomRight.x, translatedMousePos.y);
                    break;
                case ResizeDirectionEnum.Right:
                    bottomRight = new Vector2(translatedMousePos.x, bottomRight.y);
                    break;
                case ResizeDirectionEnum.TopLeft:
                    topLeft = translatedMousePos;
                    break;
                case ResizeDirectionEnum.TopRight:
                    topLeft = new Vector2(topLeft.x, translatedMousePos.y);
                    bottomRight = new Vector2(translatedMousePos.x, bottomRight.y);
                    break;
                case ResizeDirectionEnum.BottomLeft:
                    topLeft = new Vector2(translatedMousePos.x, topLeft.y);
                    bottomRight = new Vector2(bottomRight.x, translatedMousePos.y);
                    break;
                case ResizeDirectionEnum.BottomRight:
                    bottomRight = translatedMousePos;
                    break;
            }
            
            SetGlobalPosition(topLeft);
            SetSize(bottomRight - topLeft);
        }
    }
}
