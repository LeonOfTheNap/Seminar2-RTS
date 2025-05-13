using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    Camera camera;
    NavMeshAgent navMeshAgent;
    public LayerMask ground;

    private void Start()
    {
        camera = Camera.main;
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            
            //check if you hit something with right mouse button
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            //if you hit something, and it's on the ground layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                //send agent to the destination where the raycast hit
                navMeshAgent.SetDestination(hit.point);
            }
        }
    }
}
