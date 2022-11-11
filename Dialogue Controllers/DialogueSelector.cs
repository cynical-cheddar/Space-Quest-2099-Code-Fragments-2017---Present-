using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSelector : MonoBehaviour
{
    public List<GameObject> DaveBorisGeddon;

    public List<GameObject> DaveBoris;
    public List<GameObject> DaveGeddon;
    public List<GameObject> BorisGeddon;

    public List<GameObject> Dave;
    public List<GameObject> Boris;
    public List<GameObject> Geddon;

    bool dave = false;
    bool boris = false;
    bool geddon = false;

    int line = 0;
    void Start()
    {
        if (GameObject.FindObjectOfType<DaveIdentifier>() != null) dave = true;
        if (GameObject.FindObjectOfType<BorisIdentifier>() != null) boris = true;
        if (GameObject.FindObjectOfType<GeddonIdentifier>() != null) geddon = true;

        playLines();
    }
    void clearScreen()
    {
        foreach(Transform line in transform)
        {
            line.gameObject.SetActive(false);
        }

    }
    void destroySelf()
    {
        Destroy(gameObject);
    }
    //Recursive function to play line
    void playLines()
    {
        clearScreen();
        if (dave && boris && geddon)
        {
            DaveBorisGeddon[line].SetActive(true);
            
            float lineLength = DaveBorisGeddon[line].GetComponent<AudioSource>().clip.length;
            DaveBorisGeddon[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < DaveBorisGeddon.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength);
            }

        }
        else if (dave && boris)
        {

            DaveBoris[line].SetActive(true);
            float lineLength = DaveBoris[line].GetComponent<AudioSource>().clip.length;
            DaveBoris[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < DaveBoris.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength + 0.1f);
            }
        }
        else if(dave && geddon)
        {
            DaveGeddon[line].SetActive(true);
            float lineLength = DaveGeddon[line].GetComponent<AudioSource>().clip.length;
            DaveGeddon[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < DaveGeddon.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength + 0.1f);
            }
        }
        else if(boris && geddon)
        {

            BorisGeddon[line].SetActive(true);
            float lineLength = BorisGeddon[line].GetComponent<AudioSource>().clip.length;
            BorisGeddon[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < BorisGeddon.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength);
            }
        }
        else if (dave)
        {
            Dave[line].SetActive(true);
            float lineLength = Dave[line].GetComponent<AudioSource>().clip.length;
            Dave[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < Dave.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength);
            }
        }
        else if (boris)
        {
            Boris[line].SetActive(true);
            float lineLength = Boris[line].GetComponent<AudioSource>().clip.length;
            Boris[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < Boris.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength);
            }
        }
        else if (geddon)
        {
            Geddon[line].SetActive(true);
            float lineLength = Geddon[line].GetComponent<AudioSource>().clip.length;
            Geddon[line].GetComponent<AudioSource>().Play();
            line += 1;
            if (line < Geddon.Count)
            {
                Invoke("playLines", lineLength);
            }
            else
            {
                Invoke("destroySelf", lineLength);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
