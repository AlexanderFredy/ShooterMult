using UnityEngine;
using UnityEngine.UI;

public class SniperZoom : MonoBehaviour
{
    [SerializeField] private Image zoom;

    private Camera _camera;
    private Camera _mainCamera;
    private bool isZoom = false;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _camera.enabled = false;
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isZoom = !isZoom;
            _camera.enabled |= isZoom;
            _mainCamera.enabled = !isZoom;
        }

        //if (isZoom)
        //{
        //    zoom.gameObject.SetActive(true);
        //    _camera.fieldOfView = 10;
        //}
        //else
        //{
        //    zoom.gameObject.SetActive(false);
        //    _camera.fieldOfView = 60;
        //}
    }
}
