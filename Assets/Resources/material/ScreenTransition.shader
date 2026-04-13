Shader "Custom/ScreenTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Fade Color", Color) = (0, 0, 0, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Softness ("Edge Softness", Range(0, 0.2)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 100
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            fixed4 _Color;
            float _Progress;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 以屏幕中心为圆心，算距离
                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv;
                // 修正宽高比
                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x = (uv.x - 0.5) * aspect + 0.5;
                center.x = (center.x - 0.5) * aspect + 0.5;

                float dist = distance(uv, center);
                // 最大半径（对角线一半）
                float maxRadius = distance(float2(0, 0), float2(0.5 * aspect, 0.5));

                // _Progress 0 = 全透明，1 = 全黑
                // 圆从外向内缩小
                float radius = (1.0 - _Progress) * maxRadius;
                float alpha = smoothstep(radius, radius - _Softness * maxRadius, dist);

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
