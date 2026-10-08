Shader "PocketToys/OceanBackdrop"
{
    Properties { _Motion ("Motion", Float) = 1 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            float _Motion;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float2 p=i.uv;
                float t=_Time.y*.045*_Motion;
                float glow=exp(-dot((p-float2(.48,.60))*float2(2.5,1.6),(p-float2(.48,.60))*float2(2.5,1.6))*2);
                float3 c=lerp(float3(.018,.060,.105),float3(.045,.27,.31),glow);
                float wave=sin(p.x*9+p.y*5+t)*.023+sin(p.x*15-p.y*7-t)*.009;
                c+=float3(.2,.68,.62)*wave;
                float ribbon=pow(saturate(1-abs(p.y-.57-sin(p.x*5+t)*.10)*30),3);
                c+=float3(.055,.20,.19)*ribbon*.16;
                float grain=frac(sin(dot(p,float2(127.1,311.7)))*43758.5453)-.5;
                c+=grain*.007;
                return fixed4(c,1);
            }
            ENDCG
        }
    }
}
