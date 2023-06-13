using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRAvatarShaderSwap : MonoBehaviour
{
    private Shader m_urpShader;
    [SerializeField] private MeshRenderer[] m_renderers;

    public void OnStart()
    {
        m_urpShader = Shader.Find("Universal Render Pipeline/Lit");
        m_renderers = transform.Find("LOD0").GetComponentsInChildren<MeshRenderer>(true);
    }

    [ContextMenu("Convert shaders")]
    public void ConvertShaders()
    {
        foreach(MeshRenderer r in m_renderers)
        {
            Material[] mats = r.materials;
            List<Material> newMats = new List<Material>();
            foreach (var m in mats)
            {
                Texture diff = m.mainTexture;
                Material newM = new Material(m_urpShader);
                newM.mainTexture = diff;
                newMats.Add(newM);
            }
            r.materials = newMats.ToArray();
        }


    }
}
