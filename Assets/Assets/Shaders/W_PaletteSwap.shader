Shader "Wrestleverse/PaletteSwap"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OriginalColor0 ("Original Color 0", Color) = (1,1,1,1)
        _OriginalColor1 ("Original Color 1", Color) = (1,1,1,1)
        _OriginalColor2 ("Original Color 2", Color) = (1,1,1,1)
        _OriginalColor3 ("Original Color 3", Color) = (1,1,1,1)
        _OriginalColor4 ("Original Color 4", Color) = (1,1,1,1)
        _OriginalColor5 ("Original Color 5", Color) = (1,1,1,1)
        _OriginalColor6 ("Original Color 6", Color) = (1,1,1,1)
        _OriginalColor7 ("Original Color 7", Color) = (1,1,1,1)
        _TargetColor0 ("Target Color 0", Color) = (1,1,1,1)
        _TargetColor1 ("Target Color 1", Color) = (1,1,1,1)
        _TargetColor2 ("Target Color 2", Color) = (1,1,1,1)
        _TargetColor3 ("Target Color 3", Color) = (1,1,1,1)
        _TargetColor4 ("Target Color 4", Color) = (1,1,1,1)
        _TargetColor5 ("Target Color 5", Color) = (1,1,1,1)
        _TargetColor6 ("Target Color 6", Color) = (1,1,1,1)
        _TargetColor7 ("Target Color 7", Color) = (1,1,1,1)
        _SwapCount ("Swap Count", Float) = 1
        _Tolerance ("Color Match Tolerance", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _OriginalColor0;
            float4 _OriginalColor1;
            float4 _OriginalColor2;
            float4 _OriginalColor3;
            float4 _OriginalColor4;
            float4 _OriginalColor5;
            float4 _OriginalColor6;
            float4 _OriginalColor7;
            float4 _TargetColor0;
            float4 _TargetColor1;
            float4 _TargetColor2;
            float4 _TargetColor3;
            float4 _TargetColor4;
            float4 _TargetColor5;
            float4 _TargetColor6;
            float4 _TargetColor7;
            float _SwapCount;
            float _Tolerance;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
            struct v2f {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // RGB to HSV conversion
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            // HSV to RGB conversion
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord);
                float3 pixelHSV = rgb2hsv(c.rgb);
                for (int idx = 0; idx < (int)_SwapCount; idx++)
                {
                    float4 orig;
                    float4 targ;
                    if (idx == 0) { orig = _OriginalColor0; targ = _TargetColor0; }
                    else if (idx == 1) { orig = _OriginalColor1; targ = _TargetColor1; }
                    else if (idx == 2) { orig = _OriginalColor2; targ = _TargetColor2; }
                    else if (idx == 3) { orig = _OriginalColor3; targ = _TargetColor3; }
                    else if (idx == 4) { orig = _OriginalColor4; targ = _TargetColor4; }
                    else if (idx == 5) { orig = _OriginalColor5; targ = _TargetColor5; }
                    else if (idx == 6) { orig = _OriginalColor6; targ = _TargetColor6; }
                    else if (idx == 7) { orig = _OriginalColor7; targ = _TargetColor7; }
                    else { break; }

                    float3 originalHSV = rgb2hsv(orig.rgb);
                    float3 targetHSV = rgb2hsv(targ.rgb);
                    float hueDiff = abs(pixelHSV.x - originalHSV.x);
                    if (hueDiff > 0.5) hueDiff = 1.0 - hueDiff;
                    float satDiff = abs(pixelHSV.y - originalHSV.y);
                    if (hueDiff < _Tolerance && satDiff < _Tolerance)
                    {
                        float brightnessRatio = pixelHSV.z / (originalHSV.z + 1e-5);
                        float3 newHSV = targetHSV;
                        newHSV.z = saturate(targetHSV.z * brightnessRatio);
                        c.rgb = hsv2rgb(newHSV);
                        break;
                    }
                }
                return c;
            }
            ENDCG
        }
    }
} 