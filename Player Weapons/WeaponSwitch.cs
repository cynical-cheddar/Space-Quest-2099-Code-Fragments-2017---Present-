using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class WeaponSwitch : MonoBehaviour
{
    SelfNetworkManagerII snm;
    public MonoBehaviour[] WeaponScripts;
    public GameObject[] WeaponModels;
    public GameObject[] WeaponViewModels;
    public GameObject[] Backpacks;
    public int[] WeaponTypeIDs;
    public bool[] UnlockedWeapons;
    public float selected = 0;
    public float lastSelected = 0;
    GameObject UserHud;
    public GameObject iconMaster;
    public GameObject inventoryBarGraphics;
    Animator inventoryBarGraphicsAnimator;
    public Sprite[] WeaponImages;
    public GameObject[] WeaponIcons;
    public GameObject weaponIconTemplate;
    public GameObject weaponIconTemplateHighlight;
    bool foundGun = false;
    bool inventoryBarFound = false;
    public float inventoryBarSelectionCooldown = 2f;
  public  float cooldown = 2f;
    public AnimationClip switchAnimation;
    int intLastSelected = 0;
    int intSelected = 0;
    public AudioClip switchSound;
    bool scrollUp = true;
    bool scrollDown = false;
    bool sound = true;
    // Use this for initialization
    void Start()
    {
        // Enable first weapon
        DisableAndEnable();
        GetComponent<PhotonView>().RPC("DisableAndEnableModel", PhotonTargets.AllBuffered, 0, 0);
        snm = GameObject.FindObjectOfType<SelfNetworkManagerII>();
        UserHud = snm.PlayerUI;
         iconMaster = UserHud.transform.Find("InventoryBar/Icons").gameObject;
        inventoryBarGraphics = UserHud.transform.Find("InventoryBar").gameObject;
        inventoryBarGraphicsAnimator = inventoryBarGraphics.GetComponent<Animator>();
        if (iconMaster != null)
        {
            inventoryBarFound = true;
            clearInventoryBar();
            populateInventoryBar();
            highlightSelected();
        }
    }
    void showInventoryBar()
    {
        cooldown = inventoryBarSelectionCooldown;
        inventoryBarGraphicsAnimator.SetBool("DoneSwitching", false);
        inventoryBarGraphicsAnimator.SetTrigger("SwitchingWeapons");

    }
    void weaponSwitchAnimation()
    {
        transform.Find("FirstPersonCharacter/ViewModels").gameObject.GetComponent<Animation>().clip = switchAnimation;
        transform.Find("FirstPersonCharacter/ViewModels").gameObject.GetComponent<Animation>().Play();
        gameObject.GetComponent<CanFire>().CanShoot = false;
        gameObject.GetComponent<CanFire>().CanSwitch = false;

        Invoke("canNowFire", switchAnimation.length);
    }
    void canNowFire()
    {
        gameObject.GetComponent<CanFire>().CanShoot = true;
        gameObject.GetComponent<CanFire>().CanSwitch = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (GetComponent<CanFire>().CanSwitch == true)
            {

                showInventoryBar();

                lastSelected = selected;
                selected += Input.GetAxis("Mouse ScrollWheel") * 10;
                switchyThing(true);
            }
        }
        else
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0 )
            {
                inventoryBarGraphicsAnimator.SetBool("DoneSwitching", true);
            }
        }
    }

    void switchyThing(bool performAnimation)
    {
        sound = true;
        if (performAnimation == false) sound = false;
        int i = 0;
        foreach (bool weaponPresent in UnlockedWeapons)
        {
            if (weaponPresent == true)
            {
                i += 1;
            }
        }
        if (selected > lastSelected)
        {
            scrollUp = true;
            scrollDown = false;
        }
        if (selected < lastSelected)
        {
            scrollDown = true;
            scrollUp = false;
        }

        int startSelected = (int)lastSelected;
        if (scrollUp == true)
        {
            if (i > 1)
            {
                while (foundGun == false)
                {
                    selected = SwitchWeaponUp(startSelected);
                }
            }
        }
        if (scrollDown == true)
        {
            if (i > 1)
            {
                while (foundGun == false)
                {
                    selected = SwitchWeaponDown(startSelected);
                }
            }
        }


        intLastSelected = (int)lastSelected;
        intSelected = (int)selected;
        GetComponent<PhotonView>().RPC("DisableAndEnableModel", PhotonTargets.AllBuffered, intLastSelected, intSelected);


        if (i > 1)
        {
            if (performAnimation)
            {
                weaponSwitchAnimation();
                Invoke("DisableAndEnable", switchAnimation.length / 2);
            }
            else
            {
                DisableAndEnable();
            }
            
            
        }

        foundGun = false;

        // Show inventory bar:
        if (inventoryBarFound == true)
        {
            clearInventoryBar();
            populateInventoryBar();
            highlightSelected();
        }
    }

    public float SwitchWeaponUp(int startSelected)
    {

        if (selected > WeaponScripts.Length - 1)
        {
            selected = 0;
        }
        if (selected == -1)
        {
            selected = WeaponScripts.Length - 1;
        }
        //Check if we actually have the weapon:

         intLastSelected = (int)lastSelected;
         intSelected = (int)selected;
 
        if (UnlockedWeapons[intSelected] == true)
        {
            
            GetComponent<AudioSource>().clip = switchSound;
            if(sound) GetComponent<AudioSource>().Play();
           foundGun = true;
            return selected;
        }
       // if(intSelected == startSelected)
       // {
       //     foundGun = true;
       //     return selected;
      //  }
        else
        {
            selected += 1;
            return selected;
        }
    }
    float SwitchWeaponDown(int startSelected)
    {

        if (selected > WeaponScripts.Length - 1)
        {
            selected = 0;
        }
        if (selected == -1)
        {
            selected = WeaponScripts.Length - 1;
        }
        //Check if we actually have the weapon:

        int intLastSelected = (int)lastSelected;
        int intSelected = (int)selected;
        if (UnlockedWeapons[intSelected] == true)
        {
             GetComponent<AudioSource>().clip = switchSound;
            if (sound) GetComponent<AudioSource>().Play();
            foundGun = true;
            return selected;
        }
       // if (intSelected == startSelected)
        //{
           // foundGun = true;
            //return selected;
       // }
        else
        {
            selected -= 1;
            return selected;
        }
    }
    void DisableAndEnable()
    {
        WeaponScripts[intLastSelected].enabled = false;
        WeaponViewModels[intLastSelected].SetActive(false);
        int intSelected = (int)selected;
        WeaponScripts[intSelected].enabled = true;
        
        WeaponViewModels[intSelected].SetActive(true);


    }
    public void populateInventoryBar()
    {
        int i = 0;
        int j = 0;
        foreach(Sprite image in WeaponImages)
        {
            if(UnlockedWeapons[i] == true)
            {

                GameObject iconInstance = Instantiate(weaponIconTemplate);
                iconInstance.GetComponent<Image>().sprite = image;
                iconInstance.transform.parent = iconMaster.transform;
                iconInstance.transform.localPosition = new Vector3((160 * j), 0, 0);
                iconInstance.GetComponent<IconWeaponCorrespondence>().weaponNumber = i;
                float sf = scaleFactor();
                iconInstance.GetComponent<RectTransform>().localScale = new Vector3(sf, sf, sf);
                j += 1;
            }
            i += 1;
        }

    }
  public void clearInventoryBar()
    {
        foreach(Transform child in iconMaster.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void highlightSelected()
    {
        if(selected <= 0)
        {
            selected = 0;
        }
        foreach (Transform child in iconMaster.transform)
        {
            if(child.gameObject.GetComponent<IconWeaponCorrespondence>().weaponNumber == selected)
            {
                GameObject iconInstanceHighlight = Instantiate(weaponIconTemplateHighlight);
                iconInstanceHighlight.transform.parent = child;
                iconInstanceHighlight.transform.localPosition = new Vector3(0, 0, 0);
                float sf = scaleFactor();
                iconInstanceHighlight.GetComponent<RectTransform>().localScale = new Vector3(sf, sf, sf);
            }
        }
    }



    [PunRPC]
    void DisableAndEnableModel(int lastSelectedID, int SelectedID)
    {
        WeaponModels[lastSelectedID].SetActive(false);
        WeaponModels[SelectedID].SetActive(true);
        Backpacks[lastSelectedID].SetActive(false);
        Backpacks[SelectedID].SetActive(true);
        Animator anim;
        Transform animParent = this.transform.Find("PlayerModel");
        anim = animParent.gameObject.GetComponent<Animator>();
        anim.SetInteger("WeaponType", WeaponTypeIDs[SelectedID]);
    }

    float scaleFactor()
    {
        float factor = 1f;
        float currentArea = Screen.width * Screen.height;
        factor = (currentArea / (1920 * 1080));
        return 1;
    }
}