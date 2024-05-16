using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonManagement : MonoBehaviour
{
    [SerializeField] GameObject optionsPanel;

    public void ButtonStart()
    {
        GameManager.Instance.LoadScene(1);
        GameManager.Instance.RestoreStats();
        AudioManager.Instance.PlayMusic(1);
    }

    public void ButtonOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    public void ButtonMenu()
    {
        AudioManager.Instance.PlayMusic(0);
        GameManager.Instance.LoadScene(0);
    }

    public void ButtonExit()
    {
        Debug.Log("Exit game is a succes");
        Application.Quit();
    }
}
