
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] Player _player;
    
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        _player.SetInput(h, v);
    }
}
