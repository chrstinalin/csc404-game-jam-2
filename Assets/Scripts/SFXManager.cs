using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bulletFireClip;
    
    public static SFXManager Instance;
    
    void Awake() 
    {
        if (Instance == null) Instance = this;
    }
    
    public void PlayBulletFire() 
    {
        audioSource.PlayOneShot(bulletFireClip);
    }
}
