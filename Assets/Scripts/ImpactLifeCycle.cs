using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]

public class ImpactLifeCycle : MonoBehaviour {

    [SerializeField]
    private float lifespan = 1.5f;

    private ParticleSystem particleEffect;

    
    void Start() {
        GetComponent<ParticleSystem>().Play();
        Destroy(gameObject, lifespan);
    }

}
