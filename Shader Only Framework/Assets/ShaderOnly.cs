using UnityEngine;
using System.Collections;

public class ShaderOnly : MonoBehaviour {
    private ShaderNode _finalNode;

    // Use this for initialization
    void Start() {
        _finalNode = new ShaderNode("test", 1920, 1080);
        _finalNode.SetPredecessor(_finalNode, "_MainTex");
    }

    // Update is called once per frame
    void Update() {
        
    }

    void OnDestroy()
    {
        _finalNode.Release();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        _finalNode.Execute();
        Graphics.Blit(_finalNode.OutputTexture, dest);
    }
}
