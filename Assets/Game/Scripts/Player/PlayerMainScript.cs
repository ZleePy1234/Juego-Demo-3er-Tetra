using System.Collections;
using System.ComponentModel;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(Rigidbody2D))] //Atributo que asegura que el GameObject tenga un Rigidbody2D, esto nos ayuda a evitar errores. Totalmente opcional.
public class PlayerMainScript : MonoBehaviour
{
    private Rigidbody2D rb; //Referencia al Rigidbody2D del jugador, necesario para moverlo con fisicas.

    [Header("Player Refs")] //Header para organizar las variables en el inspector de Unity.
    [SerializeField] private Transform groundTransform; //Punto de chequeo para saber si el jugador esta en el suelo.
    [Space(20)]

    #region Player Stats
    [Header("Player Stats")]

    /*  ATRIBUTOS || VARIABLES || EXPLICACION

        Este primer set de variables contiene las estadisticas basicas del jugador,
        como vida, velocidad de caminar y fuerza de salto.
        Aqui vemos el uso de atributos como Tooltip, Range y SerializeField para mejorar el editor.

        Tooltip: Muestra una descripcion cuando se pasa el cursor sobre la variable en el inspector.
        Range: Limita el valor de la variable a un rango especifico, mostrando un slider en el inspector.
        SerializeField: Hace que una variable privada sea visible y editable en el inspector de Unity.

        y por ultimo vemos el uso de Space para espaciar visualmente las variables en el inspector.

        Nada de esto es obligatorio, pero facilita mucho el editar variables desde el editor, cuando llega
        a ser necesario.

    */

    [Tooltip("Vida maxima del jugador")]
    [Range(1, 100)]
    [SerializeField] private int vidaMaxima;
    [Space(10)]

    [Tooltip("Vida actual del jugador")]
    [SerializeField]private int vidaActual;
    [Space(10)]

    [Tooltip("Velocidad de caminar del jugador")]
    [Range(1f, 20f)]
    [SerializeField] private float velCaminar;
    [Space(10)]

    [Tooltip("Fuerza de salto del jugador")]
    [Range(1f, 50f)]
    [SerializeField] private float fuerzaSalto;
    [Space(20)]
    #endregion
    
    #region Player Stats 2
    [Header("Player Stats 2")]  

    /*  VARIABLES || EXPLICACION

        Este segundo set de variables contiene estadisticas adicionales del jugador,
        como dash, doble salto, salto en pared y poder de ataque.

        Al igual que antes, se usan atributos como Tooltip, Range, SerializeField y Space.

    */

    [Tooltip("Duracion del dash en segundos")]
    [Range(0.1f, 5f)]
    [SerializeField] private float duracionDash;
    [Space(10)]

    [Tooltip("Velocidad del dash del jugador")]
    [Range(1f, 50f)]
    [SerializeField] private float velDash;
    [Space(10)]

    [Tooltip("Tiempo de recarga del dash en segundos")]
    [Range(0.1f, 10f)]
    [SerializeField] private float tiempoRecargaDash;
    [Space(10)]

    [Tooltip("Altura del doble salto del jugador")]
    [Range(1f, 50f)]
    [SerializeField] private float alturaDobleSalto;
    [Space(10)]

    [Tooltip("Daño del jugador")]
    [Range(1, 50)]
    [SerializeField] private int atkPowerJugador;
    [Space(10)]
    #endregion

    private bool canDash = true; //Variable para controlar si el jugador puede hacer dash
    private bool isDashing = false; // evita que el movimiento normal sobrescriba el dash
    private bool canDoubleJump = true; //Variable para controlar si el jugador puede hacer doble salto
    [SerializeField]private bool isGrounded = true; //Variable para controlar si el jugador esta en el suelo
    private PlayerInput playerInput; //Referencia al componente PlayerInput para manejar las entradas del jugador

    /*  METODOS || UNITY || EXPLICACION

        En esta seccion tenemos los metodos basicos de Unity que se usan comunmente en scripts de MonoBehaviour:

        Awake: Se llama cuando la instancia del script se carga.
        Start: Se llama antes de la primera ejecucion de Update.
        Update: Se llama una vez por frame.
        FixedUpdate: Se llama en intervalos fijos, ideal para fisicas y movimientos.

        Buenas practicas de codigo indican que no debemos poner logica dentro de estos metodos,
        sino que debemos usarlos para llamar a otros metodos que contengan la logica que querramos ejecutar.

        No pasa nada si ponemos logica aqui, pero si lo hacen se van a ver bien tripitropi, lo unico que consideraria aceptable es obtener referencias en awake. 
        Fuera de esa situacion METAN TODO EN FUNCIONES.

    */
    #region Unity Methods
    void Awake() //Awake se llama antes que Start, ideal para inicializar todas las variables posibles.
    {
        rb = GetComponent<Rigidbody2D>(); //Obtenemos la referencia al Rigidbody2D del jugador
        playerInput = GetComponent<PlayerInput>(); //Obtenemos la referencia al PlayerInput del jugador
        vidaActual = vidaMaxima; //Inicializamos la vida actual del jugador con la vida maxima
    }

