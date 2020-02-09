using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GPUParticleAttraction
{
    public class GPUParticleAttraction : MonoBehaviour
    {
        [System.Serializable]
        public struct ParticleData
        {
            public Vector2 position;
            public Vector2 velocity;
            public Color color;
            public float size;
        }

        const int THREAD_NUM = 512;

        #region particle parameters
        // BUGS: 
        // if MaxParticleNum is not a multiple of THREAD_NUM, some particles don't move
        [Range(10000, 1000000)]
        public int MaxParticleNum = 524288;
        [Range(0.01f, 0.1f)]
        public float ParticleSize = 0.1f;
        public float AttractStrength = 10f;
        public float MaxSpeed = 5.0f;
        public float AvoidWallStrength = 10.0f;
        public Vector3 WallCenter = Vector3.zero;
        public Vector3 WallSize = new Vector3(50, 50, 5);
        #endregion

        public ComputeShader ParticleAttractCS;

        private ComputeBuffer _attractionBuffer;
        private ComputeBuffer _particleDataBuffer;
        private int attractKernel;
        private int updateKernel;
        private int threadGroupSize;
        private float camDepth;
        private float elapsedTime = 0f;


        public ComputeBuffer GetParticleDataBuffer()
        {
            return _particleDataBuffer != null ? _particleDataBuffer : null;
        }

        public int GetMaxParticleNum()
        {
            return MaxParticleNum;
        }

        public Vector3 GetSimulationAreaCenter()
        {
            return WallCenter;
        }

        public Vector3 GetSimulationAreaSize()
        {
            return WallSize;
        }

        private void Start()
        {
            InitBuffer();

            // set kernel ID
            attractKernel = ParticleAttractCS.FindKernel("Attract");
            updateKernel = ParticleAttractCS.FindKernel("Update");

            // set thread group size
            threadGroupSize = Mathf.CeilToInt(MaxParticleNum / THREAD_NUM);

            // set camera depth which is necessary for getting the right mouse position
            camDepth = Mathf.Abs(Camera.main.transform.position.z - this.transform.position.z);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                var pos = Input.mousePosition;
                pos.z = camDepth;
                pos = Camera.main.ScreenToWorldPoint(pos);

                AttractParticle(pos);
            }

            elapsedTime += Time.deltaTime;
            UpdateParticle();
        }

        private void OnDisable()
        {
            ReleaseBuffer();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(WallCenter, WallSize);
        }

        /// <summary>
        /// Initialize compute buffers
        /// </summary>
        private void InitBuffer()
        {
            _attractionBuffer   = new ComputeBuffer(MaxParticleNum, Marshal.SizeOf(typeof(Vector2)));
            _particleDataBuffer = new ComputeBuffer(MaxParticleNum, Marshal.SizeOf(typeof(ParticleData)));

            var attraction = new Vector2[MaxParticleNum];
            var particles  = new ParticleData[MaxParticleNum];
            for (int i = 0; i < MaxParticleNum; i++)
            {
                attraction[i] = Vector2.zero;

                particles[i].position = Random.insideUnitCircle * 5.0f;
                particles[i].velocity = Random.insideUnitCircle * 0.1f;
                particles[i].color    = Color.white;
                particles[i].size     = ParticleSize;
            }
            _attractionBuffer.SetData(attraction);
            _particleDataBuffer.SetData(particles);

            attraction = null;
            particles  = null;
        }

        /// <summary>
        /// execute Attract kernel
        /// </summary>
        private void AttractParticle(Vector2 mousePos)
        {
            ComputeShader cs = ParticleAttractCS;

            cs.SetFloat("_AttractStrength", AttractStrength);
            cs.SetFloat("_MaxSpeed", MaxSpeed);
            cs.SetVector("_MousePos", mousePos);
            cs.SetBuffer(attractKernel, "_AttractionBuffer", _attractionBuffer);
            cs.SetBuffer(attractKernel, "_ParticleDataBufferRead", _particleDataBuffer);
            cs.Dispatch(attractKernel, threadGroupSize, 1, 1);
        }

        /// <summary>
        /// execute Update kernel
        /// </summary>
        private void UpdateParticle()
        {
            ComputeShader cs = ParticleAttractCS;

            cs.SetFloat("_DeltaTime", Time.deltaTime);
            cs.SetFloat("_ElapsedTime", elapsedTime);
            cs.SetFloat("_AvoidWallStrength", AvoidWallStrength);
            cs.SetVector("_WallCenter", WallCenter);
            cs.SetVector("_WallSize", WallSize);
            cs.SetBuffer(updateKernel, "_AttractionBuffer", _attractionBuffer);
            cs.SetBuffer(updateKernel, "_ParticleDataBufferWrite", _particleDataBuffer);
            cs.Dispatch(updateKernel, threadGroupSize, 1, 1);
        }

        /// <summary>
        /// Release used compute buffers
        /// </summary>
        private void ReleaseBuffer()
        {
            new[] { _attractionBuffer, _particleDataBuffer }.ToList().ForEach( buffer =>
            {
                if (buffer != null)
                    buffer.Release();
                buffer = null;
            });
        }
    }
}