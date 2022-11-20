using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Westwind.Scripting;
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;

public class ScriptsController : MonoBehaviour
{
    public string FileDirectory = "UserScripts/";

    [Header("Top Bar")]
    public TextMeshProUGUI ScriptName;
    public Button SaveButton;
    public Button TestButton;
    public Color SuccessColor, ErrorColor;

    [Header("Script List")]
    public RectTransform Content;
    public GameObject ElementPrefab;
    public Color ActiveScriptColor;

    [Header("New Script Dialogue")]
    public GameObject NewScriptDialogueWindow;
    public TMP_InputField NewScriptText;
    public Button CreateButton;

    [Header("Editor")]
    public TMP_InputField Editor;
    public GameObject UnsavedChangesDialogueWindow;

    public Dictionary<string, Type> ControlScripts;

    private List<string> _scriptNames;
    private string _openScript = "";
    private bool _scriptChanged = false;
    private Color _defaultElementColor;

    private CSharpScriptExecution _scriptExecution;
    private Coroutine _buttonColorCorouting;
    private Color _defaultTestButtonColor;


    private void Awake()
    {
        _defaultElementColor = ElementPrefab.GetComponent<Image>().color;
        FileDirectory = Path.Join(Application.persistentDataPath, FileDirectory);
        if (!Directory.Exists(FileDirectory))
            Directory.CreateDirectory(FileDirectory);
        _defaultTestButtonColor = TestButton.GetComponent<Image>().color;

        // Set up script execution
        ControlScripts = new Dictionary<string, Type>();
        _scriptExecution = new CSharpScriptExecution() { SaveGeneratedCode = true };
        _scriptExecution.AddNetCoreDefaultReferences();
        _scriptExecution.AddAssembly(typeof(Components.IComponent));
        _scriptExecution.AddAssembly(typeof(object));
        _scriptExecution.AddNamespace("System");
        _scriptExecution.AddNamespace("System.Collections");
        _scriptExecution.AddNamespace("System.Collections.Generic");
        _scriptExecution.AddNamespace("Components");

        NewScriptText.onSubmit.AddListener(_ => CreateNewScript());
    }

