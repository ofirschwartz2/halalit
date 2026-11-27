using System;
using TMPro;
using UnityEngine;

// Player names (#179): the menu asks for a name until Unity has stored one, and the NAME button opens the panel again to change it
public class PlayerNamePanel : MonoBehaviour
{
    // A flag on the device, set only after Unity accepted a name, so a skipped or failed save asks again next time
    private const string NAME_CHOSEN_KEY = "PlayerNameChosen";

    private const string HINT_TEXT = "Pick a name: 3 to 12 letters or digits";
    private const string SAVING_TEXT = "Saving...";
    private const string COULD_NOT_SAVE_TEXT = "Could not save. Check your connection and try again.";

    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private TMP_InputField _nameInput;
    [SerializeField]
    private TMP_Text _messageText;

    private bool _isSaving;

    private void Awake()
    {
        _panel.SetActive(false);
        _nameInput.characterLimit = Constants.PLAYER_NAME_MAX_LENGTH;
    }

    private void Start()
    {
        if (!HasChosenName())
        {
            Open();
        }
    }

    #region Buttons
    public void Open()
    {
        _panel.SetActive(true);
        _messageText.text = HINT_TEXT;
    }

    public void Confirm()
    {
        TrySaveName();
    }

    public void Close()
    {
        if (_isSaving)
        {
            return;
        }

        _panel.SetActive(false);
    }
    #endregion

    private async void TrySaveName()
    {
        if (_isSaving)
        {
            return;
        }

        string playerName = PlayerNameRules.Clean(_nameInput.text);

        if (!PlayerNameRules.IsValid(playerName))
        {
            _messageText.text = HINT_TEXT;
            return;
        }

        _isSaving = true;

        try
        {
            _messageText.text = SAVING_TEXT;
            await Authentication.UpdatePlayerNameAsync(playerName);

            // The flag lives on the device, so it is set even if the menu was closed during the save
            SetNameChosen();

            if (this == null)
            {
                return;
            }

            // Not Close(): it does nothing while _isSaving is set
            _panel.SetActive(false);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"PlayerNamePanel: could not save the name. {exception.Message}");

            if (this == null)
            {
                return;
            }

            _messageText.text = COULD_NOT_SAVE_TEXT;
        }
        finally
        {
            _isSaving = false;
        }
    }

    private static void SetNameChosen()
    {
        PlayerPrefs.SetInt(NAME_CHOSEN_KEY, 1);
        PlayerPrefs.Save();
    }

    #region Predicates
    private static bool HasChosenName()
    {
        return PlayerPrefs.GetInt(NAME_CHOSEN_KEY, 0) == 1;
    }
    #endregion
}
