using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticleAttraction
{
    public class ParticleRender : MonoBehaviour
    {
        public GPUParticleAttraction GPUParticleScript;

        public Mesh instanceMesh;
        public Material instanceMat;

        private ComputeBuffer argsBuffer;
        // parametes for GPU Instancing
        // 1. index count per instance
        // 2. instance count 
        // 3. start index location 
        // 4. base vertex location 
        // 5. start instance location
        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        private void Start()
        {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        private void Update()
        {
            RenderInstances();
        }

        private void OnDisable()
        {
            ReleaseBuffer();
        }

        private void RenderInstances()
        {
            if (instanceMesh == null || instanceMat == null || !SystemInfo.supportsInstancing)
                return;

            args[0] = instanceMesh.GetIndexCount(0);
            args[1] = (uint)GPUParticleScript.GetMaxParticleNum();
            argsBuffer.SetData(args);

            instanceMat.SetBuffer("_ParticleDataBuffer", GPUParticleScript.GetParticleDataBuffer());
            Graphics.DrawMeshInstancedIndirect
            (
                instanceMesh,
                0,
                instanceMat,
                new Bounds(GPUParticleScript.GetSimulationAreaCenter(), GPUParticleScript.GetSimulationAreaSize()),
                argsBuffer
            );
        }

        private void ReleaseBuffer()
        {
            if (argsBuffer != null)
                argsBuffer.Release();
            argsBuffer = null;
        }
    }
}