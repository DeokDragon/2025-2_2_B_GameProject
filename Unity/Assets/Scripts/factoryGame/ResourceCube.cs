using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCube : MonoBehaviour
{
    public ResourceType type;

    public void Initalize(ResourceType resourcetype)
    {
        type = resourcetype;
        Renderer renderer = GetComponent<Renderer>();

        if (resourcetype == ResourceType.Wood) renderer.material.color = new Color(0.6f, 0.3f, 0.1f);
        if (resourcetype == ResourceType.Metal) renderer.material.color = Color.gray;
    }
}
