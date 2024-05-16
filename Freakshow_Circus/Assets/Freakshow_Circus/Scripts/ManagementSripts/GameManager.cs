using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Inicio declaración Singleton
    private static GameManager instance; //Declaración estática de la base de datos
    public static GameManager Instance //Declaración de la llave que accede a los datos públicos de la base de datos
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("GameManager is null");
            }
            return instance;
        }
    }
    //Fin de la declaración de Singleton

    [Header("Stats")]
    public int health;
    public int maxHealth;
    public int healthLeft;
    public int totalAmmo;
    public int enemies;
    public float recoverTime;
    public float timeDamaged;
    public float timeRecovering;
    public bool playerDead;
    public bool onAnimation;
    public bool onAnimation2;
    public bool menuOn;
    public bool canRecover;
    public bool isShield;
    public bool level1done;
    public bool level2done;

    [Header("SceneManagement")]
    public int actualScene;
    public int maxEnemies;
    public int remainingEnemies;
    public int enemiesToSpawn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        health = maxHealth;
        playerDead = false;
        level1done = false;
        level2done = false;
        menuOn = false;
        canRecover = false;
    }

    private void Update()
    {
        HideCursor();
        HealthControl();
        LifeRecovering();

        if (enemies <= 10 && !level1done)
        {
            LoadScene(2);
            level1done = true;
        }
        if (enemies <= 0 && !level2done)
        {
            AudioManager.Instance.PlayMusic(3);
            LoadScene(4);
            level2done = true;
        }
    }

    public void HealthControl()
    {
        if (health <= 0 && !playerDead)
        {
            health = 0;
            playerDead = true;
            AudioManager.Instance.PlayMusic(2);
            LoadScene(3);
        }
        else if (health >= 96)
        {
            health = 100;
        }
    }

    public void TakeDamage(int damage)
    {
        AudioManager.Instance.PlaySFX(4);
        if (!isShield) health -= damage;
        else health -= Mathf.RoundToInt(damage * 0.6f);
        canRecover = false;
        CancelInvoke(nameof(AllowRecover));
        Invoke(nameof(AllowRecover), timeDamaged); 
    }

    void LifeRecovering()
    {
        if (health > 0 && health < 100 && canRecover)
        {
            timeRecovering += Time.deltaTime;
            health = (int)Mathf.Lerp(health, maxHealth, timeRecovering / recoverTime);
        }
        else { canRecover = false; timeRecovering = 0; }
    }

    void AllowRecover()
    {
        canRecover = true;
    }

    void HideCursor()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;

        if (actualScene == 0 || actualScene == 3 || actualScene == 4) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
        else { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }
    }

    #region SceneManager

    public void RestartLevel()
    {
        actualScene = SceneManager.GetActiveScene().buildIndex;
        string thisScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(thisScene);
    }

    public void RestoreStats()
    {
        health = maxHealth;
        playerDead = false;
        level1done = false;
        level2done = false;
        totalAmmo = 90;
        enemies = 18;
    }
    public void NextScene()
    {
        int sceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(sceneToLoad);

        remainingEnemies = maxEnemies;
        enemiesToSpawn = maxEnemies;
    }

    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void ExitGame()
    {
        Debug.Log("Exit game is a succes");
        Application.Quit();
    }

    #endregion

}
