using UnityEngine;

public class Hand : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private LG_Shoot shootScript;
    public GameObject sword;
    public GameObject flashLight;
    public GameObject gun;

    private bool canHit;
    private bool canLight;

    void Start()
    {
        // Solución al bug: Asignar la referencia del script de disparo si está vacía
        if (shootScript == null)
        {
            shootScript = GetComponentInChildren<LG_Shoot>();
        }

        DefaultGun(); // Iniciamos con la pistola por defecto
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DefaultGun();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TurnOnLight();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetSword();
        }
    }

    void DefaultGun()
    {
        canHit = false;
        canLight = false;

        sword.SetActive(false);
        flashLight.SetActive(false);
        gun.SetActive(true);

        if (shootScript != null) shootScript.EnableShooting(true);
    }

    void TurnOnLight()
    {
        canHit = false;
        canLight = true;

        flashLight.SetActive(true);
        gun.SetActive(false);
        sword.SetActive(false);

        if (shootScript != null) shootScript.EnableShooting(false);
    }

    void GetSword()
    {
        canHit = true;
        canLight = false;

        sword.SetActive(true);
        gun.SetActive(false);
        flashLight.SetActive(false);

        if (shootScript != null) shootScript.EnableShooting(false);
    }
}