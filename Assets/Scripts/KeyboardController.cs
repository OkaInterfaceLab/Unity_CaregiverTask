using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class KeyboardController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private string defaultText = "Enter file name here";
    public UnityEvent<string> OnFileNameEntered;

    private TouchScreenKeyboard _keyboard;

    private void Start()
    {
        if (OnFileNameEntered == null)
            OnFileNameEntered = new UnityEvent<string>();

        if (string.IsNullOrEmpty(_textMeshProUGUI.text))
        {
            _textMeshProUGUI.text = defaultText;
        }
    }

    public void OpenKeyboard()
    {
        string initialText = _textMeshProUGUI.text == defaultText ? "" : _textMeshProUGUI.text;
        _keyboard = TouchScreenKeyboard.Open(initialText, TouchScreenKeyboardType.Default);
    }

    private void Update()
    {
        if (_keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            string enteredText = _keyboard.text;
            if (!string.IsNullOrEmpty(enteredText))
            {
                _textMeshProUGUI.text = enteredText;
            }
        }


    }
}
