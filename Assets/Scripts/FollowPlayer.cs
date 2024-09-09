/* Copyright (C) 2024 Christopher Buer */

using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public enum ObstacleHandling
    {
        Move,
        Clip
    }

    public Transform player;
    public ObstacleHandling obstacleHandling;
    public float clipCorrection = -0.2f;
    public float followDistance = 6.0f;
    public float followThreshold = 0.01f;

    #if DEBUG
    public bool drawDebug;
    #endif

    private int layerMask;
    private int layerMaskWalls;
    private float orignalClipPlane;

    private void OnEnable()
    {
        #if DEBUG
        drawDebug = true;
        #endif

        layerMaskWalls = LayerMask.GetMask("Walls");
        orignalClipPlane = Camera.main.nearClipPlane;
    }

    private void FixedUpdate()
    {
        Camera.main.nearClipPlane = orignalClipPlane;

        Vector3 lookAtPosition = player.position + new Vector3(0,1,0);
        Vector3 lookOffset = (player.forward * followDistance) + new Vector3(0,0.2f,0);
        Vector3 cameraPosition = lookAtPosition - lookOffset;
        Vector3 playerOffset = lookAtPosition - cameraPosition;

        #if DEBUG
        bool isObstructed = false;
        #endif

        Vector3 raycastStart = cameraPosition;
        RaycastHit hit;
        while (Physics.Raycast(raycastStart, playerOffset, out hit,
                playerOffset.magnitude, layerMaskWalls))
        {
            #if DEBUG
            isObstructed = true;
            #endif
            Vector3 offset = (hit.point - raycastStart);
            Vector3 oldRaycastStart = raycastStart;
            raycastStart = hit.point + (offset.normalized * 1.5f);
            if (obstacleHandling == ObstacleHandling.Move)
            {
                cameraPosition = raycastStart;
            }
            else
            {
                float clipAdjust = (raycastStart - oldRaycastStart).magnitude;
                Camera.main.nearClipPlane = orignalClipPlane + clipAdjust + clipCorrection;
            }
        }

        transform.position = cameraPosition;
        transform.LookAt(lookAtPosition, Vector3.up);


        #if DEBUG
        if (drawDebug)
        {
            Color debugLineColor = Color.green;
            if (isObstructed)
            {
                debugLineColor = Color.red;
            }
            Debug.DrawRay(transform.position, lookAtPosition - transform.position,
                debugLineColor);
        }
        #endif
    }
}
