using Photon.Pun;
using UnityEngine;

public class TpsGun : MonoBehaviourPunCallbacks, IPunObservable {

    [Tooltip("The scaling number for changing the local postion Y of TpsGun when aiming angle changes.")]
    [SerializeField]
    private float localPositionYScale = 0.007f;
    [SerializeField]
    private ParticleSystem gunParticles;
    [SerializeField]
    private AudioSource gunAudio;
    [SerializeField]
    private FpsGun fpsGun;
    [SerializeField]
    private Animator animator;

    private float timer;
    private Vector3 localPosition;
    private Quaternion localRotation;
    private float smoothing = 2.0f;
    private float defaultLocalPositionY;

    
    void Start() {
        if (photonView.IsMine) {
            defaultLocalPositionY = transform.localPosition.y;
        } else {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
        }
    }

    
    void Update() {
        if (photonView.IsMine) {
            transform.rotation = fpsGun.transform.rotation;
        }
    }

    
    void LateUpdate() {
        if (photonView.IsMine) {
            float deltaEulerAngle = 0f;
            if (transform.eulerAngles.x > 180) {
                deltaEulerAngle = 360 - transform.eulerAngles.x;
            } else {
                deltaEulerAngle = -transform.eulerAngles.x;
            }
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                defaultLocalPositionY + deltaEulerAngle * localPositionYScale,
                transform.localPosition.z
            );
        } else {
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, Time.deltaTime * smoothing);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation, Time.deltaTime * smoothing);
        }
    }

    
    public void RPCShoot() {
        if (photonView.IsMine) {
            photonView.RPC("Shoot", RpcTarget.All);
        }
    }

    
    [PunRPC]
    void Shoot() {
        gunAudio.Play();
        if (!photonView.IsMine) {
            if (gunParticles.isPlaying) {
                gunParticles.Stop();
            }
            gunParticles.Play();
        }
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localRotation);
        } else {
            localPosition = (Vector3)stream.ReceiveNext();
            localRotation = (Quaternion)stream.ReceiveNext();
        }
    }

}
