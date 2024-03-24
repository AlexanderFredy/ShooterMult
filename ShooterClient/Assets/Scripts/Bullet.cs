using System.Collections;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _lifeTime;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private DamageType _damageType = DamageType.point;
    [SerializeField,ShowIf("_damageType", DamageType.explosive)] private float _explosionRadius;
    [SerializeField, ShowIf("_damageType", DamageType.explosive)] private float _power = 100f;
    [SerializeField, ShowIf("_damageType", DamageType.explosive)] private VisualEffect _explosionEffectPrefab;

    private int _damage;
    public Action<Vector3> headShot;

    public void Init(Vector3 velocity, int damage = 0)
    {
        _damage = damage;
        _rigidbody.velocity = velocity;
        StartCoroutine(DelayDestroy());
    }

    private IEnumerator DelayDestroy()
    {
        yield return new WaitForSecondsRealtime(_lifeTime);
        Destroy();
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (_damageType)
        {
            case DamageType.point:
                {
                    if (collision.collider.TryGetComponent(out EnemyCharacter enemy))
                        enemy.ApplyDamage(_damage);
                    else if (collision.collider.CompareTag("Head"))
                    {
                        collision.collider.GetComponentInParent<EnemyCharacter>().ApplyDamage(_damage*2);
                        headShot?.Invoke(collision.collider.transform.position);
                    }
                    break;
                }
            case DamageType.explosive:
                {
                    if (_explosionEffectPrefab != null)
                        Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

                    var OverHits = Physics.OverlapSphere(transform.position, _explosionRadius);
                    for (int i = 0; i < OverHits.Length; i++)
                    {
                        Rigidbody RB = OverHits[i].attachedRigidbody;

                        if (RB)
                            RB.AddExplosionForce(_power, transform.position, _explosionRadius, 0.5f, ForceMode.Impulse);

                        if (OverHits[i].TryGetComponent(out EnemyCharacter enemy))
                        {
                            float distance = Vector3.Distance(transform.position, OverHits[i].transform.position);
                            int dmg = Mathf.FloorToInt(_damage * (_explosionRadius - distance) / _explosionRadius);
                            enemy.ApplyDamage(dmg);
                        }                 
                    }
                    break;
                }
        }
        
        Destroy();
    }
}

public enum DamageType
{
    point = 0,
    explosive = 1,
}
