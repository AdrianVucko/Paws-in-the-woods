using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaliPas : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip barkMini;
    [HideInInspector]
    public bool GlavniLaj;

    private Animator animator;
    void Start()
    {
        GlavniLaj = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(GlavniLaj)
        {
            StartCoroutine(korutina());
        }
    }

    IEnumerator korutina()
    {
        yield return new WaitForSeconds(1.3f);
        audioSource.Play();
        yield return new WaitForSeconds(1f);
        gameObject.GetComponent<Animator>().SetTrigger("Glas");
        GlavniLaj = false;
    }
}
