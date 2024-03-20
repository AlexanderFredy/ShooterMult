using UnityEngine;

public class GunAnimations : MonoBehaviour
{
    private const string shoot = "Shoot";

    [SerializeField] private Gun _gun;
    [SerializeField] private Animator _animator;

    void Start()
    {
        _gun.shoot += Shoot;
    }

    private void Shoot()
    {
        _animator.SetTrigger(shoot);
    }

    void OnDestroy()
    {
        _gun.shoot -= Shoot;
    }

    public void SetGun(Gun gun)
    {
        _gun = gun;
    }
}
