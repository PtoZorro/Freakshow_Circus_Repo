using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class HandsSystem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera cam; //Referencia a la cámara desde cuyo centro se dispara (Raycast desde centro cámara)
    [SerializeField] GameObject handsGun;
    [SerializeField] GameObject handsAxe;
    [SerializeField] GameObject handsFlash;
    [SerializeField] GameObject light;
    [SerializeField] GameObject reticule;
    [SerializeField] Transform shootPoint; //Referencia a la posición del objeto desde donde se dispara (Raycast desde posición concreta)
    [SerializeField] RaycastHit hit; //Referencia a la info de impacto de los disparos (información de impacto Raycast)
    [SerializeField] LayerMask enemyLayer; //Referencia a la Layer que puede impactar el disparo
    [SerializeField] AudioSource weaponSound; //Referencia al AudioSource del arma
    GameObject handsTemp;

    [Header("Weapon Stats")]
    public int gunDamage; //Daño base del arma por bala
    public int axeDamage; 
    public float gunRange; //Alcance de disparo (longitud del Raycast)
    public float axeRange; 
    public float spread; //Dispersión de los disparos
    public float shootingCooldown; //Tiempo de enfriamiento del arma
    public float attackCooldown;
    public float attack2Cooldown;
    public float timeBetweenShoots; //Tiempo real entre disparo y disparo (Impacto e impacto)
    public float reloadTime; //Tiempo que tardas en recargar (suele igualarse a la duración de la animación de recarga)
    public bool allowButtonHold; //Permite disparar por pulsación (false) o manteniendo (true)

    [Header("Bullet Management")]
    public int ammoSize; //Número de balas por cargador
    public int bulletsLeft; //Número de balas dentro del cargador ACTUAL
    [SerializeField] int bulletsShot; //Número de balas YA DISPARADAS dentro del cargador actual
    public TextMeshProUGUI ammoText;

    [Header("State Bools")]
    [SerializeField] bool shooting; //Verdadero cuando ESTAMOS DISPARANDO
    [SerializeField] bool shootingAim;
    [SerializeField] bool attacking;
    [SerializeField] bool changing;
    [SerializeField] bool alreadyAttacked;
    [SerializeField] bool canShoot; //Verdadero cuando PODEMOS DISPARAR
    [SerializeField] bool reloading; //Verdadero cuando ESTAMOS RECARGANDO
    [SerializeField] bool gunOn;
    [SerializeField] bool axeOn;
    [SerializeField] bool flashOn;
    [SerializeField] bool lightOn;
    public bool aiming;
    bool canChange;
    bool hiding;
    bool shield;

    [Header("Feedback & Graphics")]
    Animator gunAnim;
    Animator axeAnim;
    Animator flashAnim;
    Animator camAnim;
    [SerializeField] Animator deathAnimator;
    [SerializeField] GameObject muzzleFlash; //Objeto feedback del fogonazo
    [SerializeField] bool attackIsSounding; //Si es verdadero, el sonido de disparo ya suena, por lo que no hay que repetirlo
    [SerializeField] GameObject hitGraphic;
    [SerializeField] GameObject hitGraphicBlood;

    #endregion

    private void Awake()
    {
        weaponSound = GetComponent<AudioSource>();
        gunAnim = handsGun.GetComponent<Animator>();
        axeAnim = handsAxe.GetComponent<Animator>();
        flashAnim = handsFlash.GetComponent<Animator>();
        camAnim = cam.GetComponent<Animator>();
        attackIsSounding = false;
        bulletsLeft = ammoSize;
        canShoot = true;
        canChange = true;
        aiming = false;
        //muzzleFlash.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();

        gunOn = handsGun.activeSelf;
        axeOn = handsAxe.activeSelf;
        flashOn = handsFlash.activeSelf;
        lightOn = light.activeSelf;

        gunAnim.SetBool("oneLeft", bulletsLeft <= 0 && !aiming);
        gunAnim.SetBool("empty", bulletsLeft <= 0);

        if (GameManager.Instance.playerDead) { PlayerDeath(); }
    }

    void Inputs()
    {
        //Lectura constante del disparo si se reúnen las condiciones
        // Si (podemos dispara + el input de disparo se lee + no estamos recargando + nos quedan balas en el cargador)
        if (canShoot && (shooting || shootingAim) && !reloading && bulletsLeft > 0)
        {
            Shoot();
        }
        else if (canShoot && attacking)
        {
            Attack();
        }

        camAnim.SetBool("aim", aiming);
        reticule.SetActive(!aiming);
    }

    void Shoot()
    {
        canShoot = false; //Estamos en el proceso de disparo, por tanto YA NO PODEMOS DISPARAR hasta que acabe

        gunAnim.SetTrigger("shoot");
        AudioManager.Instance.PlaySFX(0);

        //Al inicio del disparo, si hay dispersión, se genera la randomización de dicha dispersión (cada disparo tiene una dispersión diferente)
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);
        float spreadZ = Random.Range(-spread, spread);
        Vector3 direction = cam.transform.forward + new Vector3(spreadX, spreadY, spreadZ);

        //Raycast del disparo
        //Generar un Raycast: Physics.Raycast(Origen, Dirección, Variable Almacén del impacto, longitud del rayo, a qué layer golpea el rayo)
        //Si no declaramos layer en un Raycast, golpea a todo lo que tenga collider
        if (Physics.Raycast(cam.transform.position, direction, out hit, gunRange, enemyLayer))
        {
            // Obtener el punto de impacto
            Vector3 hitPoint = hit.point;

            //A PARTIR DE AQUÍ SE CODEAN LOS EFECTOS DEL RAYCAST. EN ESTE CASO ES UN DISPARO
            //EN ESTE CASO SE CODEA HACER DAÑO
            if (hit.collider.CompareTag("Enemy"))
            {
                //Hacer daño concreto
                EnemyDamage enemyScript = hit.collider.GetComponent<EnemyDamage>(); //ACCESO DIRECTO AL SCRIPT DEL ENEMIGO HITEADO
                enemyScript.TakeDamage(gunDamage);

                // Generar el sistema de partículas en el punto de impacto
                Instantiate(hitGraphicBlood, hitPoint, transform.rotation);
            }
        }

        if (Physics.Raycast(cam.transform.position, direction, out hit, gunRange))
        {   
            if (!hit.collider.CompareTag("Enemy"))
            {
                // Obtener el punto de impacto
                Vector3 hitPoint = hit.point;

                // Generar el sistema de partículas en el punto de impacto
                Instantiate(hitGraphic, hitPoint, transform.rotation);
            }
        }

        //Instanciar o visualizar los efectos del disparo (hitGraphics)
        muzzleFlash.SetActive(true);

        bulletsLeft--; //Restamos una bala al cargador actual

        if (!IsInvoking(nameof(ResetShoot)) && !canShoot)
        {
            Invoke(nameof(ResetShoot), shootingCooldown);
        }
    }

    void Attack()
    {
        canShoot = false;

        if (!alreadyAttacked) axeAnim.SetTrigger("attack1");
        else axeAnim.SetTrigger("attack2");
        AudioManager.Instance.PlaySFX(1);

        //Al inicio del disparo, si hay dispersión, se genera la randomización de dicha dispersión (cada disparo tiene una dispersión diferente)
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);
        float spreadZ = Random.Range(-spread, spread);
        Vector3 direction = cam.transform.forward + new Vector3(spreadX, spreadY, spreadZ);

        if (Physics.Raycast(cam.transform.position, direction, out hit, axeRange, enemyLayer))
        {
            // Obtener el punto de impacto
            Vector3 hitPoint = hit.point;

            //A PARTIR DE AQUÍ SE CODEAN LOS EFECTOS DEL RAYCAST. EN ESTE CASO ES UN DISPARO
            //EN ESTE CASO SE CODEA HACER DAÑO
            if (hit.collider.CompareTag("Enemy"))
            {
                //Hacer daño concreto
                EnemyDamage enemyScript = hit.collider.GetComponent<EnemyDamage>(); //ACCESO DIRECTO AL SCRIPT DEL ENEMIGO HITEADO
                enemyScript.TakeDamage(axeDamage);

                // Generar el sistema de partículas en el punto de impacto
                Instantiate(hitGraphicBlood, hitPoint, transform.rotation);
            }

        }

        if (Physics.Raycast(cam.transform.position, direction, out hit, axeRange))
        {
            if (!hit.collider.CompareTag("Enemy"))
            {
                // Obtener el punto de impacto
                Vector3 hitPoint = hit.point;

                // Generar el sistema de partículas en el punto de impacto
                Instantiate(hitGraphic, hitPoint, transform.rotation);
            }
        }

        alreadyAttacked = true;

        if (!IsInvoking(nameof(SecondAttack)) && alreadyAttacked)
        {
            Invoke(nameof(SecondAttack), attack2Cooldown);
        }

        if (!IsInvoking(nameof(ResetShoot)) && !canShoot)
        {
            Invoke(nameof(ResetShoot), attackCooldown);
        }
    }

    void ResetShoot()
    {
        muzzleFlash.SetActive(false);
        canShoot = true; //La acción de disparo ha acabado y por tanto (si se reunen las condiciones) podemos volver a disparar
        //muzzleFlash.SetActive(false);
    }

    void SecondAttack()
    {
        alreadyAttacked = false;
    }

    void Reload()
    {
        if (!reloading)
        {
            reloading = true; //Entrar en estado recarga (No se pueden hacer otras acciones con el arma)
            AudioManager.Instance.PlaySFX(2);
            Invoke(nameof(ReloadFinished), reloadTime); //Intentar hacer coincidir el valor de reloadTime con la duración de la anim de recarga.
        }
    }

    void ReloadFinished()
    {
        if (GameManager.Instance.totalAmmo >= ammoSize)
        {
            int bulletsToReload = ammoSize - bulletsLeft;
            bulletsLeft = bulletsLeft + bulletsToReload;
            GameManager.Instance.totalAmmo -= bulletsToReload;
        }
        else
        {
            int bulletsToReload = ammoSize - bulletsLeft;
            bulletsLeft = bulletsLeft + bulletsToReload;
            GameManager.Instance.totalAmmo = 0;
        }

        reloading = false; //Salir del estado de recarga (Se pueden hacer otras cosas con el arma)
        handsFlash.SetActive(false);
        handsFlash.SetActive(light.activeSelf);
        flashAnim.SetTrigger("fast");

        AudioManager.Instance.PlaySFX(7);
    }

    void ResetChange() 
    {
        canChange = true;
        if (handsTemp != null) handsTemp.SetActive(false);
        if (handsTemp == handsGun) handsFlash.SetActive(false);
        handsTemp = null;
        hiding = false;
        changing = false; 
    }

    void PlayerDeath()
    {
        handsAxe.SetActive(false);
        handsGun.SetActive(false);
        handsFlash.SetActive(false);
        light.SetActive(false);
        deathAnimator.SetTrigger("death");
        GetComponent<PlayerInput>().enabled = false;
    }

    #region New Input Methods

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started && gunOn && !allowButtonHold && !hiding && !changing)
        {
            shooting = !aiming;
            shootingAim = aiming;
            gunAnim.SetBool("shooting", true);
            if (!attackIsSounding)
            {
                //weaponSound.Play();
                attackIsSounding = true;
            }
        }
        if (context.canceled)
        {
            shooting = false;
            shootingAim = false;
            gunAnim.SetBool("shooting", false);
            attackIsSounding = false;
            //weaponSound.Stop();
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.started && gunOn && bulletsLeft > 0 && !shooting && !hiding && !changing)
        {
            aiming = true;
            gunAnim.SetBool("aiming", true);
        }
        if (context.canceled)
        {
            aiming = false;
            gunAnim.SetBool("aiming", false);
        }
    }

    public void OnShield(InputAction.CallbackContext context)
    {
        if (context.started && axeOn && !attacking && !hiding && !changing && !shield)
        {
            shield = true;
            GameManager.Instance.isShield = true;
            attacking = false;
            axeAnim.SetTrigger("shield");
            axeAnim.SetBool("isShield", true);
        }
        if (context.canceled)
        {
            shield = false;
            GameManager.Instance.isShield = false;
            axeAnim.SetBool("isShield", false);
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started && gunOn && !hiding && !changing)
        {
            if (bulletsLeft < ammoSize && GameManager.Instance.totalAmmo > 0 && !reloading)
            {
                gunAnim.SetTrigger("reload");
                flashAnim.SetTrigger("hide");
                Reload();
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && axeOn && !allowButtonHold && !hiding && !changing)
        {
            attacking = true;
            shield = false;
            axeAnim.SetBool("attacking", true);
            axeAnim.SetBool("isShield", false);
            if (!attackIsSounding)
            {
                //weaponSound.Play();
                attackIsSounding = true;
            }
        }
        if (context.canceled)
        {
            attacking = false;
            axeAnim.SetBool("attacking", false);
            attackIsSounding = false;
            //weaponSound.Stop();
        }
    }

    public void OnChange(InputAction.CallbackContext context)
    {
        if (context.started && canChange && !reloading && !attacking && !shooting && !shootingAim)
        {
            canChange = false;
            changing = true;
            if (gunOn && !axeOn)
            {
                handsAxe.SetActive(true);
                gunAnim.SetTrigger("hide");
                flashAnim.SetTrigger("hide");
                hiding = true;
                handsTemp = handsGun;
                Invoke(nameof(ResetChange), .5f);
            }
            else if (!gunOn && axeOn)
            {
                handsGun.SetActive(true); handsFlash.SetActive(lightOn ? true : false);
                axeAnim.SetTrigger("hide");
                hiding = true;
                handsTemp = handsAxe;
                Invoke(nameof(ResetChange), .5f);
            }
        }
    }

    public void OnFlash(InputAction.CallbackContext context)
    {
        if (context.started && canChange && !reloading)
        {
            canChange = false;
            if (lightOn)
            {
                flashAnim.SetTrigger("hide");
                hiding = true;
                handsTemp = handsFlash;
                Invoke(nameof(ResetChange), .5f);
                light.SetActive(false);
                AudioManager.Instance.PlaySFX(3);
            }
            else if (!lightOn )
            {
                handsFlash.SetActive(axeOn ? false : true); light.SetActive(true);
                flashAnim.SetTrigger("fast");
                Invoke(nameof(ResetChange), .5f);
                AudioManager.Instance.PlaySFX(3);
            }
        }
    }

    #endregion
}
