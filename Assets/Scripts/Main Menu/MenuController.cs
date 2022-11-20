using System.IO;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Start New Game")]
    public RectTransform StartNewGameMenu;
    public TMP_InputField StartNewGameName;
    public Button StartNewGameButton;
    private bool _startNewGame;
    private VerticalSplitAnimation _startNewGameAnimation;

    [Header("Load Game")]
    public RectTransform LoadGameMenu;
    public RectTransform LoadGameContent;
    public RectTransform SessionPrefab;
    private bool _loadGame;
    private VerticalSplitAnimation _loadGameAnimation;

    [Header("Settings")]
    public RectTransform SettingsMenu;
    private bool _settings;
    private VerticalSplitAnimation _settingsAnimation;

    // Private
    private SessionManager _sessionManager;
    private SessionManager.SessionInfo[] _loadableSessions;

    // Unity methods

    private void Start()
    {
        _sessionManager = FindObjectOfType<SessionManager>();
        _startNewGame = StartNewGameMenu.gameObject.activeSelf;
        _startNewGameAnimation = StartNewGameMenu.GetComponentInChildren<VerticalSplitAnimation>();
        _loadGame = LoadGameMenu.gameObject.activeSelf;
        _loadGameAnimation = LoadGameMenu.GetComponentInChildren<VerticalSplitAnimation>();
        _settings = SettingsMenu.gameObject.activeSelf;
        _settingsAnimation = SettingsMenu.GetComponentInChildren<VerticalSplitAnimation>();
        if (SceneManager.GetSceneByName("Ingame").isLoaded)
            SceneManager.UnloadSceneAsync("Ingame");

        if (_loadableSessions == null)
            RefreshLoadableGames();
    }

    // Start New Game

    public void ToggleStartNewGameMenu()
    {
        _startNewGame = !_startNewGame;

        if (_startNewGame) // Opening menu
        {
            _loadGame = false;
            _settings = false;
            if (LoadGameMenu.gameObject.activeSelf) // Load menu was open
            {
                _loadGameAnimation.Hide(_ =>
                {
                    LoadGameMenu.gameObject.SetActive(false);
                    StartNewGameMenu.gameObject.SetActive(true);
                    _startNewGameAnimation.Show(null);
                });
            }
            else if (SettingsMenu.gameObject.activeSelf) // Settings menu was open
            {
                _settingsAnimation.Hide(_ =>
                {
                    SettingsMenu.gameObject.SetActive(false);
                    StartNewGameMenu.gameObject.SetActive(true);
                    _startNewGameAnimation.Show(null);
                });
            }
            else // Nothing was open
            {
                StartNewGameMenu.gameObject.SetActive(true);
                _startNewGameAnimation.Show(null);
            }
        }
        else // Closing menu
        {
            StartNewGameMenu.gameObject.SetActive(true);
            _startNewGameAnimation.Hide(_ =>
            {
                StartNewGameMenu.gameObject.SetActive(false);
            });
        }
    }

    public void StartNewGame()
    {
        SessionManager.SessionName = StartNewGameName.text.Trim();
        StartNewGameName.SetTextWithoutNotify("");
        SceneManager.LoadSceneAsync("Ingame").allowSceneActivation = true;
    }

    public void VerifyNewGameName()
    {
        string text = StartNewGameName.text.Trim();
        bool valid = !string.IsNullOrEmpty(text)
            && text.IndexOfAny(Path.GetInvalidPathChars()) == -1
            && Array.FindIndex(_loadableSessions, _ => _.Name == text) == -1;
        StartNewGameButton.interactable = valid;
    }

    // Load Game

    private void RefreshLoadableGames()
    {
        _loadableSessions = _sessionManager.GetLoadableSessions();

        foreach (Transform child in LoadGameContent)
            Destroy(child.gameObject);

        foreach (var session in _loadableSessions)
        {
            RectTransform element = Instantiate(SessionPrefab, LoadGameContent);
            element.name = session.Name;
            element.Find("Name").GetComponent<TextMeshProUGUI>().text = session.Name;
            TextMeshProUGUI energy = element.Find("Energy").GetComponent<TextMeshProUGUI>();
            energy.text = string.Format(energy.text, TopBarController.FormatNumber(session.Energy));
            TextMeshProUGUI stars = element.Find("Stars").GetComponent<TextMeshProUGUI>();
            stars.text = string.Format(stars.text, TopBarController.FormatNumber(session.DiscoveredStars));
            TextMeshProUGUI stations = element.Find("Stations").GetComponent<TextMeshProUGUI>();
            stations.text = string.Format(stations.text, TopBarController.FormatNumber(session.ControlStations));
            TextMeshProUGUI spaceships = element.Find("Spaceships").GetComponent<TextMeshProUGUI>();
            spaceships.text = string.Format(spaceships.text, TopBarController.FormatNumber(session.Spaceships));
            element.GetComponent<Button>().onClick.AddListener(() => LoadGame(session.Name));
        }
    }

    public void ToggleLoadGameMenu()
    {
        _loadGame = !_loadGame;

        if (_loadGame) // Opening menu
        {
            _startNewGame = false;
            _settings = false;
            if (StartNewGameMenu.gameObject.activeSelf) // Start menu was open
            {
                _startNewGameAnimation.Hide(_ =>
                {
                    StartNewGameMenu.gameObject.SetActive(false);
                    LoadGameMenu.gameObject.SetActive(true);
                    _loadGameAnimation.Show(null);
                });
            }
            else if (SettingsMenu.gameObject.activeSelf) // Settings menu was open
            {
                _settingsAnimation.Hide(_ =>
                {
                    SettingsMenu.gameObject.SetActive(false);
                    LoadGameMenu.gameObject.SetActive(true);
                    _loadGameAnimation.Show(null);
                });
            }
            else // Nothing was open
            {
                LoadGameMenu.gameObject.SetActive(true);
                _loadGameAnimation.Show(null);
            }
        }
        else // Closing menu
        {
            LoadGameMenu.gameObject.SetActive(true);
            _loadGameAnimation.Hide(_ =>
            {
                LoadGameMenu.gameObject.SetActive(false);
            });
        }
    }

    public void LoadGame(string name)
    {
        SessionManager.SessionName = name;
        SceneManager.LoadSceneAsync("Ingame").allowSceneActivation = true;
    }

    // Settings

    public void ToggleSettingsMenu()
    {
        _settings = !_settings;

        if (_settings) // Opening menu
        {
            _startNewGame = false;
            _loadGame = false;
            if (StartNewGameMenu.gameObject.activeSelf) // Start menu was open
            {
                _startNewGameAnimation.Hide(_ =>
                {
                    StartNewGameMenu.gameObject.SetActive(false);
                    SettingsMenu.gameObject.SetActive(true);
                    _settingsAnimation.Show(null);
                });
            }
            else if (LoadGameMenu.gameObject.activeSelf) // Load menu was open
            {
                _loadGameAnimation.Hide(_ =>
                {
                    LoadGameMenu.gameObject.SetActive(false);
                    SettingsMenu.gameObject.SetActive(true);
                    _settingsAnimation.Show(null);
                });
            }
            else // Nothing was open
            {
                SettingsMenu.gameObject.SetActive(true);
                _settingsAnimation.Show(null);
            }
        }
        else // Closing menu
        {
            SettingsMenu.gameObject.SetActive(true);
            _settingsAnimation.Hide(_ =>
            {
                SettingsMenu.gameObject.SetActive(false);
            });
        }
    }

    // Quit Game

    public void QuitGame()
    {
        FindObjectOfType<Settings>().Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
