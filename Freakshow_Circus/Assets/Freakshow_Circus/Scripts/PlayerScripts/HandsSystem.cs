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
    [SerializeField] Camera fpsCam; //Referencia a la cámara desde cuyo centro se dispara (Raycast desde centro cámara)
    [SerializeField] GameObject handsGun;
    [SerializeField] GameObject handsAxe;
    [SerializeField] GameObject handsFlash;
    [SerializeField] GameObject light;
    [SerializeField] Transform shootPoint; //Referencia a la posición del objeto desde donde se dispara (Raycast desde posición concreta)
    [SerializeField] RaycastHit hit; //Referencia a la info de impacto de los disparos (información de impacto Raycast)
    [SerializeField] LayerMask enemyLayer; //Referencia a la Layer que puede impactar el disparo
    [SerializeField] AudioSource weaponSound; //Referencia al AudioSource del arma
    GameObject handsTemp;

    [Header("Weapon Stats")]
    public int damage; //Daño base del arma por bala
    public float range; //Alcance de disparo (longitud del Raycast)
    public float spread; //Dispersión de los disparos
    public float shootingCooldown; //Tiempo de enfriamiento del arma
    public float attackCooldown;
    public float attack2Cooldown;
    public float timeBetweenShoots; //Tiempo real entre disparo y disparo (Impacto e impacto)
    public float reloadTime; //Tiempo que tardas en recargar (suele igualarse a la duración de la animación de recarga)
    public bool allowButtonHold; //Permite disparar por pulsación (false) o manteniendo (true)

    [Header("Bullet Management")]
    public int ammoSize; //Número de balas por cargador
    [SerializeField] int bulletsLeft; //Número de balas dentro del cargador ACTUAL
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
    [SerializeField] bool aiming;
    bool canChange;
    bool hiding;

    [Header("Feedback & Graphics")]
    Animator gunAnim;
    Animator axeAnim;
    Animator flashAnim;
    [SerializeField] GameObject muzzleFlash; //Objeto feedback del fogonazo
    [SerializeField] bool attackIsSounding; //Si es verdadero, el sonido de disparo ya suena, por lo que no hay que repetirlo
    [SerializeField] GameObject hitGraphic;

    #endregion

    private void Awake()
    {
        weaponSound = GetComponent<AudioSource>();
        gunAnim = handsGun.GetComponent<Animator>();
        axeAnim = handsAxe.GetComponent<Animator>();
        flashAnim = handsFlash.GetComponent<Animator>();
        attackIsSounding = false;
        bulletsLeft = ammoSize;
        canShoot = true;
        canChange = true;
        aiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();

        gunOn = handsGun.activeSelf;
        axeOn = handsAxe.activeSelf;
        flashOn = handsFlash.activeSelf;
        lightOn = light.activeSelf;

        //if (bulletsLeft <= 0) {gunAnim.SetBool("empty", true); }
        //else { gunAnim.SetBool("empty", false); }

        gunAnim.SetBool("oneLeft", bulletsLeft <= 0 && !aiming);
        gunAnim.SetBool("empty", bulletsLeft <= 0);
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
    }

    void Shoot()
    {
        canShoot = false; //Estamos en el proceso de disparo, por tanto YA NO PODEMOS DISPARAR hasta que acabe

        Debug.Log("shoot");

        gunAnim.SetTrigger("shoot");

        //Al inicio del disparo, si hay dispersión, se genera la randomización de dicha dispersión (cada disparo tiene una dispersión diferente)
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);
        float spreadZ = Random.Range(-spread, spread);
        Vector3 direction = fpsCam.transform.forward + new Vector3(spreadX, spreadY, spreadZ);

        //Raycast del disparo
        //Generar un Raycast: Physics.Raycast(Origen, Dirección, Variable Almacén del impacto, longitud del rayo, a qué layer golpea el rayo)
        //Si no declaramos layer en un Raycast, golpea a todo lo que tenga collider
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range, enemyLayer))
        {
            Debug.DrawRay(fpsCam.transform.position, direction, Color.red);
            Debug.Log(hit.collider.name);

            //A PARTIR DE AQUÍ SE CODEAN LOS EFECTOS DEL RAYCAST. EN ESTE CASO ES UN DISPARO
            //EN ESTE CASO SE CODEA HACER DAÑO
            if (hit.collider.CompareTag("Enemy"))
            {
                //Hacer daño concreto
                //EnemyDamage enemyScript = hit.collider.GetComponent<EnemyDamage>(); //ACCESO DIRECTO AL SCRIPT DEL ENEMIGO HITEADO
                //enemyScript.TakeDamage(damage);
            }

        }

        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range))
        {
            // Obtener el punto de impacto
            Vector3 hitPoint = hit.point;

            // Generar el sistema de partículas en el punto de impacto
            //Instantiate(hitGraphic, hitPoint, Quaternion.identity);
        }

        //Instanciar o visualizar los efectos del disparo (hitGraphics)
        //muzzleFlash.SetActive(true);

        bulletsLeft--; //Restamos una bala al cargador actual

        if (!IsInvoking(nameof(ResetShoot)) && !canShoot)
        {
            Invoke(nameof(ResetShoot), shootingCooldown);
        }
    }

    void Attack()
    {
        canShoot = false;

        Debug.Log("attack");

        if (!alreadyAttacked) axeAnim.SetTrigger("attack1");
        else axeAnim.SetTrigger("attack2");

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
            Invoke(nameof(ReloadFinished), reloadTime); //Intentar hacer coincidir el valor de reloadTime con la duración de la anim de recarga.
        }
    }

    void ReloadFinished()
    {
        bulletsLeft = ammoSize; //Balas actuales pasan a ser el máximo por cargador actual
        reloading = false; //Salir del estado de recarga (Se pueden hacer otras cosas con el arma)
        handsFlash.SetActive(false);
        handsFlash.SetActive(true);
        flashAnim.SetTrigger("fast");
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
        if (context.started && gunOn && bulletsLeft > 0 && !shooting && !hiding && !reloading && !changing)
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

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started && gunOn && !hiding && !changing)
        {
            if (bulletsLeft < ammoSize && !reloading)
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
            axeAnim.SetBool("attacking", true);
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
                Invoke(nameof(ResetChange), 1);
            }
            else if (!gunOn && axeOn)
            {
                handsGun.SetActive(true); handsFlash.SetActive(lightOn ? true : false);
                axeAnim.SetTrigger("hide");
                hiding = true;
                handsTemp = handsAxe;
                Invoke(nameof(ResetChange), 1);
            }
        }
    }

    public void OnFlash(InputAction.CallbackContext context)
    {
        if (context.started && canChange && !reloading)
        {
            if (lightOn)
            {
                flashAnim.SetTrigger("hide");
                hiding = true;
                handsTemp = handsFlash;
                Invoke(nameof(ResetChange), .5f);
                light.SetActive(false);
            }
            else if (!lightOn )
            {
                handsFlash.SetActive(axeOn ? false : true); light.SetActive(true);
                flashAnim.SetTrigger("fast");
            }
        }
    }

    #endregion
}
