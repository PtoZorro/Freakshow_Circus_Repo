using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIControl : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] RawImage hpBar;
    [SerializeField] float opacity;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject interactPanel;
    public TextMeshProUGUI ammoText;

    [Header("References")]
    [SerializeField] HandsSystem handsSystem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HealthSystem();
        AmmoSystem();
    }

    void AmmoSystem()
    {
        ammoText.text = handsSystem.bulletsLeft.ToString() + " / " + GameManager.Instance.totalAmmo.ToString();
    }

    void HealthSystem()
    {
        opacity = (float)(GameManager.Instance.maxHealth - GameManager.Instance.health) / 100;
        Color alpha = hpBar.color;
        alpha.a = opacity;
        hpBar.color = alpha;
    }
}
