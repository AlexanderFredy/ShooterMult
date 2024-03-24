using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [SerializeField] float _lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,_lifeTime);
    }
}
