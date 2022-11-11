using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicShouting : MonoBehaviour {
    public GameObject speechIndicator;
    public AudioSource quoteShouter;
    public float volume = 1f;
    public float chanceIncreasePerKill = 0.1f;
    public float baseChanceNormal = 0.2f;
    public float delay = 0.35f;
    public string damageTypeNormalName = "Normal";
    public string damageTypeExplosiveName = "Explosive";
    public string damageTypeShotgunName = "Shotgun";
    public string damageTypeTeslaName = "Tesla";
    public string damageTypeLanceName = "Vaporise";
    public AudioClip[] normalKillShouts;
    public AudioClip[] explosiveKillShouts;
    public AudioClip[] shotgunKillShouts;
    public AudioClip[] teslaKillShouts;
    public AudioClip[] lanceKillShouts;
    public List<int> normalIndex = new List<int>();
    List<int> explosiveIndex = new List<int>();
    List<int> shotgunIndex = new List<int>();
    List<int> teslaIndex = new List<int>();
    List<int> lanceIndex = new List<int>();
    string damageType = "Normal";

    int getNormalIndex()
    {
        int randomInt = Random.Range(0, normalKillShouts.Length);
        if(normalIndex.Count == normalKillShouts.Length)
        {
            normalIndex = new List<int>(); //Just reset the normal index
        }
        foreach (int i in normalIndex)
        {
            if(i == randomInt)
            {
               return getNormalIndex(); //We've already said this thing, roll again.
            }
        }
        normalIndex.Add(randomInt);
        return randomInt;
    }

    bool shoutChanceCheck(float currentChance)
    {
        float randomValue = (Random.Range(0, 100))/100f;
        if (randomValue < currentChance)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    int getExplosiveIndex()
    {
        int randomInt = Random.Range(0, explosiveKillShouts.Length);
        if (explosiveIndex.Count == explosiveKillShouts.Length)
        {
            explosiveIndex = new List<int>(); //Just reset the normal index
        }
        foreach (int i in explosiveIndex)
        {
            if (i == randomInt)
            {
                return getExplosiveIndex(); //We've already said this thing, roll again.
            }
        }
        explosiveIndex.Add(randomInt);
        return randomInt;
    }
    int getShotgunIndex()
    {
        int randomInt = Random.Range(0, shotgunKillShouts.Length);
        if (shotgunIndex.Count -1 == shotgunKillShouts.Length)
        {
            shotgunIndex = new List<int>(); //Just reset the normal index
        }
        foreach (int i in shotgunIndex)
        {
            if (i == randomInt)
            {
                return getExplosiveIndex(); //We've already said this thing, roll again.
            }
        }
        shotgunIndex.Add(randomInt);
        return randomInt;
    }
    int getTeslaIndex()
    {
        int randomInt = Random.Range(0, teslaKillShouts.Length);
        if (teslaIndex.Count -1 == teslaKillShouts.Length)
        {
            teslaIndex = new List<int>(); //Just reset the normal index
        }
        foreach (int i in teslaIndex)
        {
            if (i == randomInt)
            {
                return getTeslaIndex(); //We've already said this thing, roll again.
            }
        }
        teslaIndex.Add(randomInt);
        return randomInt;
    }

    int getLanceIndex()
    {
        int randomInt = Random.Range(0, lanceKillShouts.Length);
        if (lanceIndex.Count == lanceKillShouts.Length)
        {
            lanceIndex = new List<int>(); //Just reset the normal index
        }
        foreach (int i in lanceIndex)
        {
            if (i == randomInt)
            {
               return getLanceIndex(); //We've already said this thing, roll again.
            }
        }
        lanceIndex.Add(randomInt);
        return randomInt;
    }

    public void CharacterShoutKill(string damageTypeLocal)
    {
        damageType = damageTypeLocal;
        Invoke("quoteSelect", delay);
    }

    void quoteSelect()
    {
        Debug.Log("CharacterShoutKill called");

        if (quoteShouter.isPlaying == false && GameObject.FindObjectOfType<SpeechIndicator>() == null)
        {
            quoteShouter.volume = volume;
            if (damageType == damageTypeNormalName)
            {
                //baseChanceNormal += chanceIncreasePerKill;
                if (shoutChanceCheck(baseChanceNormal) == true)
                {
                    int randomInt = getNormalIndex();
                    GetComponent<PhotonView>().RPC("playShoutNormal", PhotonTargets.All, randomInt);
                }
            }
            if (damageType == damageTypeExplosiveName)
            {
                int randomInt = getExplosiveIndex();
                GetComponent<PhotonView>().RPC("playShoutExplosive", PhotonTargets.All, randomInt);

            }
            if (damageType == damageTypeShotgunName)
            {
                int randomInt = getShotgunIndex();
                GetComponent<PhotonView>().RPC("playShoutShotgun", PhotonTargets.All, randomInt);
            }
            if (damageType == damageTypeTeslaName)
            {
                int randomInt = getTeslaIndex();
                GetComponent<PhotonView>().RPC("playShoutTesla", PhotonTargets.All, randomInt);

            }
            if (damageType == damageTypeLanceName)
            {
                int randomInt = getLanceIndex();
                GetComponent<PhotonView>().RPC("playShoutVaporise", PhotonTargets.All, randomInt);

            }
        }
    }
    [PunRPC]
    void playShoutNormal(int i)
    {
        GameObject indicator = speechIndicator;
        speechIndicator.GetComponent<TimedObjectDestructor>().m_TimeOut = normalKillShouts[i].length;
        Instantiate(indicator);
        quoteShouter.clip = normalKillShouts[i];
        quoteShouter.Play();
    }
    [PunRPC]
    void playShoutExplosive(int i)
    {
        GameObject indicator = speechIndicator;
        speechIndicator.GetComponent<TimedObjectDestructor>().m_TimeOut = explosiveKillShouts[i].length;
        Instantiate(indicator);
        quoteShouter.clip = explosiveKillShouts[i];
        quoteShouter.Play();
    }
    [PunRPC]
    void playShoutShotgun(int i)
    {
        GameObject indicator = speechIndicator;
        speechIndicator.GetComponent<TimedObjectDestructor>().m_TimeOut = shotgunKillShouts[i].length;
        Instantiate(indicator);
        quoteShouter.clip = shotgunKillShouts[i];
        quoteShouter.Play();
    }
    [PunRPC]
    void playShoutTesla(int i)
    {
        GameObject indicator = speechIndicator;
        speechIndicator.GetComponent<TimedObjectDestructor>().m_TimeOut = teslaKillShouts[i].length;
        Instantiate(indicator);
        quoteShouter.clip = teslaKillShouts[i];
        quoteShouter.Play();
    }
    [PunRPC]
    void playShoutVaporise(int i)
    {
        GameObject indicator = speechIndicator;
        speechIndicator.GetComponent<TimedObjectDestructor>().m_TimeOut = lanceKillShouts[i].length;
        Instantiate(indicator);
        quoteShouter.clip = lanceKillShouts[i];
        quoteShouter.Play();
    }


}
