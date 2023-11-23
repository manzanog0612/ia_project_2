using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpggleLightsAndMeshes : MonoBehaviour
{
    private MeshRenderer[] meshRenderers;
    private Light[] lights;
    public bool toggle = true;
    private bool lastToggle = true;

    void Update()
    {
        if (lastToggle != toggle)
        {
            Toggle(toggle);
            lastToggle = toggle;
        }
    }

    private void Toggle(bool toggle)
    {
        if (meshRenderers == null)
        { 
            meshRenderers = FindObjectsOfType<MeshRenderer>(); 
        }

        if (lights == null)
        {
            lights = FindObjectsOfType<Light>();
        }

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = toggle;
        }

        for (int i = 0; i < lights.Length; i++) 
        {
            lights[i].enabled = toggle;
        }
    }
}
