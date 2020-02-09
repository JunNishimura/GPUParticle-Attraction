Shader "GPUParticleAttraction/ParticleRender"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "ParticleDataCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            StructuredBuffer<ParticleData> _ParticleDataBuffer;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);

                ParticleData p = _ParticleDataBuffer[instanceID];

                // apply color 
                o.color = p.color;

                // apply scaling and transformation
                float4x4 object2world = (float4x4)0;
                object2world._11_22_33_44 = float4(p.size, p.size, p.size, 1.0);
                object2world._14_24 += p.position;
                v.vertex = mul(object2world, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
