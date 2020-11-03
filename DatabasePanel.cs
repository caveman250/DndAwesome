using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using Godot.Collections;
using Directory = System.IO.Directory;

public class DatabasePanel : Panel
{
    private List<object> backStack = new List<object>();
    private Array _selectedArray;
    private Dictionary _selectedObject;

    private bool _panelOpen = false;
    private bool _isAnimating = false;
    private Vector2 _animateStartPosition;
    private Vector2 _targetPosition;
    private const float AnimateTargetTime = 0.3f;
    private readonly Vector2 TargetOpenPosition = new Vector2(0.0f, 0.0f);
    private readonly Vector2 TargetClosedPosition = new Vector2(-600.0f, 0.0f);
    private float _animateCurrentTime = 0.0f;
    
    private DynamicFont _font = (DynamicFont)GD.Load("res://fonts/jmHarkam.tres");
    private DynamicFont _boldFont = (DynamicFont)GD.Load("res://fonts/jmHarkam_bold.tres");

    private Godot.Collections.Dictionary<string, Array> m_Db = new Godot.Collections.Dictionary<string, Array>();

    private void LoadDataBase(string path)
    {
        string rawJson = System.IO.File.ReadAllText(path);
        string key = path.Split('/').Last();
        m_Db[key] = (Array) JSON.Parse(rawJson).Result;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (var file in System.IO.Directory.EnumerateFiles("data/database"))
        {
            LoadDataBase(file);
        }
    }

    public void TogglePanel()
    {
        _animateStartPosition = GetRect().Position;
        _targetPosition = _panelOpen ? TargetClosedPosition : TargetOpenPosition;
        _isAnimating = true;
        _animateCurrentTime = 0.0f;
    }

    private void AddTextNode(string text)
    {
        VBoxContainer container = (VBoxContainer)GetNode("ScrollContainer/PanelContainer");

        RichTextLabel textLabel = new RichTextLabel {FitContentHeight = true, BbcodeEnabled = true, BbcodeText = text};
        //textLabel.RectMinSize = new Vector2(((ScrollContainer)container.GetParent()).RectSize.x, textLabel.RectSize.y);
        textLabel.AddFontOverride("normal_font", _font);
        textLabel.AddFontOverride("bold_font", _boldFont);
        container.AddChild(textLabel);
    }
    
    private void AddButtonNode(string text, object obj)
    {
        Node container = GetNode("ScrollContainer/PanelContainer");

        Button button = new Button {Text = text};
        button.Connect("pressed", this, "OnButtonPress", new Array() {obj});
        button.AddFontOverride("font", _font);
        //button.RectMinSize = new Vector2(((ScrollContainer)container.GetParent()).RectSize.x - 20, button.RectSize.y);
        container.AddChild(button);
    }

    static bool hasUpdated = false;

    private void PrintDictionary(Dictionary dict, bool expand, bool parentIsArray, string name)
    {
        if (expand || parentIsArray)
        {
            if (!parentIsArray && name != null)
            {
                AddTextNode($"[b]{name}:[/b]");
            }
            
            foreach (DictionaryEntry kvp in dict)
            {
                if (kvp.Value is Dictionary)
                {
                    PrintDictionary(kvp.Value as Dictionary, true, false, kvp.Key.ToString());
                }
                else if (kvp.Value is Array arr)
                {
                    PrintArray(arr, kvp.Key.ToString(), true, false);
                }
                else
                {
                    if ((string) kvp.Key == "index" || (string) kvp.Key == "url")
                    {
                        continue;
                    }

                    if ((string) kvp.Key == "name" && parentIsArray || ShouldForceExpandDictionary(dict))
                    {
                        AddTextNode($"{kvp.Value}");
                    }
                    else
                    {
                        AddTextNode($"[b]{kvp.Key}:[/b] {kvp.Value}");
                    }
                }
            }
        }
        else
        {
            if (((Array) dict.Keys).Contains("name"))
            {
                AddButtonNode(dict["name"].ToString(), dict);
            }
            else
            {
                PrintDictionary(dict, true, false, name);
            }
        }
    }

    bool ShouldForceExpandDictionary(Dictionary dict)
    {
        //repeated pattern in the database.
        return dict.Keys.Count == 3 && ((Array) dict.Keys).Contains("index") &&
            ((Array) dict.Keys).Contains("name") && ((Array) dict.Keys).Contains("url");
    }

    private void PrintArray(Array array, string name, bool expand, bool parentIsArray)
    {
        Node container = GetNode("ScrollContainer/PanelContainer");

        if (expand)
        {
            if (name.Length > 0)
            {
                AddTextNode($"[b]{name}:[/b]");
            }
            
            foreach (var arrayVal in array)
            {
                if (arrayVal is Dictionary dict)
                {
                    //dirty hack to force the first menu of each category to be buttons
                    if (backStack.Count > 1)
                    {
                        PrintDictionary(dict, true, true, null);
                    }
                    else
                    {
                        PrintDictionary(dict, false, false, null);
                    }
                }
                else
                {
                    AddTextNode(arrayVal.ToString());
                }
            }
        }
        else
        {
            foreach (var arrayVal in array)
            {
                AddButtonNode(name, arrayVal);
            }
        }
    }

    void OnBackButton()
    {
        object newObj = backStack.Last();
        backStack.RemoveAt(backStack.Count - 1);

        if (newObj is Dictionary)
        {
            _selectedObject = newObj as Dictionary;
            _selectedArray = null;
        }
        else
        {
            _selectedArray = newObj as Array;
            _selectedObject = null;
        }

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        Node container = GetNode("ScrollContainer/PanelContainer");

        foreach (Node child in container.GetChildren())
        {
            container.RemoveChild(child);
        }

        if (backStack.Count > 0)
        {
            Button backButton = new Button();
            backButton.Text = "Back";
            backButton.Connect("pressed", this, "OnBackButton");
            backButton.AddFontOverride("font", _font);
            container.AddChild(backButton);
        }

        if (_selectedArray != null)
        {
            PrintArray(_selectedArray, "", true, false);
        }
        else if (_selectedObject != null)
        {
            PrintDictionary(_selectedObject, true, false, null);
        }
        else
        {
            foreach (var db in m_Db)
            {
                TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;
                AddButtonNode(textInfo.ToTitleCase(db.Key.Split(".").First().Replace("-", " ")), db.Value);
            }
        }
    }

    private void OnButtonPress(object buttonObj)
    {
        if (_selectedArray != null)
        {
            backStack.Add(_selectedArray);
        }
        else
        {
            backStack.Add(_selectedObject);
        }

        if (buttonObj is Dictionary)
        {
            _selectedObject = (Dictionary) buttonObj;
            _selectedArray = null;
        }
        else if (buttonObj is Array)
        {
            _selectedArray = (Array) buttonObj;
            _selectedObject = null;
        }

        RefreshButtons();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!hasUpdated)
        {
            Node container = GetNode("ScrollContainer/PanelContainer");
            if (container != null)
            {
                RefreshButtons();
                hasUpdated = true;
            }
        }

        if (_isAnimating)
        {
            _animateCurrentTime += delta;
            float lerp = _animateCurrentTime / AnimateTargetTime;
            Vector2 newPos = new Vector2(Mathf.Lerp(_animateStartPosition.x, _targetPosition.x, lerp),
                Mathf.Lerp(_animateStartPosition.y, _targetPosition.y, lerp));

            SetPosition(newPos);

            if (_animateCurrentTime >= AnimateTargetTime)
            {
                _isAnimating = false;
                SetPosition(_targetPosition);
                _panelOpen = !_panelOpen;
            }
        }
    }
}