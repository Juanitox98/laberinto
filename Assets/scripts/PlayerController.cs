using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;  
using TMPro;

public class PlayerController : MonoBehaviour
{
    private Vector3 posicion;
    public float speed;
    private Rigidbody rb;
    public Transform particulas;
    private ParticleSystem systemaParticulas;

    private AudioSource[] audioSources; 
    private AudioSource choqueAudio;      
    private AudioSource recolectarAudio;
    private AudioSource rampaAudio; 

    public TextMeshProUGUI countText;       
    public TextMeshProUGUI timerText;       
    public TextMeshProUGUI gameOverText;    // 👉 Texto de "Ganaste" o "Caíste en la trampa"

    private int count;
    private float timer;
    private bool gameActive;

    public int targetRecolectables;   // se configura en cada escena desde el Inspector
    private Vector3 offsetTexto = new Vector3(0, 1.5f, 0); // altura del texto sobre la esfera

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSources = GetComponents<AudioSource>();
        systemaParticulas = particulas.GetComponent<ParticleSystem>();
        systemaParticulas.Stop();

        if (audioSources.Length >= 2)
        {
            choqueAudio = audioSources[0];       
            recolectarAudio = audioSources[1];
            if (audioSources.Length > 2)
                rampaAudio = audioSources[2];   
        }

        count = 0;
        SetCountText();

        timer = 0f;
        gameActive = true;

        if (gameOverText != null)
            gameOverText.text = ""; // empieza vacío

        ResetRecolectables();
    }

    void Update()
    {
        if (gameActive)
        {
            timer += Time.deltaTime;
            UpdateTimerText();
        }

        // 👉 Mantener el texto siempre encima de la esfera si está activo
        if (gameOverText != null && !string.IsNullOrEmpty(gameOverText.text))
        {
            gameOverText.transform.position = Camera.main.WorldToScreenPoint(
                transform.position + offsetTexto
            );
        }
    }

    void FixedUpdate()
    {
        if (!gameActive) return; 

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movimiento * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Recolectable"))
        {
            posicion = other.gameObject.transform.position;
            particulas.position = posicion;
            systemaParticulas.Play();
            other.gameObject.SetActive(false);

            count++;
            SetCountText();

            if (recolectarAudio != null)
                recolectarAudio.Play();

            // ✅ condición: cuando se juntan todos los recolectables
            if (count >= targetRecolectables)
            {
                Ganaste();
            }
        }

        // ✅ TRAMPA
        if (other.gameObject.CompareTag("Trampa"))
        {
            CaerEnTrampa(other.gameObject);
        }
    }

    void Ganaste()
    {
        gameActive = false;

        // Mensaje sobre la esfera
        if (gameOverText != null)
            gameOverText.text = "¡Ganaste!";

        Debug.Log("Ganaste en " + SceneManager.GetActiveScene().name + " con tiempo: " + FormatTime(timer));

        // si no es el último nivel, carga la siguiente escena
        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            StartCoroutine(LoadNextScene());
        }
    }

    void CaerEnTrampa(GameObject trampa)
    {
        gameActive = false;

        // ✅ Cambiar color de la trampa a negro inmediatamente
        Renderer rend = trampa.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.black;
        }

        // Detener la esfera
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Mensaje sobre la esfera
        if (gameOverText != null)
            gameOverText.text = "¡Caíste en la trampa!";

        Debug.Log("Caíste en la trampa en " + SceneManager.GetActiveScene().name);

        // Espera antes de reiniciar
        StartCoroutine(RestartScene());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pared"))
        {
            if (choqueAudio != null)
                choqueAudio.Play();
        }

        if (collision.gameObject.CompareTag("Rampa"))
        {
            StartCoroutine(DesactivarRampa(collision.gameObject));
        }
    }

    IEnumerator DesactivarRampa(GameObject rampa)
    {
        yield return new WaitForSeconds(1f);
        if (rampaAudio != null)
            rampaAudio.Play();

        rampa.SetActive(false);
    }

    void SetCountText() 
    {
        countText.text = "Recolectables: " + count.ToString();
    }

    void UpdateTimerText()
    {
        timerText.text = "Tiempo: " + FormatTime(timer);
    }

    string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60F);
        int seconds = Mathf.FloorToInt(t % 60F);
        int milliseconds = Mathf.FloorToInt((t * 100F) % 100F);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    void ResetRecolectables()
    {
        GameObject[] recolectables = GameObject.FindGameObjectsWithTag("Recolectable");
        foreach (GameObject r in recolectables)
        {
            r.SetActive(true);
        }
        count = 0;
        timer = 0f;
        gameActive = true;
        SetCountText();
        UpdateTimerText();
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(2f); 
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1); 
    }

    IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(2f); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }
}

