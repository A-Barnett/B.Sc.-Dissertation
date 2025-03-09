Shader "Custom/TerrainShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _GrassColour ("Grass Colour", Color) = (1,1,1,1)
        _CliffColour ("Cliff Colour", Color) = (0,0,0,1)
        _SteepnessThreshold ("Steepness Threshold", Range(0, 90)) = 45
        _BlendWidth ("Blend Width", Range(0, 100)) = 1.0

    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma vertex vert

        #include "UnityCG.cginc"
         

            
        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 pos;
        };
        
        struct appdata {
        float4 vertex : POSITION;
        float3 texcoord : TEXCOORD0;
        };

        struct v2f {
        float4 pos : SV_POSITION;
        float4 uv : TEXCOORD0;
        float3 ray : TEXCOORD1;
        };
        
        sampler2D _MainTex;
        fixed4 _GrassColour;
        fixed4 _CliffColour;
        fixed4 _SnowColour;
        float _SteepnessThreshold;
        float _BlendWidth;
        float _SnowHeight;
        float _GrassHeight;
        
        
        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord;
            o.worldNormal = mul((float3x3)unity_WorldToObject, v.normal);
            o.pos = mul(unity_ObjectToWorld, v.vertex);
            
        }
        
         void surf (Input IN, inout SurfaceOutput o) {
            // Calculate the slope angle in degrees
            float slopeAngle = acos(dot(IN.worldNormal, float3(0, 1, 0))) * (180.0 / 3.14159265359);
            
            // Calculate blend factor based on steepness and blend width
            float blendFactor = 1.0 - smoothstep(_SteepnessThreshold - _BlendWidth, _SteepnessThreshold + _BlendWidth, slopeAngle);
            
            // Interpolate between grass, snow, and cliff colors based on height
            fixed4 finalColor = lerp(_CliffColour,_GrassColour, blendFactor);
        
            
            // Apply texture
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * finalColor.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

