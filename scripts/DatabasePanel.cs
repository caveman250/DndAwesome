using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using Godot.Collections;

namespace DndAwesome.scripts
{
    public class DatabasePanel : Panel
    {
        //Animation
        private bool m_IsAnimating;
        private Vector2 m_AnimateStartPosition;
        private Vector2 m_AnimateTargetPosition;
        private const float c_AnimationTime = 0.2f;
        private readonly Vector2 m_TargetOpenPosition = new Vector2(0.0f, 24.0f);
        private readonly Vector2 m_TargetClosedPosition = new Vector2(-400.0f, 24.0f);
        private float m_AnimateCurrentTime;

        //State
        private bool m_PanelOpen;
        private bool m_PanelFocused;
        private readonly List<object> m_BackStack = new List<object>();

        //List View
        private Node m_ListContainer;
        private Array m_SelectedArray;
        private Dictionary m_SelectedObject;
        private readonly DynamicFont m_Font = (DynamicFont)GD.Load("res://fonts/jmHarkam.tres");

        private readonly DynamicFont m_BoldFont = (DynamicFont)GD.Load("res://fonts/jmHarkam_bold.tres");

        //Search Bar
        private LineEdit m_SearchField;
        private string m_SearchString = "";

        private readonly Godot.Collections.Dictionary<string, Array> m_Databases =
            new Godot.Collections.Dictionary<string, Array>();

        //Public Methods
        public bool IsPanelOpen()
        {
            return m_PanelOpen;
        }

        public bool IsPanelFocused()
        {
            return m_PanelFocused;
        }


        public void FocusSearchBox()
        {
            m_SearchField.GrabFocus();
        }

        public void TogglePanel()
        {
            m_AnimateStartPosition = GetRect().Position;
            m_AnimateTargetPosition = m_PanelOpen ? m_TargetClosedPosition : m_TargetOpenPosition;
            m_IsAnimating = true;
            m_AnimateCurrentTime = 0.0f;
        }

        //Godot methods
        public override void _Input(InputEvent input)
        {
            if (input is InputEventMouse mouseEvent)
            {
                m_PanelFocused = mouseEvent.Position.x > GetRect().Position.x &&
                                 mouseEvent.Position.y > GetRect().Position.y &&
                                 mouseEvent.Position.x < GetRect().Position.x + GetRect().Size.x &&
                                 mouseEvent.Position.y < GetRect().Position.y + GetRect().Size.y;
            }

            base._Input(input);
        }

        public override void _Process(float delta)
        {
            if (m_ListContainer == null)
            {
                RefreshButtons();
            }

            if (!m_IsAnimating)
            {
                return;
            }

            m_AnimateCurrentTime += delta;
            float lerp = m_AnimateCurrentTime / c_AnimationTime;
            Vector2 newPos = new Vector2(Mathf.Lerp(m_AnimateStartPosition.x, m_AnimateTargetPosition.x, lerp),
                                         Mathf.Lerp(m_AnimateStartPosition.y, m_AnimateTargetPosition.y, lerp));

            SetPosition(newPos);

            if (m_AnimateCurrentTime >= c_AnimationTime)
            {
                m_IsAnimating = false;
                SetPosition(m_AnimateTargetPosition);
                m_PanelOpen = !m_PanelOpen;
                if (m_PanelOpen)
                {
                    m_SearchField.GrabFocus();
                }
            }
        }

        public override void _Ready()
        {
            foreach (var file in System.IO.Directory.EnumerateFiles("data/database"))
            {
                LoadDataBase(file);
            }
        }

        private void LoadDataBase(string path)
        {
            string rawJson = System.IO.File.ReadAllText(path);
#if GODOT_WINDOWS
			string key = path.Split('\\').Last();
#else
            string key = path.Split('/').Last();
#endif
            m_Databases[key] = (Array)JSON.Parse(rawJson).Result;
        }

        private static bool ShouldForceExpandDictionary(IDictionary dict)
        {
            //repeated pattern in the database.
            return dict.Keys.Count == 3 &&
                   ((Array)dict.Keys).Contains("index") &&
                   ((Array)dict.Keys).Contains("name") &&
                   ((Array)dict.Keys).Contains("url");
        }

