using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class Hand : MonoBehaviour
{
    LG_Shoot shootScript;
    public GameObject sword;
    public GameObject flashLight;
    public GameObject gun;
    
    private bool canHit;
    private bool canShoot;
    private bool canLight;

    void Start()
    {
        canHit = false;
        canShoot = true;
        canLight = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            DefaultGun();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TurnOnLight();
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetSword();
        }
    }
    void DefaultGun()
    {
        canShoot = true;
        canHit = false;
        canLight = false;
        
        sword.SetActive(false);
        flashLight.SetActive(false);
        gun.SetActive(true);
        
        shootScript.EnableShooting(true);
    }
    void TurnOnLight()
    {
        canLight = true;
        canShoot = false;
        canHit = false;
        
        flashLight.SetActive(true);
        gun.SetActive(false);
        sword.SetActive(false);

        shootScript.EnableShooting(false);
    }

    void GetSword()
    {
        canHit = true;
        canShoot = false;
        canLight = false;
       
        sword.SetActive(true);
        gun.SetActive(false);
        flashLight.SetActive(false);

        shootScript.EnableShooting(false);
    }
}