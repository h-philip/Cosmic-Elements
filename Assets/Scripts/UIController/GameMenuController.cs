using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour
{
    // Public
    public Color SuccessfulSaveColor;
    public RectTransform SettingsMenu;

    // Private
    private SpaceSimulation _spaceSimulation;
    private Coroutine _buttonColorCorouting;
    private Color _defaultSaveButtonColor;
    private Button _saveButton;

    // Unity methods
    private void Awake()
    {
        _spaceSimulation = FindObjectOfType<SpaceSimulation>();
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.name.Contains("Save"))
            {
                _saveButton = button;
                _defaultSaveButtonColor = button.GetComponent<Image>().color;
                break;
            }
        }
    }

    private void OnEnable()
    {
        _spaceSimulation.Paused = true;
    }

    private void OnDisable()
    {
        _spaceSimulation.Paused = false;
    }

    // Resume Game

    public void ResumeGame()
    {
        gameObject.SetActive(false);
    }

    // Save Game
    
    public void SaveGame()
    {
        FindObjectOfType<SessionManager>().SaveGame();
        _saveButton.GetComponent<Image>().color = SuccessfulSaveColor;
        if (_buttonColorCorouting != null)
        {
            StopCoroutine(_buttonColorCorouting);
            _buttonColorCorouting = null;
        }
        _buttonColorCorouting = StartCoroutine(ResetButtonColorAfter(1));
    }

    private IEnumerator ResetButtonColorAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _saveButton.GetComponent<Image>().color = _defaultSaveButtonColor;
    }

    // Load Game

    public void LoadGame()
    {
#if UNITY_EDITOR
        SessionManager.SessionName = "Editor";
#else
        // TODO
#endif
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Settings

    public void Settings()
    {
        SettingsMenu.gameObject.SetActive(!SettingsMenu.gameObject.activeSelf);
    }

    // Quit Game

    public void QuitGame()
    {
        FindObjectOfType<Settings>(true).Save();
        SceneManager.LoadSceneAsync("Main Menu").allowSceneActivation = true;
    }
}
