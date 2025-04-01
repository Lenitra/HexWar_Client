Shader "Custom/ScanReceiver"
{
    Properties
    {
        _MainTex ("Albedo (RGB) Alpha (A)", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpStrength ("Normal Intensity", Range(0, 50)) = 1.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        _ScanOrigin ("Scan Origin", Vector) = (0,0,0,0)
        _ScanRadius ("Scan Radius", Float) = 0
        _ScanWidth ("Scan Width", Float) = 1
        _ScanColor ("Scan Color", Color) = (0,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        float _BumpStrength;

        float4 _ScanOrigin;
        float _ScanRadius;
        float _ScanWidth;
        fixed4 _ScanColor;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            // Normal map avec intensité
            float3 normalTex = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Normal = normalize(float3(normalTex.xy * _BumpStrength, normalTex.z));

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            float dist = distance(IN.worldPos, _ScanOrigin.xyz);
            float wave = smoothstep(_ScanRadius - _ScanWidth, _ScanRadius, dist)
                       * (1.0 - smoothstep(_ScanRadius, _ScanRadius + _ScanWidth, dist));

            o.Emission = _ScanColor.rgb * wave;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
