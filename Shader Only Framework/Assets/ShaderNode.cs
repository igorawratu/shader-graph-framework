using UnityEngine;
using System;
using System.Collections.Generic;

class ShaderNode
{
    public const uint MAX_WIDTH = 4096;
    public const uint MAX_HEIGHT = 4096;

    private Shader shader_ = null;
    private Material material_ = null;
    private RenderTexture [] output_ = null;
    private uint curr_texture_ = 0;

    private Dictionary<string, ShaderNode> predecessors_;
    private Dictionary<string, Texture> input_textures_;

    private ulong last_frame_num_ = 0;

    public RenderTexture OutputTexture
    {
        get { return output_[curr_texture_]; }
    }

    public ShaderNode(string shader_name, uint width, uint height)
    {
        shader_ = Shader.Find(shader_name);
        if(shader_ == null)
        {
            throw new Exception("Unable to find shader " + shader_);
        }

        if(width > MAX_WIDTH || height > MAX_HEIGHT || width == 0 || height == 0)
        {
            throw new Exception("Shader node output dimensions invalid");
        }

        output_ = new RenderTexture[2];
        for(int i = 0; i < 2; ++i)
        {
            output_[i] = new RenderTexture((int)width, (int)height, 0);
        }

        predecessors_ = new Dictionary<string, ShaderNode>();
        input_textures_ = new Dictionary<string, Texture>();

        material_ = new Material(shader_);
    }

    public void Release()
    {
        if(output_ != null)
        {
            for(int i = 0; i < output_.Length; ++i)
            {
                if(output_[i] != null)
                {
                    output_[i].Release();
                }
            }
            output_ = null;
        }

        if(material_ != null)
        {
            Material.Destroy(material_);
            material_ = null;
        }
    }

    public void SetTexture(Texture texture, string input_name)
    {
        input_textures_[input_name] = texture;
    }

    public void SetPredecessor(ShaderNode predecessor, string input_name)
    {
        predecessors_[input_name] = predecessor;
    }

    public void Execute()
    {
        if((ulong)Time.frameCount == last_frame_num_)
        {
            return;
        }

        last_frame_num_ = (ulong)Time.frameCount;

        foreach (var predecessor in predecessors_)
        {
            if (predecessor.Value != this)
            {
                predecessor.Value.Execute();
                material_.SetTexture(predecessor.Key, predecessor.Value.OutputTexture);
            }
        }
        
        foreach(var texture in input_textures_)
        {
            material_.SetTexture(texture.Key, texture.Value);
        }

        Texture main = null;

        if (predecessors_.ContainsKey("_MainTex"))
        {
            main = predecessors_["_MainTex"].OutputTexture;
        }
        else if(input_textures_.ContainsKey("_MainTex"))
        {
            main = input_textures_["_MainTex"];
        }

        material_.SetFloat("_t", Time.time);

        curr_texture_ = (curr_texture_ + 1) % 2;

        Graphics.Blit(main, output_[curr_texture_], material_);
    }
}