        private void AddTextNode(string text)
        {
            VBoxContainer container = (VBoxContainer)GetNode("ScrollContainer/PanelContainer");

            RichTextLabel textLabel = new RichTextLabel
                { FitContentHeight = true, BbcodeEnabled = true, BbcodeText = text };
            textLabel.AddFontOverride("normal_font", m_Font);
            textLabel.AddFontOverride("bold_font", m_BoldFont);
            container.AddChild(textLabel);
        }

        private void AddButtonNode(string text, object obj)
        {
            Node container = GetNode("ScrollContainer/PanelContainer");

            Button button = new Button { Text = text };
            button.Connect("pressed", this, "OnButtonPress", new Array() { obj });
            button.AddFontOverride("font", m_Font);
            container.AddChild(button);
        }

        private void PrintDictionary(Dictionary dict, bool expand, bool parentIsArray, string name)
        {
            if (expand || parentIsArray)
            {
                if (!parentIsArray && name != null)
                {
                    TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;
                    string titleCaseName = textInfo.ToTitleCase(name.Replace("_", " "));
                    AddTextNode($"[b]{titleCaseName}:[/b]");
                }

                foreach (DictionaryEntry kvp in dict)
                {
                    if (kvp.Value is Dictionary value)
                    {
                        PrintDictionary(value, true, false, kvp.Key.ToString());
                    }
                    else if (kvp.Value is Array arr)
                    {
                        PrintArray(arr, kvp.Key.ToString(), true);
                    }
                    else
                    {
                        if ((string)kvp.Key == "index" || (string)kvp.Key == "url")
                        {
                            continue;
                        }

                        if ((string)kvp.Key == "name" && parentIsArray || ShouldForceExpandDictionary(dict))
                        {
                            AddTextNode($"{kvp.Value}");
                        }
                        else
                        {
                            TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;
                            string titleCaseName = textInfo.ToTitleCase(kvp.Key.ToString().Replace("_", " "));
                            AddTextNode($"[b]{titleCaseName}:[/b] {kvp.Value}");
                        }
                    }
                }
            }
            else
            {
                if (((Array)dict.Keys).Contains("name"))
                {
                    AddButtonNode(dict["name"].ToString(), dict);
                }
                else if (((Array)dict.Keys).Contains("class"))
                {
                    AddButtonNode(((Dictionary)dict["class"])["name"].ToString(), dict);
                }
                else
                {
                    PrintDictionary(dict, true, false, name);
                }
            }
        }

