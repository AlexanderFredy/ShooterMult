using System.Collections;
using TMPro;
using UnityEngine;

public class HeadShotLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmp;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float upOffset = 50f;
    public RectTransform rectTrans;

    IEnumerator Start()
    {
        var time = 0f;
        Vector3 pos = rectTrans.position;
        Color col = _tmp.color;

        while (time < duration)
        {
            pos.y += upOffset/duration*Time.deltaTime;
            col.a -= duration*Time.deltaTime/2;
            rectTrans.position = pos;
            _tmp.color = col;

            time += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
