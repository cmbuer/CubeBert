/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBertMove : MonoBehaviour
{
    public Transform rightHip;
    public Transform leftHip;
    public Transform rightKnee;
    public Transform leftKnee;
    public Transform stepTargetRight;
    public Transform stepTargetLeft;
    public Transform rightArm;
    public Transform leftArm;
    public CubeBertLegMove leftLegMove;
    public CubeBertLegMove rightLegMove;
    public CharacterController controller;
    public float speed = 5;

    #if DEBUG
    public bool drawDebug;
    #endif

    private enum AppendageSide { Right, Left }

    private AppendageSide currentFoot;

    private bool stepInProgress;
    private bool jumpInProgress;
    private bool freeFallin;

    private MainPlayerInput playerControls;

    private Vector3 originalPositionRight;
    private Vector3 originalPositionLeft;
    private Vector3 referencePoistionRight;
    private Vector3 referencePositionLeft;

    private float jumpStartTime;
    private float stepTime;

    private const float stepHeight = 0.25f;
    private const float stepLength = 0.30f;
    private const float stepInterval = 0.25f;
    private const float jumpRiseInterval = 0.5f;
    private const float jumpSpeed = 15;
    private const float stepRate = 3;

    public Vector3 GetChestPosition()
    {
        return transform.position + controller.center + (transform.up * 0.5f);
    }

    public Vector3 GetFeetPosition()
    {
        return transform.position + controller.center - (transform.up * controller.height * 0.5f);
    }

    private void Awake()
    {
        #if DEBUG
        drawDebug = true;
        #endif

        playerControls = new MainPlayerInput();
        playerControls.Newactionmap.Enable();

        stepInProgress = false;
        jumpInProgress = false;
        freeFallin = false;

        referencePoistionRight = stepTargetRight.localPosition;
        referencePositionLeft = stepTargetLeft.localPosition;
    }

    private void FixedUpdate()
    {
        controller.Move(Vector3.down * Time.deltaTime * 0.04f); // constant downward force to trigger grounding

        #if DEBUG
        if (drawDebug)
        {
            const float vertLineLen = 4;
            const float horzLineLen = 1;
            Color lineColor = controller.isGrounded ? Color.yellow : Color.magenta;
            Vector3 feetPosition = GetFeetPosition();
            Debug.DrawRay(feetPosition, Vector3.up * vertLineLen, lineColor);
            Debug.DrawRay(feetPosition, (Vector3.forward * horzLineLen * 2), lineColor);
            Debug.DrawRay(feetPosition, (Vector3.right * horzLineLen * 2), lineColor);
        }
        #endif

        if (!controller.isGrounded)
        {
            if (!freeFallin)
            {
                freeFallin = true;
                stepTargetLeft.localPosition = referencePositionLeft;
                stepTargetRight.localPosition = referencePoistionRight;
            }

            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        if (freeFallin && controller.isGrounded)
        {
            freeFallin = false;
            stepTargetLeft.localPosition = referencePositionLeft;
            stepTargetRight.localPosition = referencePoistionRight;
        }

        if (jumpInProgress)
        {
            if (!controller.isGrounded)
            {
                if (Time.time - jumpStartTime < jumpRiseInterval)
                {
                    controller.Move(Vector3.up * jumpSpeed * Time.deltaTime);
                }
            }
            else
            {
                jumpInProgress = false;
            }
        }

        if (playerControls.Newactionmap.Jump.IsPressed() && !jumpInProgress)
        {
            stepTargetLeft.localPosition = referencePositionLeft;
            stepTargetRight.localPosition = referencePoistionRight;
            jumpInProgress = true;
            jumpStartTime = Time.time;
            controller.Move(Vector3.up * 12f * Time.deltaTime);
        }

        if (playerControls.Newactionmap.TurnRight.IsPressed())
        {
            transform.eulerAngles += new Vector3(0, 90, 0) * Time.deltaTime;
        }

        if (playerControls.Newactionmap.TurnLeft.IsPressed())
        {
            transform.eulerAngles += new Vector3(0, -90, 0) * Time.deltaTime;
        }

        int walkDir = 0;
        if (playerControls.Newactionmap.Forward.IsPressed())
        {
            walkDir = 1;
        }
        else if (playerControls.Newactionmap.Backward.IsPressed())
        {
            walkDir = -1;
        }

        if (walkDir != 0)
        {
            if (!stepInProgress)
            {
                currentFoot = (currentFoot == AppendageSide.Left) ?
                                AppendageSide.Right : AppendageSide.Left;
                originalPositionRight = referencePoistionRight;
                originalPositionLeft = referencePositionLeft;
                stepInProgress = true;
                stepTime = 0;
            }

            controller.Move(transform.forward * (float)walkDir * Time.deltaTime * speed);

            float percent = stepTime / stepInterval;
            if (percent >= 1.0f)
            {
                percent = 1.0f;
                stepInProgress = false;
            }

            Vector3 footOffset = new Vector3();
            if (percent < 0.65f)
            {
                float stepHalfLength = stepLength * 0.5f;
                float arcPercent = percent / 0.65f;
                float angle = (180.0f - (arcPercent * 180.0f)) * Mathf.Deg2Rad;
                footOffset.z = stepHalfLength + (Mathf.Cos(angle) * stepHalfLength);
                footOffset.y = Mathf.Sin(angle) * stepHeight;
            }
            else
            {
                float slidePercent = (0.35f - (percent - 0.65f)) / 0.35f;
                footOffset.z = (stepLength * slidePercent);
            }

            if (currentFoot == AppendageSide.Left)
            {
                stepTargetLeft.localPosition = referencePositionLeft + footOffset;
                leftLegMove.Solve();
                rightArm.localRotation = leftHip.localRotation;
                Quaternion leftHipRotationInverse = Quaternion.Inverse(leftHip.localRotation);
                leftArm.localRotation = leftHipRotationInverse;
            }
            else
            {
                stepTargetRight.localPosition = referencePoistionRight + footOffset;
                rightLegMove.Solve();
                leftArm.localRotation = rightHip.localRotation;
                Quaternion rightHipRotationInverse = Quaternion.Inverse(rightHip.localRotation);
                rightArm.localRotation = rightHipRotationInverse;
            }

            stepTime += Time.deltaTime;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "platform")
        {
            if (Vector3.Dot(hit.normal, transform.up.normalized) == 1)
            {
                // hit platform top
            }
            else
            {
                // hit platform side
                if (jumpInProgress)
                {
                    controller.Move(Vector3.up);
                }
            }
        }
    }
}
