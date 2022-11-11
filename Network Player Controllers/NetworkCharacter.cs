using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class NetworkCharacter : Photon.MonoBehaviour {


    Vector3 realPosition = Vector3.zero;
   public  Quaternion realRotation = Quaternion.identity;
   public Animator anim;
    public Animator animPhaserBarrel;
    public Animator animBarrel;
    bool gotFirstUpdate = false;
	public bool isPlayer = true;
    public float RealAimAngle = 0f;
    // Use this for initialization

    void Start() {
        if (anim == null)
        {
            Transform animParent = transform.Find("PlayerModel");
            anim = animParent.gameObject.GetComponent<Animator>();
        }
        if (GetComponent<PhotonView>().isMine == true && isPlayer)
        {
            if(GameObject.FindObjectOfType<StartObject>() != null){
                transform.position = GameObject.FindObjectOfType<StartObject>().transform.position;
                transform.rotation = GameObject.FindObjectOfType<StartObject>().transform.rotation;
            }
            gameObject.layer = 2; // makes it so we can't shoot ourself
            foreach(Transform child in transform)
            {
				if (child.name == "LandTrigger") {
					child.gameObject.layer = 2;
				}
              //  
            }

        }
    }

    void OnLevelWasLoaded(){

             if(GameObject.FindObjectOfType<StartObject>() != null){
                transform.position = GameObject.FindObjectOfType<StartObject>().transform.position;
                transform.rotation = GameObject.FindObjectOfType<StartObject>().transform.rotation;
            }
    }

    void CacheComponents()
    {
        if (anim == null)
        {
            Transform animParent = transform.Find("PlayerModel");
            anim = animParent.gameObject.GetComponent<Animator>();
        }
    }
    // Update is called once per frame
    void Update() {
        if (photonView.isMine)
        {
            // DO NUFFINK
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
            anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), RealAimAngle, 0.1f));
            animPhaserBarrel.SetFloat("AimAngleBarrel", Mathf.Lerp(animPhaserBarrel.GetFloat("AimAngleBarrel"), RealAimAngle, 0.1f));
            animBarrel.SetFloat("AimAngleBarrel", Mathf.Lerp(animPhaserBarrel.GetFloat("AimAngleBarrel"), RealAimAngle, 0.1f));
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        CacheComponents();
        if (stream.isWriting)
        {
            //Our player, our position must be sent to the network.

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(anim.GetFloat("Speed"));
            stream.SendNext(anim.GetFloat("StrafeSpeed"));
            stream.SendNext(anim.GetBool("Jumping"));
         //   stream.SendNext(anim.GetBool("Fire"));
            stream.SendNext(anim.GetFloat("AimAngle"));
          //  stream.SendNext(anim.GetBool("Firing"));
        }
        else
        {
            //Not our player, position must be received (as of some ms ago, then update position)

            realPosition = (Vector3) stream.ReceiveNext();
            realRotation = (Quaternion)stream.ReceiveNext();
            anim.SetFloat("Speed", (float)stream.ReceiveNext());
            anim.SetFloat("StrafeSpeed", (float)stream.ReceiveNext());
            anim.SetBool("Jumping", (bool)stream.ReceiveNext());
          //  anim.SetBool("Fire", (bool)stream.ReceiveNext());
            //anim.SetBool("Firing", (bool)stream.ReceiveNext());
            RealAimAngle = (float)stream.ReceiveNext();


            if (gotFirstUpdate == false)
            {
                transform.position = realPosition;
                transform.rotation = realRotation;
                anim.SetFloat("AimAngle", RealAimAngle);
                animPhaserBarrel.SetFloat("AimAngleBarrel", RealAimAngle);
                gotFirstUpdate = true;
            }
        }
    }
}
