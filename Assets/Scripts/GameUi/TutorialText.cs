using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI tutorialText;

    Dictionary<string, string> TutorialTextByInputDevice = new Dictionary<string, string>();

    string MouseKeyboardControls =
        "A + D or ARROW KEYS = move\r\n" +
        "SPACE or Z = jump\r\n" +
        "LEFT CLICK or X = punch\r\n" +
        "S or DOWN ARROW = crouch\r\n" +
        "RIGHT CLICK or C = shoot axe\r\n" +
        "SHIFT = dash";

    string XboxControls =
        "LEFT STICK/D-PAD = move\r\n" +
        "A = jump\r\n" +
        "X = punch\r\n" +
        "B = shoot axe\r\n" +
        "LEFT STICK/D-PAD DOWN = crouch\r\n" +
        "RIGHT SHOULDER = Dash";

    string PlaystationControls =
        "LEFT STICK/D-PAD = move\r\n" +
        "X = jump\r\n" +
        "SQUARE = punch\r\n" +
        "CIRCLE = shoot axe\r\n" +
        "LEFT STICK/D-PAD DOWN = crouch\r\n";


    private void Awake()
    {
        TutorialTextByInputDevice.Add("Keyboard&Mouse", MouseKeyboardControls);
        TutorialTextByInputDevice.Add("Gamepad", XboxControls);
    }

    private void Update()
    {
        //Debug.Log(InputManager.Instance.PlayerInput.currentControlScheme);
        tutorialText.text = TutorialTextByInputDevice[InputManager.Instance.PlayerInput.currentControlScheme];
    }
}
