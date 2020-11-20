using Godot;

namespace DndAwesome.scripts.UI.DmScreens.EditBackgroundImageWindow
{
    public class EditBackgroundImageScreen : Control
    {
        private RichTextLabel Text { get; set; }
        private OptionButton OptionButton { get; set; }
        
        public override void _Ready()
        {
            SetupSnapToGridComboBox();
        }

        private void SetupSnapToGridComboBox()
        {
            Control comboBox = GetNode<Control>("Scroll/Contents/SnapToGrid");

            Text = comboBox.GetNode<RichTextLabel>("Text");
            Text.Text = "Snap To Grid";

            OptionButton = comboBox.GetNode<OptionButton>("OptionButton");
            OptionButton.AddItem("Enabled");
            OptionButton.AddItem("Disabled");
            OptionButton.Connect("item_selected", this, "OnSnapToGridSelectionChanged");
        }

        private void OnSnapToGridSelectionChanged(int index)
        {
            string selectedOption = OptionButton.GetItemText(index);
            switch (selectedOption)
            {
                case "Enabled":
                    DmSettings.BackgoundImageSnapToGrid = true;
                    break;
                case "Disabled":
                    DmSettings.BackgoundImageSnapToGrid = false;
                    break;
            }
        }
    }
}
