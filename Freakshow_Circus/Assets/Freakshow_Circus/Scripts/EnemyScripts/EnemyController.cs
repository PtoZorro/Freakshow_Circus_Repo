using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("AI Configuration")]
    NavMeshAgent agent; //Ref al componente Agente, que permite que el objeto tenga IA
    Transform target; //Ref al transform del objeto que la IA va a perseguir
    [SerializeField] LayerMask targetLayer; //Determina cual es la capa de detección del target
    [SerializeField] LayerMask groundLayer; //Determina cual es la capa de detección del suelo

    [Header("Patroling Stats")]
    public Vector3 walkPoint; //Dirección a la que la IA se va a mover si no detecta target
    [SerializeField] float walkPointRange; //Rango máximo de dirección de movimiento si la IA no detecta target
    bool walkPointSet; //Bool que determina si la IA ha llegado al objetivo y entonces cambia de objetivo

    [Header("Attack Configuration")]
    public float timeBetweenAttacks; //Tiempo de espera entre ataque y ataque
    public float damage1Delay;
    public float damage2Delay;
    bool alreadyAttacked; //Bool para determinar si se ha atacado
    [SerializeField] bool has2Attacks;

    [Header("States & Detection")]
    [SerializeField] float sightRange; //Rango de detección de persecución de la IA
    [SerializeField] float attackRange; //Rango a partir del cual la IA ataca
    [SerializeField] int damageToPlayer;
    [SerializeField] bool targetInSightRange; //Bool que determina si el target está a distancia de detección
    [SerializeField] bool targetInAttackRange; //Bool que determina si el target está a distancia de ataque
    [SerializeField] bool isIddle;
    [SerializeField] bool isWalking;
    [SerializeField] bool isAttacking;

    [Header("Model")]
    [SerializeField] GameObject model;
    Animator anim;

    private void Awake()
    {
        anim = model.GetComponent<Animator>();
        target = GameObject.Find("Player").transform; //Al inicio referencia el transform del Player, para poder perseguirlo cuando toca
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Chequear si el target está en los rangos de detección y de ataque
        targetInSightRange = Physics.CheckSphere(transform.position, sightRange, targetLayer);
        targetInAttackRange = Physics.CheckSphere(transform.position, attackRange, targetLayer);

        //Cambios dinámicos de estado de la IA
        //Si no detecta el target ni está en rango de ataque: PATRULLA
        if (!targetInSightRange && !targetInAttackRange) Patroling();
        //Si detecta el target pero no está en rango de ataque: PERSIGUE
        if (targetInSightRange && !targetInAttackRange && !alreadyAttacked) ChaseTarget();
        //Si detecta el target y está en rango de ataque: ATACA
        if (targetInSightRange && targetInAttackRange) AttackTarget();

    }

    void Patroling()
    {
        if (!isWalking) { anim.SetBool("iddle", false); anim.SetTrigger("walk"); Debug.Log("walk"); }
        isIddle = false; isWalking = true; isAttacking = false;

        if (!walkPointSet)
        {
            //Si no existe punto al que dirigirse, inicia el método de crearlo
            SearchWalkPoint();
        }
        else
        {
            //Si existe punto, el personaje mueve la IA hacia ese punto
            agent.SetDestination(walkPoint);
        }

        //Sistema para que la IA busque un nuevo destino de patrullaje una vez ha llegado al destino actual
        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1) { walkPointSet = false; }
    }

    void SearchWalkPoint()
    {
        //Crear el sistema de puntos "random" a patrullar

        //Sistema de creación de puntos Random
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        //Dirección a la que se mueve la IA
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        //Detección del suelo por debajo del personaje, para evitar caídas
        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
        {
            walkPointSet = true; //Comienza el movimiento, porque existe SUELO en el DESTINO
        }
    }

    void ChaseTarget()
    {

        agent.SetDestination(target.position);

        if (!isWalking) { if (!isWalking) anim.SetBool("iddle", false); anim.SetTrigger("walk"); }
        isIddle = false; isWalking = true; isAttacking = false;
    }

    void AttackTarget()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(target);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        if (!alreadyAttacked)
        {
            if (!isAttacking) anim.SetBool("iddle", false); anim.SetTrigger("attack"); 
            isIddle = false; isWalking = false; isAttacking = true;

            AudioManager.Instance.PlaySFX(5);

            StartCoroutine(DamageDone());

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    IEnumerator DamageDone()
    {
        yield return new WaitForSeconds(damage1Delay);

        if (targetInAttackRange) GameManager.Instance.TakeDamage(damageToPlayer);

        if (has2Attacks)
        {
            yield return new WaitForSeconds(damage2Delay);

            if (targetInAttackRange) GameManager.Instance.TakeDamage(damageToPlayer);
        }
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
