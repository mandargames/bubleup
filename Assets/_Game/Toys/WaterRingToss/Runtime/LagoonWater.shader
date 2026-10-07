Shader "PocketToys/LagoonWater"
{
    Properties { _Motion ("Motion", Float) = 1 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
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
                float2 p=i.uv; float t=_Time.y*.14*_Motion;
                float3 deep=float3(.035,.23,.31), shallow=float3(.17,.57,.59);
                float3 color=lerp(deep,shallow,p.y*.74+.10);
                float wave=sin(p.x*22+sin(p.y*15+t)*1.5+t)*sin(p.y*19+cos(p.x*12-t));
                float caustic=pow(saturate(wave*.5+.5),12);
                color+=float3(.12,.25,.21)*caustic*.27;
                float shafts=pow(saturate(sin((p.x+p.y*.3)*17+t)*.5+.5),8)*p.y*.035;
                color+=float3(.55,.9,.85)*shafts;
                float edge=smoothstep(0,.09,p.x)*smoothstep(0,.09,1-p.x)*smoothstep(0,.06,p.y)*smoothstep(0,.06,1-p.y);
                color*=lerp(.57,1,edge);
                return fixed4(color,1);
            }
            ENDCG
        }
    }
}
