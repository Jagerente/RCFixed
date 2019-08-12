using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Xffect")]
public class Xffect : MonoBehaviour
{
    private List<EffectLayer> EflList = new List<EffectLayer>();
    protected float ElapsedTime;
    public float LifeTime = -1f;
    private Dictionary<string, VertexPool> MatDic = new Dictionary<string, VertexPool>();

    public void Active()
    {
        foreach (Transform transform in base.transform)
        {
            transform.gameObject.SetActive(true);
        }
        base.gameObject.SetActive(true);
        this.ElapsedTime = 0f;
    }


    private void Awake()
    {
        this.Initialize();
    }

    public void DeActive()
    {
        foreach (Transform transform in base.transform)
        {
            transform.gameObject.SetActive(false);
        }
        base.gameObject.SetActive(false);
    }


    // Xffect
    public void Initialize()
    {
        if (this.EflList.Count > 0)
        {
            return;
        }
        foreach (Transform transform in base.transform)
        {
            EffectLayer effectLayer = (EffectLayer)transform.GetComponent(typeof(EffectLayer));
            if (!(effectLayer == null) && !(effectLayer.Material == null))
            {
                Material material = effectLayer.Material;
                this.EflList.Add(effectLayer);
                Transform transform2 = base.transform.Find("mesh " + material.name);
                if (transform2 != null)
                {
                    MeshFilter meshFilter = (MeshFilter)transform2.GetComponent(typeof(MeshFilter));
                    MeshRenderer meshRenderer = (MeshRenderer)transform2.GetComponent(typeof(MeshRenderer));
                    meshFilter.mesh.Clear();
                    this.MatDic[material.name] = new VertexPool(meshFilter.mesh, material);
                }
                if (!this.MatDic.ContainsKey(material.name))
                {
                    GameObject gameObject = new GameObject("mesh " + material.name);
                    gameObject.transform.parent = base.transform;
                    gameObject.AddComponent("MeshFilter");
                    gameObject.AddComponent("MeshRenderer");
                    MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
                    MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent(typeof(MeshRenderer));
                    meshRenderer.castShadows = false;
                    meshRenderer.receiveShadows = false;
                    meshRenderer.renderer.material = material;
                    this.MatDic[material.name] = new VertexPool(meshFilter.mesh, material);
                }
            }
        }
        foreach (EffectLayer current in this.EflList)
        {
            current.Vertexpool = this.MatDic[current.Material.name];
        }
    }


    private void LateUpdate()
    {
        foreach (KeyValuePair<string, VertexPool> pair in this.MatDic)
        {
            pair.Value.LateUpdate();
        }
        if ((this.ElapsedTime > this.LifeTime) && (this.LifeTime >= 0f))
        {
            foreach (EffectLayer layer in this.EflList)
            {
                layer.Reset();
            }
            this.DeActive();
            this.ElapsedTime = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
    }

    public void SetClient(Transform client)
    {
        foreach (EffectLayer layer in this.EflList)
        {
            layer.ClientTransform = client;
        }
    }

    public void SetDirectionAxis(Vector3 axis)
    {
        foreach (EffectLayer layer in this.EflList)
        {
            layer.OriVelocityAxis = axis;
        }
    }

    public void SetEmitPosition(Vector3 pos)
    {
        foreach (EffectLayer layer in this.EflList)
        {
            layer.EmitPoint = pos;
        }
    }

    private void Start()
    {
        base.transform.position = Vector3.zero;
        base.transform.rotation = Quaternion.identity;
        base.transform.localScale = Vector3.one;
        foreach (Transform transform in base.transform)
        {
            transform.transform.position = Vector3.zero;
            transform.transform.rotation = Quaternion.identity;
            transform.transform.localScale = Vector3.one;
        }
        foreach (EffectLayer current in this.EflList)
        {
            current.StartCustom();
        }
    }


    private void Update()
    {
        this.ElapsedTime += Time.deltaTime;
        foreach (EffectLayer layer in this.EflList)
        {
            if (this.ElapsedTime > layer.StartTime)
            {
                layer.FixedUpdateCustom();
            }
        }
    }
}