        private void PrintArray(IEnumerable array, string name, bool expand)
        {
            if (expand)
            {
                if (name.Length > 0)
                {
                    TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;
                    string titleCaseName = textInfo.ToTitleCase(name.Replace("_", " "));
                    AddTextNode($"[b]{titleCaseName}:[/b]");
                }

                foreach (object arrayVal in array)
                {
                    if (arrayVal is Dictionary dict)
                    {
                        //dirty hack to force the first menu of each category to be buttons
                        if (m_BackStack.Count > 1 || m_SearchString.Length > 0)
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
                foreach (object arrayVal in array)
                {
                    AddButtonNode(name, arrayVal);
                }
            }
        }

        private void RefreshButtons()
        {
            m_ListContainer ??= GetNode("ScrollContainer/PanelContainer");

            foreach (Node child in m_ListContainer.GetChildren())
            {
                m_ListContainer.RemoveChild(child);
            }

            m_SearchField = new LineEdit();
            m_SearchField.Connect("text_changed", this, "OnSearchChanged");
            m_SearchField.Connect("text_entered", this, "OnSearchEnter");
            m_ListContainer.AddChild(m_SearchField);
            m_SearchField.GrabFocus();

            if (m_BackStack.Count > 0)
            {
                Button backButton = new Button { Text = "Back" };
                backButton.Connect("pressed", this, "OnBackButton");
                backButton.AddFontOverride("font", m_Font);
                m_ListContainer.AddChild(backButton);
            }

            if (m_SelectedArray != null)
            {
                PrintArray(m_SelectedArray, "", true);
            }
            else if (m_SelectedObject != null)
            {
                PrintDictionary(m_SelectedObject, true, false, null);
            }
            else
            {
                foreach ((string key, Array value) in m_Databases)
                {
                    TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;
                    AddButtonNode(textInfo.ToTitleCase(key.Split(".").First().Replace("-", " ")), value);
                }
            }
        }

        private struct SearchResult
        {
            public string Name;
            public object Value;
        }

        private IEnumerable<SearchResult> SearchDictionary(IDictionary dict, string searchString)
        {
            List<SearchResult> foundObjects = new List<SearchResult>();
            if (((Array)dict.Keys).Contains("name"))
            {
                if (dict["name"].ToString().ToLower().Contains(searchString))
                {
                    foundObjects.Add(new SearchResult { Name = dict["name"].ToString(), Value = dict });
                }
            }
            else
            {
                foreach (DictionaryEntry kvp in dict)
                {
                    switch (kvp.Value)
                    {
                        case Array array:
                            foundObjects.AddRange(SearchArray(array, searchString));
                            break;
                        case Dictionary value:
                            foundObjects.AddRange(SearchDictionary(value, searchString));
                            break;
                    }
                }
            }

            return foundObjects;
        }

        private IEnumerable<SearchResult> SearchArray(IEnumerable arr, string searchString)
        {
            List<SearchResult> foundObjects = new List<SearchResult>();
            foreach (object value in arr)
            {
                switch (value)
                {
                    case Array array:
                        foundObjects.AddRange(SearchArray(array, searchString));
                        break;
                    case Dictionary dictionary:
                        foundObjects.AddRange(SearchDictionary(dictionary, searchString));
                        break;
                }
            }

            return foundObjects;
        }

        private static int CommonChars(string left, string right)
        {
            return left.GroupBy(c => c).
                        Join(right.GroupBy(c => c),
                             g => g.Key,
                             g => g.Key,
                             (lg, rg) => lg.Zip(rg, (l, r) => l).Count()).
                        Sum();
        }

        private void OnSearchChanged(string newText)
        {
            m_SearchString = newText.ToLower();
            if (newText.Length > 0)
            {
                Node container = GetNode("ScrollContainer/PanelContainer");

                foreach (object child in container.GetChildren())
                {
                    if (child is LineEdit)
                    {
                        continue;
                    }

                    Node node = child as Node;
                    container.RemoveChild(node);
                }

                List<SearchResult> foundObjects = new List<SearchResult>();
                foreach (KeyValuePair<string, Array> kvp in m_Databases)
                {
                    foundObjects.AddRange(SearchArray(kvp.Value, m_SearchString));
                }

                foundObjects.Sort((a, b) =>
                {
                    //get the number of characters different between the results and the search string, show the closest matches first.
                    int diffA = a.Name.Length - CommonChars(a.Name, m_SearchString);
                    int diffB = b.Name.Length - CommonChars(b.Name, m_SearchString);

                    if (diffA < diffB)
                    {
                        return -1;
                    }

                    if (diffB < diffA)
                    {
                        return 1;
                    }

                    return 0;
                });

                foreach (SearchResult obj in foundObjects)
                {
                    AddButtonNode(obj.Name, obj.Value);
                }
            }
            else
            {
                RefreshButtons();
            }
        }

        //Event Handlers
        private void OnSearchEnter(string newText)
        {
            if (newText.Length <= 0)
            {
                return;
            }

            Node container = GetNode("ScrollContainer/PanelContainer");

            foreach (object child in container.GetChildren())
            {
                if (!(child is Button))
                {
                    continue;
                }

                ((Button)child).EmitSignal("pressed");
                break;
            }
        }

        private void OnBackButton()
        {
            m_SearchString = "";
            object newObj = m_BackStack.Last();
            m_BackStack.RemoveAt(m_BackStack.Count - 1);

            if (newObj is Dictionary obj)
            {
                m_SelectedObject = obj;
                m_SelectedArray = null;
            }
            else
            {
                m_SelectedArray = newObj as Array;
                m_SelectedObject = null;
            }

            RefreshButtons();
        }

        private void OnButtonPress(object buttonObj)
        {
            if (m_SelectedArray != null)
            {
                m_BackStack.Add(m_SelectedArray);
            }
            else
            {
                m_BackStack.Add(m_SelectedObject);
            }

            if (buttonObj is Dictionary obj)
            {
                m_SelectedObject = obj;
                m_SelectedArray = null;
            }
            else if (buttonObj is Array array)
            {
                m_SelectedArray = array;
                m_SelectedObject = null;
            }

            RefreshButtons();
        }
    }
}