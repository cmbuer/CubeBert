/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBertLegMove : MonoBehaviour
{
    public Transform target;

    public float rootXpos;
    public float rootXneg;
    public float rootYpos;
    public float rootYneg;
    public float rootZpos;
    public float rootZneg;

    public float kneeXpos;
    public float kneeXneg;
    public float kneeYpos;
    public float kneeYneg;
    public float kneeZpos;
    public float kneeZneg;

    public float footXpos;
    public float footXneg;
    public float footYpos;
    public float footYneg;
    public float footZpos;
    public float footZneg;

    private struct JointCache
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private FabrikChain fabrikChain;
    private Transform[] joints;
    private JointCache[] jointCaches;

    private void Awake()
    {
        fabrikChain = new FabrikChain();
        joints = new Transform[transform.childCount];
        jointCaches = new JointCache[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            // The joints transform array will be fed to the IK solver
            joints[i] = transform.GetChild(i);
            // Cache the original joint transforms for use as a reference later
            jointCaches[i] = new JointCache {
                position = joints[i].localPosition,
                rotation = joints[i].localRotation
            };
        }
    }

    public void RestoreDefaults()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].localPosition = jointCaches[i].position;
            joints[i].localRotation = jointCaches[i].rotation;
        }
    }

    public void Solve()
    {
        /* Always restore the default joint transforms before performing
         * the IK solve. Otherwise the joint transforms will slowly start
         * to accumulate error and drift apart. */
        RestoreDefaults();

        float[,] constraintsArray = { { rootXpos, rootXneg, rootYpos, rootYneg, rootZpos, rootZneg },
                                      { kneeXpos, kneeXneg, kneeYpos, kneeYneg, kneeZpos, kneeZneg },
                                      { footXpos, footXneg, footYpos, footYneg, footZpos, footZneg } };

        fabrikChain.SolveChain(joints, target, constraintsArray, 30);
    }
} 