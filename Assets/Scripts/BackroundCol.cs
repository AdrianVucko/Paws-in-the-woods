using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackroundCol : MonoBehaviour
{
    public Color color1 = Color.magenta;
    public Color color2 = Color.red;
    public float duration = 3.0F;
    public Camera cam;
    public Dropdown m_Dropdown;
    public Text m_Text;
    public Button btn;
    public Button exitBtn;
    public Text prikazKontrola;
    public Text opisKontrola;

    private readonly Dictionary<string, int> dificultyDatabase = new Dictionary<string, int>()
{
    { "Easy", 3000 },
    { "Normal", 240 },
    { "Hard", 180 },
    {"Very Hard",120 },
    {"Impossible",60 }
};

    void Start()
    {
        Cursor.visible = true;
        m_Dropdown.options.Clear();
        m_Dropdown.AddOptions(new List<string>(dificultyDatabase.Keys));
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
        m_Text.text = "This difficulty has no time limit, it's main purpose is exploring.\nYou have to find 1 pup.";
        btn.onClick.AddListener(stisak);
        exitBtn.onClick.AddListener(doExitGame);
        prikazKontrola.text = "\tw, \u2191\n\ts, \u2193\n\ta, \u2190\n\td, \u2192\n\n\tLeft Shift\n\tLeft Click";
        opisKontrola.text = "Foward\nBackward\nLeft\nRight\n\nSprint\nCall children";
        StateNameController.difficulty = "Easy";
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time, duration) / duration;
        cam.backgroundColor = Color.Lerp(color1, color2, t);
    }

    void DropdownValueChanged(Dropdown change)
    {
        int vrijednost= dificultyDatabase[m_Dropdown.options[change.value].text];
        if(m_Dropdown.options[change.value].text == "Easy")
        {
            m_Text.text = "This difficulty has no time limit, it's main purpose is exploring.\nYou have to find 1 pup.";
        }
        else
        {
            m_Text.text = "Time limit for this difficulty is " + vrijednost + " seconds and you have to find " + (change.value+1) + " pups.";
        }
    }

    void stisak()
    {
        SceneManager.LoadScene("NormalnaSuma");
        StateNameController.difficulty = m_Dropdown.options[m_Dropdown.value].text;
    }

    void doExitGame()
    {
        Application.Quit();
    }
}
