/* Inverse Kinematics solver
 * Copyright (C) 2024 Christopher Buer
 * Implementation is based on the following whitepaper:
 *  FABRIK: A fast, iterative solver for the Inverse Kinematics problem
 *  by: Andreas Aristidou, Joan Lasenby
 *  http://www.andreasaristidou.com/publications/papers/FABRIK.pdf
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabrikChain
{
    private const int axes = 3;
    private const int Xpos = 0;
    private const int Xneg = 1;
    private const int Ypos = 2;
    private const int Yneg = 3;
    private const int Zpos = 4;
    private const int Zneg = 5;

    /* These work arrays are cached across all calls to SolveChain and are
     * additionally shared across all instances of FabrikChain.
     * This amounts to a very basic implementation of the Flyweight pattern.
     * Note that this is not intended for multithreaded use since there
     * is obviously no locking protecting the static objects.
     */
    private static Vector3[] jointPositions;
    private static Quaternion[] initialJointRotations;
    private static Vector3[] initialLinkDirections;
    private static float[] linkLengths;

    public void SolveChain(Transform[] joints, Transform target, float[,] rotationConstraints,
        int iterations = 10, float maxDelta = .0001f)
    {
        float chainLength = 0;
        int jointQty = joints.Length;
        int linkQty = jointQty - 1;
        int endEffector = jointQty - 1;
        Transform root = joints[0];

        // Expand work arrays if needed

        if (jointPositions == null || jointPositions.Length < jointQty)
        {
            jointPositions = new Vector3[jointQty];
        }
        if (initialJointRotations == null || initialJointRotations.Length < jointQty)
        {
            initialJointRotations = new Quaternion[jointQty];
        }
        if (initialLinkDirections == null || initialLinkDirections.Length < linkQty)
        {
            initialLinkDirections = new Vector3[linkQty];
        }
        if (linkLengths == null || linkLengths.Length < linkQty)
        {
            linkLengths = new float[linkQty];
        }
        
        // End effector

        initialLinkDirections[linkQty - 1] = target.localPosition - joints[endEffector].localPosition;
        initialJointRotations[endEffector] = joints[endEffector].localRotation;

        // Remaining joints

        for (int i = endEffector - 1; i >= 0; i--)
        {
            chainLength += (linkLengths[i] = (initialLinkDirections[i] = joints[i + 1].localPosition - joints[i].localPosition).magnitude);
            initialJointRotations[i] = joints[i].localRotation;
        }

        /** Solve Cycle **/

        // First, we check to see it the target is within reach of the chain.
        // If not: We extend the chain along a straight line to it's max in the direction of the target.
        // If so:  We solve normally, traversing back and forth through the chain.
        if ((target.localPosition - root.localPosition).sqrMagnitude > chainLength * chainLength)
        {
            Vector3 targetDirection = (target.localPosition - root.localPosition).normalized;
            for (int i = 1; i < jointQty; i++)
            {
                jointPositions[i] = jointPositions[i - 1] + targetDirection * linkLengths[i - 1];
            }
        }
        else
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                /* Backwards Solve */

                // set end effector first:
                jointPositions[endEffector] = target.localPosition;

                // then loop through remaining joints:
                for (int i = endEffector - 1; i >= 0; i--)
                {
                    jointPositions[i] = jointPositions[i + 1] + (jointPositions[i] - jointPositions[i + 1]).normalized * linkLengths[i];

                    Vector3 currentLinkDirection = jointPositions[i + 1] - jointPositions[i];
                    float angleDeltaX = Vector3.SignedAngle(currentLinkDirection, initialLinkDirections[i], Vector3.left);

                    if (angleDeltaX > rotationConstraints[i, Xpos] || angleDeltaX < rotationConstraints[i, Xneg])
                    {
                        if (angleDeltaX > rotationConstraints[i, Xpos])
                        {
                            angleDeltaX = rotationConstraints[i, Xpos];
                        }
                        else
                        {
                            angleDeltaX = rotationConstraints[i, Xneg];
                        }

                        Vector3 perp = Vector3.Cross(Vector3.left, initialLinkDirections[i]);
                        Vector3 e1 = initialLinkDirections[i].normalized;
                        Vector3 e2 = perp.normalized;
                        currentLinkDirection = initialLinkDirections[i].magnitude * (Mathf.Cos(angleDeltaX) * e1 + Mathf.Sin(angleDeltaX) * e2);
                        jointPositions[i] = jointPositions[i + 1] - currentLinkDirection;
                    }

                    // TEMPORARY //

                    currentLinkDirection = jointPositions[i + 1] - jointPositions[i];
                    float angleDeltaY = Vector3.SignedAngle(currentLinkDirection, initialLinkDirections[i], Vector3.up);
                    if (angleDeltaY != 0)
                    {
                        angleDeltaY = 0;

                        Vector3 perp = Vector3.Cross(Vector3.up, initialLinkDirections[i]);
                        Vector3 e1 = initialLinkDirections[i].normalized;
                        Vector3 e2 = perp.normalized;
                        currentLinkDirection = initialLinkDirections[i].magnitude * (Mathf.Cos(angleDeltaY) * e1 + Mathf.Sin(angleDeltaY) * e2);
                        jointPositions[i] = jointPositions[i + 1] - currentLinkDirection;
                    }

                    currentLinkDirection = jointPositions[i + 1] - jointPositions[i];
                    float angleDeltaZ = Vector3.SignedAngle(currentLinkDirection, initialLinkDirections[i], Vector3.forward);
                    if (angleDeltaZ != 0)
                    {
                        angleDeltaZ = 0;

                        Vector3 perp = Vector3.Cross(Vector3.forward, initialLinkDirections[i]);
                        Vector3 e1 = initialLinkDirections[i].normalized;
                        Vector3 e2 = perp.normalized;
                        currentLinkDirection = initialLinkDirections[i].magnitude * (Mathf.Cos(angleDeltaZ) * e1 + Mathf.Sin(angleDeltaZ) * e2);
                        jointPositions[i] = jointPositions[i + 1] - currentLinkDirection;
                    }
                }

                /* Forward Solve */

                // re-anchor root
                jointPositions[0] = root.localPosition;

                // now loop through remaining joints
                for (int i = 0; i < endEffector; i++)
                {
                    jointPositions[i + 1] = jointPositions[i] + (jointPositions[i + 1] - jointPositions[i]).normalized * linkLengths[i];
                }

                /* Delta Check:  Are We Close Enough? */
                if ((jointPositions[endEffector] - target.localPosition).sqrMagnitude < maxDelta * maxDelta)
                {
                    break;
                }
            } // end for iterations
        } // end else (full solve)

        // set position and rotation, assign to transforms
        joints[endEffector].localPosition = jointPositions[endEffector];

        for (int i = 0; i < endEffector; i++)
        {
            Quaternion rotationDelta = Quaternion.FromToRotation(initialLinkDirections[i], jointPositions[i + 1] - jointPositions[i]);
            joints[i].localRotation *= rotationDelta;
            joints[i].localPosition = jointPositions[i];
        }

    } // end SolveChain()
}