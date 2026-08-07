using UnityEngine;
using UnityEngine.InputSystem;

public class RIMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float caminar = 4f;
    public float correr = 7f;
    public float agachado = 2f;         //velocidad al ir agachado
    public float aceleracion = 14f;     //que tan rapido alcanza la velocidad final
    public float gravedad = -22f;       //necestamos definir la gravedad ya que no tenemos RigidBody

    [Header("Estamina")]
    public float estaminaMax = 5f;       // segundos que aguanta corriendo
    public float recuperacion = 1.5f;    //tiempo de recuperación de la estamina

    public Transform cabeza;             // objeto vacio donde cuelga la camara

    CharacterController charCotroller;
    InputAction accionMover, accionCorrer, accionAgacharse;
    Vector3 velocidad;                   
    float estamina;
    bool estoyAgachado;

    void Awake()
    {
        charCotroller = GetComponent<CharacterController>();

        //Acciones por el nombre del Input System_Actions
        var acciones = GetComponent<PlayerInput>().actions;
        accionMover = acciones["Move"];
        accionCorrer = acciones["Sprint"];
        accionAgacharse = acciones["Crouch"];
    }

    void Start()
    {
        estamina = estaminaMax;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        bool enSuelo = charCotroller.isGrounded;

        //Direccion que pide el jugador, relativa a hacia donde mira
        Vector2 entrada = accionMover.ReadValue<Vector2>();
        Vector3 direccion = (transform.right * entrada.x + transform.forward * entrada.y).normalized;

        //Agacharse (encoge el collider y baja la cabeza)
        if (accionAgacharse.WasPressedThisFrame()) estoyAgachado = !estoyAgachado;
        charCotroller.height = Mathf.Lerp(charCotroller.height, estoyAgachado ? 1f : 1.8f, Time.deltaTime * 10f);
        charCotroller.center = new Vector3(0f, charCotroller.height / 2f, 0f);
        cabeza.localPosition = Vector3.Lerp(cabeza.localPosition, new Vector3(0f, charCotroller.height - 0.2f, 0f), Time.deltaTime * 10f);

        //Correr: solo hacia adelante, de pie y con estamina
        bool corriendo = accionCorrer.IsPressed() && entrada.y > 0f && !estoyAgachado && estamina > 0f;
        estamina += corriendo ? -Time.deltaTime : recuperacion * Time.deltaTime;
        estamina = Mathf.Clamp(estamina, 0f, estaminaMax);

        float velocidadDeseada = estoyAgachado ? agachado : (corriendo ? correr : caminar);

        //Suavizado para que la velocidad no cambie de golpe y se sienta como acelera
        Vector3 objetivo = direccion * velocidadDeseada;
        velocidad.x = Mathf.Lerp(velocidad.x, objetivo.x, aceleracion * Time.deltaTime);
        velocidad.z = Mathf.Lerp(velocidad.z, objetivo.z, aceleracion * Time.deltaTime);

        //Gravedad 
        if (enSuelo && velocidad.y < 0f) velocidad.y = -2f;   // lo mantiene pegado al piso
        velocidad.y += gravedad * Time.deltaTime;

        //Mover de verdad
        charCotroller.Move(velocidad * Time.deltaTime);
    }

    //Para hacer una barra de estamina en un futuro
    public float EstaminaNormalizada()
    {
        return estamina / estaminaMax;
    }

    //Deja que el jugador empuje objetos sin usar un RigidBody que cree conflictos con el CharacterController
    void OnControllerColliderHit(ControllerColliderHit golpe)
    {
        Rigidbody rb = golpe.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
            rb.AddForce(golpe.moveDirection * 2f, ForceMode.Impulse);
    }
}