using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Helpers.Serialization;
using VRage.Game;

namespace Common.Monitor
{
    /// <summary>
    /// Класс-обёртка над текстовой панелью
    /// </summary>
    public class MonitorPanel
    {
        private int _lineLen = -1;
        protected IMyTextPanel _textPanel;
        public string Text
        {
            get { return _textPanel.GetText(); }
            set { _textPanel.WriteText(value); }
        }

        public MonitorPanel(IMyTerminalBlock panel)
        {
            _textPanel = panel as IMyTextPanel;
            _textPanel.FontSize = 1.5f;
        }

        public void SetLineLength(int length) => _lineLen = length;

        public void WriteLines(string text)
        {
            var newLineChar = '\n';
            if (_lineLen < 0)
            {
                _textPanel.WriteText(newLineChar + text, append: true);
                return;
            }

            var currLineLen = 0;
            var lineBuilder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                currLineLen++;

                var c = text[i];
                lineBuilder.Append(c);

                if (c == newLineChar)
                {
                    currLineLen = 0;
                    continue;
                }

                if (currLineLen >= _lineLen)
                {
                    currLineLen = 0;
                    lineBuilder.Append(newLineChar);
                }
            }

            _textPanel.WriteText(lineBuilder.ToString(), append: true);
        }
        public void Clear() => _textPanel.WriteText("");
        public void SetFont(float fontSize) => _textPanel.FontSize = fontSize;
    }

    public class PanelVariables : MonitorPanel
    {
        public List<KeyValuePair<string, string>> Variables = new List<KeyValuePair<string, string>>();
        public string Name { get { return _textPanel.CustomName; } }
        public PanelVariables(IMyTerminalBlock panel) : base(panel)
        {
            _textPanel.FontSize = 0.8f;
            if (Text.Length > 0 && Text.Contains("{"))
                Variables = ListSerialiser.Deserialize(Text);
        }
        public string this[string name]
        {
            get
            {
                Variables = ListSerialiser.Deserialize(Text);
                return Variables.Find(x => x.Key == name).Value;
            }
            set
            {
                KeyValuePair<string, string> item = Variables.Find(x => x.Key == name);
                if (item.Key != null)
                {
                    UpdateVariableValue(item, value);
                }
                else
                {
                    item = new KeyValuePair<string, string>(name, value);
                    TryAddVariable(item);
                }
            }
        }

        public void UpdateVariableValue(KeyValuePair<string, string> item, string newValue)
        {
            Variables.Remove(item);
            item = new KeyValuePair<string, string>(item.Key, newValue);
            Variables.Add(item);
            Text = ListSerialiser.Serialize(Variables);
        }

        public void TryAddVariable(string key, string value)
        {
            var item = new KeyValuePair<string, string>(key, value);
            TryAddVariable(item);
        }

        public void TryAddVariable(KeyValuePair<string, string> item)
        {
            if (!Variables.Any(x => x.Key == item.Key))
            {
                Variables.Add(item);
                Text = ListSerialiser.Serialize(Variables);
            }
        }
        public bool IsVariableExist(string name) => Variables.Any(x => x.Key == name);
    }
}