using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(Rigidbody))]

public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable {

    public delegate void Respawn(float time);
    public delegate void AddMessage(string Message);
    public event Respawn RespawnEvent;
    public event AddMessage AddMessageEvent;

    [SerializeField]
    private int startingHealth = 100;
    [SerializeField]
    private float sinkSpeed = 0.12f;
    [SerializeField]
    private float sinkTime = 2.5f;
    [SerializeField]
    private float respawnTime = 8.0f;
    [SerializeField]
    private AudioClip deathClip;
    [SerializeField]
    private AudioClip hurtClip;
    [SerializeField]
    private AudioSource playerAudio;
    [SerializeField]
    private float flashSpeed = 2f;
    [SerializeField]
    private Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    [SerializeField]
    private NameTag nameTag;
    [SerializeField]
    private Animator animator;

    private FirstPersonController fpController;
    private IKControl ikControl;
    private Slider healthSlider;
    private Image damageImage;
    private int currentHealth;
    private bool isDead;
    private bool isSinking;
    private bool damaged;
    
    private Text scoreBoard;
    
    void Start() {
        fpController = GetComponent<FirstPersonController>();
        ikControl = GetComponentInChildren<IKControl>();
        damageImage = GameObject.FindGameObjectWithTag("Screen").transform.Find("DamageImage").GetComponent<Image>();
        healthSlider = GameObject.FindGameObjectWithTag("Screen").GetComponentInChildren<Slider>();
        scoreBoard = GameObject.FindGameObjectWithTag("Score").GetComponent<Text>();
         // Cập nhật giá trị của scoreText
        // UpdateScoreText();
        currentHealth = startingHealth;
        // currscore = startscore;
        
        if (photonView.IsMine) {
            gameObject.layer = LayerMask.NameToLayer("FPSPlayer");
            healthSlider.value = currentHealth;
            // scoreText.text = "Score: " + currscore;
        }
        damaged = false;
        isDead = false;
        isSinking = false;
    }

    
    void Update() {
        if (damaged) {
            damaged = false;
            damageImage.color = flashColour;
        } else {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        if (isSinking) {
            transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime);
        }
    }
    [PunRPC]
    void AddScore (int scoreAdd, string enemyName) {
        print("AddScore for " + enemyName);
        // Tìm người chơi có tên trùng với enemyName
        foreach (Player player in PhotonNetwork.PlayerList) {
            if (player.NickName == enemyName) {
                //add score
                player.AddScore(scoreAdd);
            }
        }
    }

    
    [PunRPC]
    public void TakeDamage(int amount, string enemyName) {
        if (isDead) return;
        if (photonView.IsMine) {
            damaged = true;
            currentHealth -= amount;
            if (currentHealth <= 0) {
                photonView.RPC("Death", RpcTarget.All, enemyName);
                photonView.RPC("AddScore", RpcTarget.All, 1, enemyName);
                photonView.RPC("UpdateScoreBoard_RPC", RpcTarget.All);
                
            }
            healthSlider.value = currentHealth;
            animator.SetTrigger("IsHurt");
        }
        playerAudio.clip = hurtClip;
        playerAudio.Play();
    }

    
    [PunRPC]
    void Death(string enemyName) {
        isDead = true;
        ikControl.enabled = false;
        nameTag.gameObject.SetActive(false);
        if (photonView.IsMine) {
            fpController.enabled = false;
            animator.SetTrigger("IsDead");
            AddMessageEvent(PhotonNetwork.LocalPlayer.NickName + " was killed by " + enemyName + "!");
            photonView.RPC("UpdateScoreBoard_RPC", RpcTarget.All);
            RespawnEvent(respawnTime);
            StartCoroutine("DestoryPlayer", respawnTime);
        }
        playerAudio.clip = deathClip;
        playerAudio.Play();
        StartCoroutine("StartSinking", sinkTime);
    }
    [PunRPC]
    void UpdateScoreBoard_RPC() {
        //clear old
        scoreBoard.text = "";
        //log score of all player in room
        print("Player list: " + PhotonNetwork.PlayerList.Length);
        foreach (Player player in PhotonNetwork.PlayerList) {
            print(player.NickName + ": " + player.GetScore());
            scoreBoard.text += player.NickName + ": " + player.GetScore() + "  ";
            if(player.GetScore() >= 5) {
                scoreBoard.text = "";
                scoreBoard.text += "WINNER: " + player.NickName;
                StartCoroutine("EndGame", 5.0f);
            }
        }

    }
    
    IEnumerator DestoryPlayer(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        PhotonNetwork.Destroy(gameObject);
    }

    
    IEnumerator StartSinking(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        isSinking = true;
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(currentHealth);
        } else {
            currentHealth = (int)stream.ReceiveNext();
        }
    }

    IEnumerator EndGame(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Start");
    }



    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}
