Shader "PocketToys/LagoonWater"
{
    Properties
    {
        _Motion ("Motion", Float) = 1 _MainTex ("Coral lagoon", 2D) = "white" {}
        _Biome ("Lagoon / ice / lava", Float) = 0 _Frost ("Freeze progress", Float) = 0
        _WaterTime ("Paused gameplay clock", Float) = 0
        _VentSide ("Active vent side", Float) = 0 _VentLift ("Vent active", Float) = 0 _VentWarning ("Vent warning", Float) = 0
        [HideInInspector] _TankArea ("Tank origin and size", Vector) = (-2.8,-2.45,5.6,7.2)
        [HideInInspector] _CurrentBounds0 ("Current bounds 0", Vector) = (0,0,1,1)
        [HideInInspector] _CurrentBounds1 ("Current bounds 1", Vector) = (0,0,1,1)
        [HideInInspector] _CurrentBounds2 ("Current bounds 2", Vector) = (0,0,1,1)
        [HideInInspector] _CurrentBounds3 ("Current bounds 3", Vector) = (0,0,1,1)
        [HideInInspector] _CurrentFlow0 ("Current direction, state, visible 0", Vector) = (0,0,0,0)
        [HideInInspector] _CurrentFlow1 ("Current direction, state, visible 1", Vector) = (0,0,0,0)
        [HideInInspector] _CurrentFlow2 ("Current direction, state, visible 2", Vector) = (0,0,0,0)
        [HideInInspector] _CurrentFlow3 ("Current direction, state, visible 3", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            float _Motion, _Biome, _Frost, _WaterTime, _VentSide, _VentLift, _VentWarning;
            float4 _TankArea, _CurrentBounds0, _CurrentBounds1, _CurrentBounds2, _CurrentBounds3;
            float4 _CurrentFlow0, _CurrentFlow1, _CurrentFlow2, _CurrentFlow3;
            sampler2D _MainTex;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            float3 currentOverlay(float3 color, float2 world, float4 bounds, float4 flow)
            {
                if (flow.w < .5) return color;
                float2 relative=world-bounds.xy;
                float2 normalized=abs(relative)/max(bounds.zw,float2(.001,.001));
                float edge=max(normalized.x,normalized.y);
                // Matches CurrentDefinition.AccelerationAt: no force beyond this outline.
                float footprint=saturate((1-edge)/.25);
                float border=(1-smoothstep(.015,.055,abs(1-edge)))*step(edge,1);
                float warning=step(.5,flow.z), active=step(1.5,flow.z);
                float2 axis=flow.xy, crossAxis=float2(-axis.y,axis.x);
                float along=dot(relative,axis)-_WaterTime*.28*_Motion*active;
                float across=dot(relative,crossAxis);
                float a=(frac(along/.62+.5)-.5)*.62;
                float b=(frac(across/.50+.5)-.5)*.50;
                float arrow=(1-smoothstep(.025,.05,abs(a+abs(b)*.65-.12)))*(1-smoothstep(.14,.19,abs(b)));
                float3 tint=_Biome>1.5 ? float3(1,.58,.18) : float3(.46,.94,.85);
                color=lerp(color,tint,border*(.16+warning*.13)+footprint*(.035+warning*.045+active*.035));
                // Static chevrons and state contrast remain when reduced motion is on.
                return lerp(color,lerp(tint,float3(.96,.96,.82),active),arrow*footprint*warning*(.24+active*.28));
            }
            fixed4 frag(v2f i):SV_Target
            {
                float2 p=i.uv; float t=_WaterTime*.14*_Motion;
                float2 ripple=float2(sin(p.y*21+t*3),cos(p.x*18+t*2))*.0018*_Motion;
                float3 color=tex2D(_MainTex,saturate(p+ripple)).rgb;
                float wave=sin(p.x*22+sin(p.y*15+t)*1.5+t)*sin(p.y*19+cos(p.x*12-t));
                float caustic=pow(saturate(wave*.5+.5),12);
                color+=float3(.12,.25,.21)*caustic*.14;
                float shafts=pow(saturate(sin((p.x+p.y*.3)*17+t)*.5+.5),8)*p.y*.055;
                color+=float3(.55,.9,.85)*shafts;
                // Distinct quiet backgrounds: ice and basalt do not tint the coral
                // painting into an unrelated scene. Gameplay objects stay in front.
                if (_Biome > .5 && _Biome < 1.5)
                {
                    color=lerp(float3(.035,.12,.23),float3(.22,.52,.62),p.y);
                    color+=float3(.15,.23,.28)*(shafts+caustic*.055);
                    float border=min(min(p.x,1-p.x),min(p.y,1-p.y));
                    float frost=saturate((_Frost*.22-border)*7);
                    float veins=pow(saturate(1-abs(sin(p.x*48+p.y*29+sin(p.y*17)))),18);
                    color=lerp(color,float3(.65,.83,.88),frost*(.42+veins*.32));
                    color+=float3(.13,.19,.22)*veins*frost;
                }
                if (_Biome > 1.5)
                {
                    float stone=1-smoothstep(.09,.18,min(p.x,1-p.x)+sin(p.y*23)*.015);
                    stone=max(stone,1-smoothstep(.07,.13,p.y+sin(p.x*26)*.012));
                    float seams=pow(saturate(sin(p.x*43+sin(p.y*31)*1.6)*sin(p.y*37)*.5+.5),14);
                    color=lerp(float3(.23,.065,.025),float3(.065,.07,.11),p.y);
                    color=lerp(color,float3(.045,.045,.06)+float3(.25,.065,.005)*seams,stone);
                    float ventX=lerp(.152,.848,_VentSide);
                    float column=exp(-pow((p.x-ventX)/.10,2));
                    float flow=.65+.35*sin(p.y*32-_WaterTime*4*_Motion);
                    color+=column*(_VentLift*.28*flow+_VentWarning*.10)*float3(1,.42,.075)*(1-p.y*.6);
                }
                float edge=smoothstep(0,.09,p.x)*smoothstep(0,.09,1-p.x)*smoothstep(0,.06,p.y)*smoothstep(0,.06,1-p.y);
                color*=lerp(.72,1,edge);
                float2 world=_TankArea.xy+p*_TankArea.zw;
                color=currentOverlay(color,world,_CurrentBounds0,_CurrentFlow0);
                color=currentOverlay(color,world,_CurrentBounds1,_CurrentFlow1);
                color=currentOverlay(color,world,_CurrentBounds2,_CurrentFlow2);
                color=currentOverlay(color,world,_CurrentBounds3,_CurrentFlow3);
                return fixed4(color,1);
            }
            ENDCG
        }
    }
}
