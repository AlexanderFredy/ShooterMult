using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private RectTransform _filledImage;
    [SerializeField]  private float _defaultWidth;
    private Transform _camera;

    private void OnValidate()
    {
        _defaultWidth = _filledImage.sizeDelta.x;
    }

    void Start()
    {     
        _camera = Camera.main.transform;
    }

    private void Update()
    {
        transform.LookAt(_camera);
    }

    public void UpdateHealth(float max, int current)
    {
        float percent = current / max;
        _filledImage.sizeDelta = new Vector2(percent * _defaultWidth, _filledImage.sizeDelta.y);
    }
}
