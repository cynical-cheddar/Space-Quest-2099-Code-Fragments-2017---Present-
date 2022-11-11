using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NetworkAiRobot : Photon.MonoBehaviour {



	Vector3 realPosition = Vector3.zero;
	public  Quaternion realRotation = Quaternion.identity;
	public  Quaternion realTorsoRotation = Quaternion.identity;


	bool gotFirstUpdate = false;
	public bool isPlayer = true;
	public float RealAimAngle = 0f;

	public Transform torso;
	// Use this for initialization

	void Start() {
		if (PhotonNetwork.isMasterClient) {
			GetComponent<AiBehaviour> ().enabled = true;
		}
	}

	void CacheComponents()
	{
		if (!PhotonNetwork.isMasterClient) {
			GetComponent<NavMeshAgent> ().enabled = false;
		}
	}
	// Update is called once per frame
	void Update() {
		if (PhotonNetwork.isMasterClient)
		{
			// DO NUFFINK
		}
		else
		{
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
			torso.rotation = Quaternion.Lerp(torso.rotation, realTorsoRotation, 0.1f);

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
			stream.SendNext(torso.rotation);
			//  stream.SendNext(anim.GetBool("Firing"));
		}
		else
		{
			//Not our player, position must be received (as of some ms ago, then update position)

			realPosition = (Vector3) stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();

			//  anim.SetBool("Fire", (bool)stream.ReceiveNext());
			//anim.SetBool("Firing", (bool)stream.ReceiveNext());
			realTorsoRotation = (Quaternion)stream.ReceiveNext();


			if (gotFirstUpdate == false)
			{
				transform.position = realPosition;
				transform.rotation = realRotation;
				torso.rotation = realTorsoRotation;
				gotFirstUpdate = true;
			}
		}
	}
}
