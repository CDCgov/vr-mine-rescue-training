using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;
using System.Reflection;
using System;

[HasCommandConsoleCommands]
public class CommandConsole : MonoBehaviour
{

    private enum CommandMethodType
    {
        VoidMethod,
        StringMethod,
        BoolMethod,
        IntMethod,
        FloatMethod
    }

    private struct CommandConsoleCommand
    {
        public Type ClassType;
        public MethodInfo Method;
        public CommandMethodType MethodType;
        public string CommandName;
        public CommandConsoleCommandAttribute Attrib;
    }

    public GameObject CommandConsoleObj;
    public TextMeshProUGUI ConsoleText;
    public ScrollRect ScrollRect;
    public InputField ConsoleInput;
    public int MaxLines = 200;

    private LinkedList<string> _consoleLines;
    private StringBuilder _consoleSB;
    private StringBuilder _commandListSB;

    private static CommandConsole _instance;

    private Dictionary<string, CommandConsoleCommand> _commandMap;

    private LinkedList<string> _commandHistory;
    private LinkedListNode<string> _lastCommandHistoryNode;

    private List<string> _autocompleteList;
    private int _autocompleteIndex;

    public static void Print(string fmt, params object[] parameters)
    {
        Print(string.Format(fmt, parameters));
    }

    public static void Print(string message)
    {
        if (_instance == null)
            return;

        _instance.AddString(message);
    }

    public static void ShowCommandConsole(bool bShow)
    {
        if (_instance == null)
            return;

        _instance.CommandConsoleObj.SetActive(bShow);

        if (bShow)
        {
            _instance.ConsoleInput.text = "";
            _instance.ConsoleInput.Select();
            _instance.ConsoleInput.ActivateInputField();
        }

    }

    private void AddString(string message)
    {
        _consoleLines.AddLast(message);

        while (_consoleLines.Count > MaxLines)
        {
            _consoleLines.RemoveFirst();
        }

        UpdateConsoleText();
    }

    private void UpdateConsoleText()
    {
        //retrieve scrollbar position before we change content
        float scrollPos = 0;
        if (ScrollRect != null)
            scrollPos = ScrollRect.verticalScrollbar.value;

        LinkedListNode<string> node = _consoleLines.First;

        _consoleSB.Length = 0;
        while (node != null)
        {
            _consoleSB.AppendLine(node.Value);
            node = node.Next;
        }

        ConsoleText.text = _consoleSB.ToString();

        if (ScrollRect != null)
        {
            //Debug.LogFormat("Console text size  {0}", ConsoleText.preferredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ScrollRect.transform);
            //Debug.LogFormat("Console text size after  {0}", ConsoleText.preferredHeight);

            //move scrollbar to bottom if it was already there
            //if (scrollPos < 0.2f || ScrollRect.verticalScrollbar.size > 0.8f)

            ScrollRect.verticalScrollbar.value = 0;
            ScrollRect.verticalNormalizedPosition = 0;
        }
    }