    void Start() // Start se llama una vez antes de la primera ejecucion de Update
    {
        
    }

    void Update() // Update se llama una vez por frame
    {
        GroundCheck();
        SaltoJugador();
        DashJugador();
        MovimientoJugador();
    }

    void FixedUpdate() // FixedUpdate se llama en intervalos fijos, ideal para fisicas y movimientos
    {
        
    }
    #endregion


    #region Metodos Player
    void MovimientoJugador() // Metodo para manejar el movimiento del jugador
    {
        if(isDashing) return; // Si el jugador esta haciendo dash, no permitimos el movimiento normal
        if (playerInput.actions["Move"].ReadValue<Vector2>().x != 0) //Checamos nuestro playerinput por cualquier cambio en el eje horizontal
        {   
            //Dependiendo de si es mayor o menor a 0, el jugador mira a la derecha o a la izquierda
            //Esto lo hacemos cambiando la escala en X del transform del jugador para voltearlo.
            if(playerInput.actions["Move"].ReadValue<Vector2>().x > 0) 
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z); //Mirar a la derecha
            }
            else
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z); //Mirar a la izquierda
            }

            //Movemos al jugador en el eje X segun la direccion del input y la velocidad de caminar
            float moveInput = playerInput.actions["Move"].ReadValue<Vector2>().x; // Obtenemos el valor del input en el eje X y lo guardamos en un float
            rb.linearVelocity = new Vector2(moveInput * velCaminar, rb.linearVelocity.y); //Asignamos la velocidad al Rigidbody2D del jugador
        }
        else
        {
            //Si no hay input, el jugador no se mueve horizontalmente
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void SaltoJugador() // Metodo para manejar el salto del jugador
    {
        if(playerInput.actions["Jump"].WasPerformedThisFrame()) // Checamos si se presiono el boton de salto este frame y puede saltar
        {
            if(isGrounded) // Si el jugador esta en el suelo
            {
                rb.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse); // Aplicamos una fuerza hacia arriba al Rigidbody2D del jugador
            }
            else if(isGrounded == false && canDoubleJump) // Si el jugador no esta en el suelo y puede hacer doble salto
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reseteamos la velocidad vertical del jugador para que el doble salto sea consistente
                rb.AddForce(new Vector2(0, alturaDobleSalto), ForceMode2D.Impulse); // Aplicamos una fuerza hacia arriba al Rigidbody2D del jugador
                canDoubleJump = false; // Desactivamos la variable de doble salto para que no pueda volver a saltar en el aire
            }
        }
    }

    void GroundCheck() // Metodo para checar si el jugador esta en el suelo
    {
        if(Physics2D.OverlapCircle(groundTransform.position, 0.1f, LayerMask.GetMask("Ground"))) // Checamos si hay colision con el suelo usando un OverlapCircle
        {
            isGrounded = true; // Si hay colision, el jugador esta en el suelo
            canDoubleJump = true; // Reseteamos la variable de doble salto para que pueda volver a saltar
            
        }
        else
        {
            isGrounded = false; // Si no hay colision, el jugador no esta en el suelo
        }
    }

    void DashJugador() // Metodo para manejar el dash del jugador
    {
        if(playerInput.actions["Dash"].WasPerformedThisFrame() && canDash) // Checamos si se presiono el boton de dash este frame y puede hacer dash
        {
            StartCoroutine(DashCoroutine()); // Iniciamos la corrutina de dash
        }
    }

    private IEnumerator DashCoroutine() // Corrutina para manejar el dash del jugador
    {
        canDash = false; // Desactivamos la variable de dash para que no pueda volver a hacer dash
        float originalGravity = rb.gravityScale; // Guardamos la gravedad original del jugador
        rb.gravityScale = 0; // Desactivamos la gravedad del jugador para que no caiga durante el dash

        // Determinamos la direccion del dash segun la direccion en la que mira el jugador
        float dashDirection = transform.localScale.x; // Si el jugador mira a la derecha, dashDirection sera 1, si mira a la izquierda, sera -1
        rb.linearVelocity = new Vector2(dashDirection * velDash, 0); // Asignamos la velocidad de dash al Rigidbody2D del jugador
        isDashing = true; // Activamos la variable de dash para evitar que el movimiento normal sobrescriba el dash

        yield return new WaitForSeconds(duracionDash); // Esperamos la duracion del dash
        isDashing = false; // Desactivamos la variable de dash para que el movimiento normal pueda volver a funcionar

        rb.gravityScale = originalGravity; // Reseteamos la gravedad del jugador
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Reseteamos la velocidad horizontal del jugador

        yield return new WaitForSeconds(tiempoRecargaDash); // Esperamos el tiempo de recarga del dash
        canDash = true; // Activamos la variable de dash para que pueda volver a hacer dash
    }

    #endregion
}