    private void OnEnable()
    {
        // Fetch list of scripts
        string[] files = Directory.GetFiles(FileDirectory, "*.cs");

        // Add to list
        List<string> knownNames = _scriptNames != null ? _scriptNames : new List<string>();
        _scriptNames = new List<string>();
        foreach (string file in files)
        {
            if (Path.GetExtension(file).ToLower() == ".cs")
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrEmpty(GetClassName(name)))
                    continue;
                AddScriptToList(name);
                // Check if script already got compiled
                if (!knownNames.Contains(name))
                    CompileAndSaveScript(name);
            }
        }
        // Check if any files got deleted
        foreach (string name in ControlScripts.Keys)
            if (!_scriptNames.Contains(name))
                ControlScripts.Remove(name);

        // Deactive editor if no script open
        if (_openScript == null || _openScript == "")
            Editor.gameObject.SetActive(false);
        else
            OpenScript(_openScript);
    }

    private void OnDisable()
    {
        foreach (Transform child in Content.transform)
            Destroy(child.gameObject);
    }

    // Content List

    private void AddScriptToList(string name)
    {
        _scriptNames.Add(name);
        GameObject go = Instantiate(ElementPrefab, Content.transform);
        go.name = name;
        go.GetComponentInChildren<TextMeshProUGUI>().text = name;
        go.GetComponent<Button>().onClick.AddListener(() => OpenScript(name));
    }

    // Editor

    public void OpenScript(string name)
    {
        // If script open -> close
        if (Editor.gameObject.activeSelf)
            CloseScript();
        if (Editor.gameObject.activeSelf)
            return;
        Content.transform.Find(name).GetComponent<Image>().color = ActiveScriptColor;

        Editor.gameObject.SetActive(true);
        string text = File.ReadAllText(Path.Join(FileDirectory, name + ".cs"));
        Editor.text = text;
        _openScript = name;
        _scriptChanged = false;
        SaveButton.interactable = true;
    }

    public void CloseScript()
    {
        // Check if edited script is saved first
        if (_scriptChanged)
        {
            // Can't open because unsaved changes
            UnsavedChangesDialogueWindow.SetActive(true);
        }
        else
        {
            ForceCloseScript();
        }
    }

    public void ForceCloseScript()
    {
        Content.transform.Find(_openScript).GetComponent<Image>().color = _defaultElementColor;
        _scriptChanged = false;
        SaveButton.interactable = false;
        _openScript = null;
        Editor.gameObject.SetActive(false);
        Editor.text = "";
    }

    public bool CompileAndSaveScript(string name, string code = null)
    {
        bool readFromFile = false;
        if (code == null)
        {
            // Read code from file
            code = File.ReadAllText(Path.Join(FileDirectory, name + ".cs"));
            readFromFile = true;

        }
        string className = GetClassName(name);
        string fullCode = $@"using System;
using System.Collections;
using System.Collections.Generic;
using Components;
public class {className} : ControlScript {{
public {className}() : base(""{name}"")
{{}}
{code}
}}";

        Type generatedType = _scriptExecution.CompileClassToType(fullCode);
        if (_scriptExecution.Error)
        {
            if (!readFromFile)
                Editor.text += $"\n\nError:\n{_scriptExecution.ErrorMessage}";
            return false;
        }
        ControlScripts[name] = generatedType;
        return true;
    }

    public void TestScript()
    {
        bool success = CompileAndSaveScript(_openScript, Editor.text);
        if (success)
            // Make Test button green for 2 sec
            TestButton.GetComponent<Image>().color = SuccessColor;
        else
            // Make Test button red for 2 sec
            TestButton.GetComponent<Image>().color = ErrorColor;
        // Stop running reset coroutine
        if (_buttonColorCorouting != null)
            StopCoroutine(_buttonColorCorouting);
        _buttonColorCorouting = StartCoroutine(ResetButtonColorAfter(2));
    }

    private IEnumerator ResetButtonColorAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        TestButton.GetComponent<Image>().color = _defaultTestButtonColor;
    }

    public void SaveScript()
    {
        File.WriteAllText(Path.Join(FileDirectory, _openScript + ".cs"), Editor.text);
        _scriptChanged = false;
        SaveButton.interactable = false;
    }

    public void ChangeScript()
    {
        _scriptChanged = true;
        SaveButton.interactable = true;
    }

    public Components.ControlScript GetNewScript(string name)
    {
        if (ControlScripts.ContainsKey(name))
            return Activator.CreateInstance(ControlScripts[name]) as Components.ControlScript;
        else if (_scriptNames == null)
        {
            OnEnable();
            if (!this.isActiveAndEnabled)
                OnDisable();
            if (ControlScripts.ContainsKey(name))
                return Activator.CreateInstance(ControlScripts[name]) as Components.ControlScript;
        }
        return null;
    }

    // New Script

    public void OpenNewScriptDialogue()
    {
        NewScriptText.text = "";
        NewScriptDialogueWindow.SetActive(true);
        NewScriptText.Select();
    }

    public void CreateNewScript()
    {
        if (!ValidateNewScriptName())
            return;
        string name = NewScriptText.text;
        NewScriptText.text = "";
        NewScriptDialogueWindow.SetActive(false);
        try
        {
            File.Create(Path.Join(FileDirectory, name + ".cs")).Close();
            File.WriteAllText(Path.Join(FileDirectory, name + ".cs"), @"/// <summary>
/// Start method that is called when the control script first loads.
/// 
/// This should be used to load data from Storage if this isn't the first time the script runs.
/// </summary>
public override void Start() { }

/// <summary>
/// Stop method that is called when the game is closed.
/// 
/// This should be used to save data in Storage that should be kept.
/// </summary>
public override void Stop() { }

/// <summary>
/// Update method that is called continously.
/// </summary>
/// <param name=""deltatime"">Time passed since last run in seconds.</param>
public override void Update(double deltatime) { }");
        }
        catch (Exception e)
        {
            // Abort
            Debug.LogError(e);
            return;
        }
        AddScriptToList(name);
        OpenScript(name);
    }

    public void NewScriptNameChanged()
    {
        CreateButton.interactable = ValidateNewScriptName();
    }

    public bool ValidateNewScriptName()
    {
        string name = NewScriptText.text.Trim();
        return name != "" &&
            !_scriptNames.Contains(name) &&
            !name.Contains(new string(Path.GetInvalidFileNameChars())) &&
            GetClassName(name) != "";
    }

    public static string GetClassName(string name)
    {
        string className = Regex.Replace(name, @"[^A-Za-z0-9_]+", "");
        if (Regex.IsMatch(className, @"^[0-9]+.*")) // Check if starts with numeric
            return "";
        else
            return className;
    }
}
