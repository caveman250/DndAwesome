using Godot;

namespace DndAwesome.scripts
{
    public class PanelButton : Button
    {
        private DatabasePanel m_Panel;
        public override void _Ready()
        {
            m_Panel = GetNode<DatabasePanel>("../");
            Connect("pressed", this, "OnButtonPress");
        }

        private void OnButtonPress()
        {
            m_Panel.TogglePanel();
        }

        public override void _Input(InputEvent input)
        {
            if (input is InputEventKey inputKey)
            {
                if (inputKey.Control)
                {
                    if (inputKey.Scancode == (uint) KeyList.F)
                    {
                        if (m_Panel.IsPanelOpen())
                        {
                            m_Panel.FocusSearchBox();
                        }
                        else
                        {
                            m_Panel.TogglePanel();
                        }
                    }
                }
                else if (inputKey.Scancode == (uint) KeyList.Escape)
                {
                    if (m_Panel.IsPanelOpen())
                    {
                        m_Panel.TogglePanel();
                    }
                }
            }

            base._Input(input);
        }
    }
}
