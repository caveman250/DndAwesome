using Godot;

namespace DndAwesome.scripts
{
    public class Camera2D : Godot.Camera2D
    {
        //True when the user is holding the middle mouse button.
        private bool m_IsPanning;

        public override void _Input(InputEvent input)
        {
            if (input is InputEventMouseButton mouseButtonEvent)
            {
                switch (mouseButtonEvent.ButtonIndex)
                {
                    case (int)ButtonList.Middle:
                        m_IsPanning = mouseButtonEvent.Pressed;
                        break;
                    case (int)ButtonList.WheelDown:
                        ZoomCamera(0.01f, mouseButtonEvent.Position);
                        break;
                    case (int)ButtonList.WheelUp:
                        ZoomCamera(-0.01f, mouseButtonEvent.Position);
                        break;
                }
            }

            if (m_IsPanning)
            {
                if (input is InputEventMouseMotion mouseMotionEvent)
                {
                    Position -= mouseMotionEvent.Relative;
                }
            }

            base._Input(input);
        }

        private void ZoomCamera(float amount, Vector2 zoomPoint)
        {
            Vector2 newZoom = Zoom + new Vector2(amount, amount);
            Vector2 newCameraPosition = Position + zoomPoint * (Zoom - newZoom);

            Zoom = newZoom;
            Position = newCameraPosition;
        }

        public Vector2 WorldPosToScreenPos(Vector2 worldPos)
        {
            return Position + worldPos * Zoom;
        }
    }
}