    private void LoadCommands()
    {
        _commandMap = new Dictionary<string, CommandConsoleCommand>();

        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        foreach (Type type in types)
        {
            var hasCommands = type.GetCustomAttributes(typeof(HasCommandConsoleCommandsAttribute), false);
            if (hasCommands != null && hasCommands.Length >= 1)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    var methodAttribs = method.GetCustomAttributes(typeof(CommandConsoleCommandAttribute), false);
                    if (methodAttribs != null && methodAttribs.Length >= 1)
                    {
                        CommandConsoleCommandAttribute cmdAttrib = (CommandConsoleCommandAttribute)methodAttribs[0];

                        CommandConsoleCommand cmd = new CommandConsoleCommand();

                        var parameters = method.GetParameters();
                        if (parameters == null || parameters.Length <= 0)
                            cmd.MethodType = CommandMethodType.VoidMethod;
                        else
                        {
                            if (parameters.Length != 1)
                                continue; //multiple parameter methods not supported

                            ParameterInfo paramInfo = parameters[0];
                            Type paramType = paramInfo.ParameterType;

                            if (paramType == typeof(string))
                                cmd.MethodType = CommandMethodType.StringMethod;
                            else if (paramType == typeof(bool?))
                                cmd.MethodType = CommandMethodType.BoolMethod;
                            else if (paramType == typeof(int))
                                cmd.MethodType = CommandMethodType.IntMethod;
                            else if (paramType == typeof(float))
                                cmd.MethodType = CommandMethodType.FloatMethod;
                            else
                                continue;
                        }

                        cmd.ClassType = type;
                        cmd.Method = method;
                        cmd.CommandName = cmdAttrib.CommandName.ToLower();
                        cmd.Attrib = cmdAttrib;

                        if (_commandMap.ContainsKey(cmd.CommandName))
                        {
                            Debug.LogErrorFormat("Error: Two console commands defined with the same name: {0}", cmd.CommandName);
                        }
                        else
                        {
                            _commandMap.Add(cmd.CommandName, cmd);
                        }
                    }
                }
            }
        }
    }

    private void ExecuteCommand()
    {
        ExecuteCommand(ConsoleInput.text);
    }

    private void ExecuteCommand(string text)
    {
        int spacePos = text.IndexOf(' ');

        string cmdText;
        string cmdParams;

        if (spacePos > 0)
        {
            cmdText = text.Substring(0, spacePos).Trim().ToLower();
            cmdParams = text.Substring(spacePos + 1);
        }
        else
        {
            cmdText = text.Trim().ToLower();
            cmdParams = "";
        }

        Debug.LogFormat("Executing Command: {0} Parameters: {1}", text, cmdParams);

        CommandConsoleCommand cmd;
        if (_commandMap.TryGetValue(cmdText, out cmd))
        {
            switch (cmd.MethodType)
            {
                case CommandMethodType.VoidMethod:
                    cmd.Method.Invoke(null, null);
                    break;

                case CommandMethodType.StringMethod:
                    cmd.Method.Invoke(null, new object[] { cmdParams });
                    break;

                case CommandMethodType.BoolMethod:
                    bool? bVal = null;
                    cmdParams = cmdParams.ToLower();
                    if (cmdParams.Contains("on") || cmdParams.Contains("enable"))
                        bVal = true;
                    else if (cmdParams.Contains("off") || cmdParams.Contains("disable"))
                        bVal = false;

                    cmd.Method.Invoke(null, new object[] { bVal });
                    break;

                case CommandMethodType.IntMethod:
                    int ival = 0;
                    if (Int32.TryParse(cmdParams, out ival))
                    {
                        cmd.Method.Invoke(null, new object[] { ival });
                    }
                    break;

                case CommandMethodType.FloatMethod:
                    float fval = 0;
                    if (float.TryParse(cmdParams, out fval))
                    {
                        cmd.Method.Invoke(null, new object[] { fval });
                    }
                    break;
            }

        }

        AddToCommandHistory(text);
    }

    private void AddToCommandHistory(string text)
    {
        if (text == null || text.Length <= 0)
            return;

        _commandHistory.AddLast(text);
        _lastCommandHistoryNode = _commandHistory.Last;

        while (_commandHistory.Count > 50)
        {
            _commandHistory.RemoveFirst();
        }
    }

    private void AutocompleteCommand(string text)
    {
        if (_autocompleteList.Count > 0)
        {
            _autocompleteIndex++;
            if (_autocompleteIndex >= _autocompleteList.Count)
                _autocompleteIndex = 0;

            ConsoleInput.text = _autocompleteList[_autocompleteIndex];
            return;
        }

        if (text == null || text.Length <= 0)
            return;

        string cmdStart = text.ToLower().Trim();

        foreach (string cmdName in _commandMap.Keys)
        {
            if (cmdName.StartsWith(cmdStart))
            {
                _autocompleteList.Add(cmdName);
            }
        }

        _autocompleteIndex = 0;
        ConsoleInput.text = _autocompleteList[0];
    }

    private void ShowCommandList(string text)
    {
        if (text == null || text.Length <= 0)
            return;

        string cmdStart = text.ToLower().Trim();
        _commandListSB.Length = 0;

        foreach (KeyValuePair<string, CommandConsoleCommand> kvp in _commandMap)
        {
            if (kvp.Key.StartsWith(cmdStart))
            {
                string methodType = null;
                switch (kvp.Value.MethodType)
                {
                    case CommandMethodType.VoidMethod:
                        methodType = "void";
                        break;

                    case CommandMethodType.StringMethod:
                        methodType = "string";
                        break;

                    case CommandMethodType.IntMethod:
                        methodType = "int";
                        break;

                    case CommandMethodType.FloatMethod:
                        methodType = "float";
                        break;

                    case CommandMethodType.BoolMethod:
                        methodType = "bool";
                        break;

                    default:
                        methodType = "unknown";
                        break;
                }

                _commandListSB.AppendFormat("{0} ({1}) :: {2}\n", kvp.Key, methodType, kvp.Value.Attrib.Description);
            }
        }

        ConsoleText.text = _commandListSB.ToString();
    }

    private void Awake()
    {
        _instance = this;
        _consoleLines = new LinkedList<string>();
        _consoleSB = new StringBuilder();
        _commandListSB = new StringBuilder();
        _commandHistory = new LinkedList<string>();

        _autocompleteIndex = 0;
        _autocompleteList = new List<string>();

        LoadCommands();
    }

    // Use this for initialization
    void Start()
    {

        ConsoleText.text = "";

        ConsoleInput.onValueChanged.AddListener(OnCommandInputChanged);
        ConsoleInput.onEndEdit.AddListener(OnEndEdit);
        ConsoleInput.onValidateInput = OnValidateInput;
    }

    private char OnValidateInput(string text, int charIndex, char addedChar)
    {
        if (addedChar == '\n')
        {
            Debug.Log("Newline");
        }

        //intercept tab for autocomplete
        if (addedChar == '\t')
        {
            addedChar = '\0';
            AutocompleteCommand(text);
        }

        return addedChar;
    }

    private void OnCommandInputChanged(string text)
    {
        if (text != null && text.Length > 0)
            ShowCommandList(text);
        else
            UpdateConsoleText();
    }

    private void OnEndEdit(string text)
    {
        if (!ConsoleInput.wasCanceled)
        {
            ConsoleInput.text = "";
            ExecuteCommand(text);
            ConsoleInput.Select();
            ConsoleInput.ActivateInputField();
        }

        _autocompleteList.Clear();
        _autocompleteIndex = 0;

        UpdateConsoleText();
    }

    void TestMethod()
    {
        AddString(string.Format("Hello world {0:F2}", Time.time));
    }

    [CommandConsoleCommand("test_command", "Example test command")]
    static void TestCommand(string cmdParams)
    {
        _instance.AddString(string.Format("Hello world {0:F2}", Time.time));
    }

    [CommandConsoleCommand("test_int_command", "Test command that takes an int")]
    static void TestIntCommand(int ival)
    {
        Print("Int Command: Got {0}", ival);
    }

    [CommandConsoleCommand("test_float_command", "Test command that takes a float")]
    static void TestFloatCommand(float fval)
    {
        Print("Float Command: Got {0:F2}", fval);
    }

    [CommandConsoleCommand("test_bool_command", "Test command that takes a bool")]
    static void TestBoolCommand(bool? bval)
    {
        if (bval == null)
            Print("Bool Command: Got null");
        else
            Print("Bool Command: Got {0}", bval.ToString());
    }

    [CommandConsoleCommand("list_scenes", "List the scenes included in this build")]
    static void TestListScenes()
    {
        int numScenes = SceneManager.sceneCountInBuildSettings;
        Print($"{numScenes} scenes available in this build");
        for (int i = 0; i < numScenes; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            Print(scenePath);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ShowCommandConsole(!CommandConsoleObj.activeSelf);
        }

        if (ConsoleInput != null && ConsoleInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ConsoleInput.text = _lastCommandHistoryNode.Value;
                if (_lastCommandHistoryNode.Previous != null)
                    _lastCommandHistoryNode = _lastCommandHistoryNode.Previous;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ConsoleInput.text = _lastCommandHistoryNode.Value;
                if (_lastCommandHistoryNode.Next != null)
                    _lastCommandHistoryNode = _lastCommandHistoryNode.Next;
            }
            else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            {
                _autocompleteList.Clear();
                _autocompleteIndex = 0;
            }
        }

        /*
        if (ConsoleInput != null && ConsoleInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                AutocompleteCommand(ConsoleInput.text);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                ExecuteCommand(ConsoleInput.text);
            }
        }
        */
    }
}
