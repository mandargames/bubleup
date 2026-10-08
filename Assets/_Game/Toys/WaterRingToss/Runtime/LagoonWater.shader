Shader "PocketToys/LagoonWater"
{
    Properties { _Motion ("Motion", Float) = 1 _MainTex ("Coral lagoon", 2D) = "white" {} }
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
            sampler2D _MainTex;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float2 p=i.uv; float t=_Time.y*.14*_Motion;
                float2 ripple=float2(sin(p.y*21+t*3),cos(p.x*18+t*2))*.0018*_Motion;
                float3 color=tex2D(_MainTex,saturate(p+ripple)).rgb;
                float wave=sin(p.x*22+sin(p.y*15+t)*1.5+t)*sin(p.y*19+cos(p.x*12-t));
                float caustic=pow(saturate(wave*.5+.5),12);
                color+=float3(.12,.25,.21)*caustic*.14;
                float shafts=pow(saturate(sin((p.x+p.y*.3)*17+t)*.5+.5),8)*p.y*.055;
                color+=float3(.55,.9,.85)*shafts;
                float edge=smoothstep(0,.09,p.x)*smoothstep(0,.09,1-p.x)*smoothstep(0,.06,p.y)*smoothstep(0,.06,1-p.y);
                color*=lerp(.72,1,edge);
                return fixed4(color,1);
            }
            ENDCG
        }
    }
}
