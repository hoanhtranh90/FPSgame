using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NameTag : MonoBehaviourPunCallbacks {

    [HideInInspector]
    public Transform target = null;

    [SerializeField]
    private Text nameText;

    
    void Start() {
        if (photonView.IsMine) {
            photonView.RPC("SetName", RpcTarget.All, PhotonNetwork.NickName);
        } else {
            SetName(photonView.Owner.NickName);
        }
    }

    
    void Update() {
        if (target != null) {
            Vector3 lookAtVec = transform.position + (transform.position - target.position);
            transform.LookAt(lookAtVec, Vector3.up);
        }
    }

    /// <summary>
    /// RPC function to set player name tag.
    /// </summary>
    [PunRPC]
    void SetName(string name) {
        nameText.text = name;
    }

}
