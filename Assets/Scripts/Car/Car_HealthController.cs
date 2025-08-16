using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_HealthController : MonoBehaviour, IDamagable
{
    private Car_Controller carController;

    public int maxHealth;
    public int currentHealth;

    private bool carBroken;


    [Header("Infor Explosion")]
    [SerializeField] private int explosionDamage=350;

    [Space]
    [SerializeField] private float explosionRadius = 3;
    [SerializeField] private float explosionDelay = 3;
    [SerializeField] private float explosionForce=7;
    [SerializeField] private float explosionUpwardsModifer=2;
    [SerializeField] private Transform explosionPoint;
    [Space]
    [SerializeField] private ParticleSystem fireFx;
    [SerializeField] private ParticleSystem explosionFX;
     private void Start()
    {
        carController = GetComponent<Car_Controller>();
        currentHealth = maxHealth;
    }
    private void Update()
    {
        if (fireFx.gameObject.activeSelf)
        {
            fireFx.transform.rotation = Quaternion.identity;
        }

    }

    public void UpdateCarHealthUI()
    {
        UI.instance.inGameUI.UpdateCarHealthUI(currentHealth,maxHealth);
    }

    private void ReduceHealth(int damage)
    {
        if (carBroken)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            BrakeTheCar();
    }

    private void BrakeTheCar()
    {
        carBroken = true;
        carController.BrakeTheCar();

        fireFx.gameObject.SetActive(true);
        StartCoroutine(ExplosionCo(explosionDelay));
    }

    public void TakeDamage(int damage)
    {
        ReduceHealth(damage);
        UpdateCarHealthUI();
    }

    private IEnumerator ExplosionCo(float delay)
    {
        yield return new WaitForSeconds(delay);

     
        explosionFX.gameObject.SetActive(true);
        carController.rb.AddExplosionForce(//transform.position - Vector3.down same + vector3.up
            explosionForce, explosionPoint.position, explosionRadius,
            explosionUpwardsModifer, ForceMode.Impulse);
        Explode();
    }

    private void Explode()
    {
        HashSet<GameObject> uniqueEnties = new HashSet<GameObject>();
        Collider[] collider = Physics.OverlapSphere(explosionPoint.position, explosionRadius);
        
        foreach (Collider hit in collider)
        {
            //Debug.Log("hit "+hit.name);
            IDamagable damagable= hit.GetComponent<IDamagable>();
           
            if (damagable != null)
            {
                //Debug.Log("Co chay");
                GameObject rootEntity= hit.transform.root.gameObject;
                if(uniqueEnties.Add(rootEntity)== false) 
                    continue;
                damagable.TakeDamage(explosionDamage);

                hit.GetComponentInChildren<Rigidbody>().
                    AddExplosionForce(explosionForce, explosionPoint.position,explosionRadius, explosionUpwardsModifer, ForceMode.VelocityChange);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(explosionPoint.position, explosionRadius);
    }
}
