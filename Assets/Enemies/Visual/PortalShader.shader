Shader "Custom/PortalShader"
{
    Properties
    {
        _Color ("Portal Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 2
        _InnerRadius ("Inner Radius", Range(0, 0.5)) = 0.1
        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0, 0.5)) = 0.1
        _Rotation ("Rotation Speed", Float) = 1
        _NoiseScale ("Noise Scale", Float) = 10
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.3
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float4 _Color;
            float _EmissionStrength;
            float _InnerRadius;
            float _OuterRadius;
            float _EdgeSoftness;
            float _Rotation;
            float _NoiseScale;
            float _NoiseStrength;
            
            // Simple noise function
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Center UV coordinates
                float2 center = i.uv - 0.5;
                
                // Rotate UV coordinates
                float angle = _Time.y * _Rotation;
                float s = sin(angle);
                float c = cos(angle);
                center = float2(
                    center.x * c - center.y * s,
                    center.x * s + center.y * c
                );
                
                // Distance from center
                float dist = length(center);
                
                // Add animated noise
                float n = noise(center * _NoiseScale + _Time.y);
                dist += n * _NoiseStrength * 0.1;
                
                // Create ring shape
                float ring = smoothstep(_InnerRadius - _EdgeSoftness, _InnerRadius, dist) * 
                            (1.0 - smoothstep(_OuterRadius, _OuterRadius + _EdgeSoftness, dist));
                
                // Pulsating effect
                ring *= 0.8 + 0.2 * sin(_Time.y * 3.0);
                
                // Swirling energy effect
                float swirl = sin(atan2(center.y, center.x) * 5.0 + _Time.y * 2.0) * 0.5 + 0.5;
                ring *= 0.7 + 0.3 * swirl;
                
                // Apply color with emission
                float4 col = _Color * _EmissionStrength;
                col.a = ring;
                
                return col;
            }
            ENDCG
        }
    }
}