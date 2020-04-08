using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class ChainIKConstraintTests {

    const float k_Epsilon = 1e-4f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public ChainIKConstraint constraint;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var chainIKGO = new GameObject("chainIK");
        var chainIK = chainIKGO.AddComponent<ChainIKConstraint>();
        chainIK.Reset();

        chainIKGO.transform.parent = data.rigData.rigGO.transform;

        chainIK.data.root = data.rigData.hipsGO.transform.Find("Chest");
        Assert.IsNotNull(chainIK.data.root, "Could not find root transform");

        chainIK.data.tip = chainIK.data.root.transform.Find("LeftArm/LeftForeArm/LeftHand");
        Assert.IsNotNull(chainIK.data.tip, "Could not find tip transform");

        var targetGO = new GameObject ("target");
        targetGO.transform.parent = chainIKGO.transform;

        chainIK.data.target = targetGO.transform;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();
        targetGO.transform.position = chainIK.data.tip.position;

        data.constraint = chainIK;

        return data;
    }

    [UnityTest]
    public IEnumerator ChainIKConstraint_FollowsTarget()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var target = constraint.data.target;
        var tip = constraint.data.tip;
        var root = constraint.data.root;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

        for (int i = 0; i < 5; ++i)
        {
            target.position += new Vector3(0f, 0.1f, 0f);
            yield return RuntimeRiggingTestFixture.YieldTwoFrames();

            Vector3 rootToTip = (tip.position - root.position).normalized;
            Vector3 rootToTarget = (target.position - root.position).normalized;

            Assert.That(rootToTarget, Is.EqualTo(rootToTip).Using(positionComparer), String.Format("Expected rootToTip to be {0}, but was {1}", rootToTip, rootToTarget));
        }
    }

    [UnityTest]
    public IEnumerator ChainIKConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        Transform[] chain = ConstraintsUtils.ExtractChain(constraint.data.root, constraint.data.tip);

        // Chain with no constraint.
        Vector3[] bindPoseChain = chain.Select(transform => transform.position).ToArray();

        var target = constraint.data.target;
        target.position += new Vector3(0f, 0.5f, 0f);

        yield return null;

        // Chain with ChainIK constraint.
        Vector3[] weightedChain = chain.Select(transform => transform.position).ToArray();

        // In-between chains.
        List<Vector3[]> inBetweenChains = new List<Vector3[]>();
        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            inBetweenChains.Add(chain.Select(transform => transform.position).ToArray());
        }

        var floatComparer = new RuntimeRiggingTestFixture.FloatEqualityComparer(k_Epsilon);

        for (int i = 0; i <= 5; ++i)
        {
            Vector3[] prevChain = (i > 0) ? inBetweenChains[i - 1] : bindPoseChain;
            Vector3[] currentChain = inBetweenChains[i];
            Vector3[] nextChain = (i < 5) ? inBetweenChains[i + 1] : weightedChain;

            for (int j = 0; j < bindPoseChain.Length - 1; ++j)
            {
                Vector2 dir1 = prevChain[j + 1] - prevChain[j];
                Vector2 dir2 = currentChain[j + 1] - currentChain[j];
                Vector2 dir3 = nextChain[j + 1] - nextChain[j];

                float maxAngle = Vector2.Angle(dir1, dir3);
                float angle = Vector2.Angle(dir1, dir2);

                Assert.That(angle, Is.GreaterThanOrEqualTo(0f).Using(floatComparer));
                Assert.That(angle, Is.LessThanOrEqualTo(maxAngle).Using(floatComparer));
            }
        }
    }
}