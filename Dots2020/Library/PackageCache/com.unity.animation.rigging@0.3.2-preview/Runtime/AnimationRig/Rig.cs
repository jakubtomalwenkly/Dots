using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging
{
    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Setup/Rig")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@latest?preview=1&subfolder=/manual/index.html")]
    public class Rig : MonoBehaviour, IRigEffectorHolder
    {
        [SerializeField, Range(0f, 1f)]
        protected float m_Weight = 1f;

        public float weight { get => m_Weight; set => m_Weight = Mathf.Clamp01(value); }

#if UNITY_EDITOR
        [SerializeField] private List<RigEffectorData> m_Effectors = new List<RigEffectorData>();
        public IEnumerable<RigEffectorData> effectors { get => m_Effectors; }

        public void AddEffector(Transform transform)
        {
            var effector = new RigEffectorData();
            effector.Initialize(transform, RigEffectorData.defaultStyle);

            m_Effectors.Add(effector);
        }

        public void RemoveEffector(Transform transform)
        {
            m_Effectors.RemoveAll((RigEffectorData data) => data.transform == transform);
        }

        public bool ContainsEffector(Transform transform)
        {
            return m_Effectors.Exists((RigEffectorData data) => data.transform == transform);
        }
#endif
    }
}