using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrganControler : MonoBehaviour
{
    private Camera _camera;
    [SerializeField]private LayerMask clickableMask; 
    void Start()
    {
        if(!_camera) _camera = Camera.main;
    }
    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        Vector3 world = _camera.ScreenToWorldPoint(mousePos);
        world.z = 0f;
        transform.position = world;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        
        var hit = Physics2D.OverlapPoint(world, clickableMask);
        if (!hit) return;
        
        if (hit.CompareTag($"OrganBox"))
        {
            OrganBoxChosen();
        }
        else if (hit.CompareTag($"MopBucket"))
        {
            MopBucketChosen();
        }
        
        gameObject.SetActive(false);
    }
    private void OrganBoxChosen()
    {
        Debug.Log("OrganBox Chosen");
    }

    private void MopBucketChosen()
    {
        Debug.Log("MopBucket chosent");
    }
}