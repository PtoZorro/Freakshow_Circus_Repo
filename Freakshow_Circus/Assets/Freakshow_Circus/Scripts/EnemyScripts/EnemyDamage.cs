using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Configuration")]
    [SerializeField] float health;
    [SerializeField] float maxHealth;
    [SerializeField] GameObject model;
    Animator anim;
    bool dead;

    // Start is called before the first frame update
    void Start()
    {
        anim = model.GetComponent<Animator>();
        health = maxHealth;
        dead = false;
    }

    // Update is called once per frame
    void Update()
    {
        HealthManagement();
    }

    void HealthManagement()
    {
        if (health <= 0 & !dead)
        {
            dead = true;
            anim.SetBool("dead", true);
            anim.SetTrigger("death");

            GetComponent<EnemyController>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;

            GameManager.Instance.enemies--;

            AudioManager.Instance.PlaySFX(6);
        }
    }

    public void TakeDamage(int damageToTake)
    {
        health -= damageToTake;
    }
}
