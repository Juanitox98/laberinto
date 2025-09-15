using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            rampaAudio = audioSources[2];   
        }
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movimiento * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        // Caso: recolectable
        if (other.gameObject.CompareTag("Recolectable"))
        {
            posicion = other.gameObject.transform.position;
            particulas.position = posicion;
            systemaParticulas.Play();
            other.gameObject.SetActive(false);

            if (recolectarAudio != null)
                recolectarAudio.Play();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Caso: pared
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
}

