// 使用Shader模拟PS中一些基本的色彩调整效果
// 饱和度：https://www.cnblogs.com/muyuge/p/6152396.html
// 色彩平衡：https://zhuanlan.zhihu.com/p/59450298
Shader "UI/Ps Simulation"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_RBalance("红平衡", Range(-100, 100)) = 0
		_GBalance("绿平衡", Range(-100, 100)) = 0
		_BBalance("蓝平衡", Range(-100, 100)) = 0

		_Brightness("明度", Range(0, 2)) = 1
		_Saturation("饱和度", Range(-100, 100)) = 0
		_Contrast("对比度", Range(0, 2)) = 1

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				half4 mask : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;
			float _MaskSoftnessX;
			float _MaskSoftnessY;
			half _Brightness;
			half _Saturation;
			half _Contrast;
			float _RBalance;
			float _GBalance;
			float _BBalance;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				float4 vPosition = UnityObjectToClipPos(v.vertex);
				OUT.worldPosition = v.vertex;
				OUT.vertex = vPosition;

				float2 pixelSize = vPosition.w;
				pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

				float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
				float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
				OUT.texcoord = float4(v.texcoord.x, v.texcoord.y, maskUV.x, maskUV.y);
				OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw,
				                 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + abs(pixelSize.xy)));

				OUT.color = v.color * _Color;
				return OUT;
			}

			fixed calc_mid_tones_delta(fixed v, float balance)
			{
				return pow(v, exp(-0.0033944 * balance));
			}

			fixed3 calc_mid_tones_balance(fixed3 origin)
			{
				fixed3 balance = fixed3(0, 0, 0);
				// 红平衡
				fixed redDelta = calc_mid_tones_delta(origin.r, _RBalance) - origin.r;
				balance = balance + fixed3(redDelta, -redDelta, -redDelta);
				// 绿平衡
				fixed greenDelta = calc_mid_tones_delta(origin.g, _GBalance) - origin.g;
				balance = balance + fixed3(-greenDelta, greenDelta, -greenDelta);
				// 蓝平衡
				fixed blueDelta = calc_mid_tones_delta(origin.b, _BBalance) - origin.b;
				balance = balance + fixed3(-blueDelta, -blueDelta, blueDelta);

				origin = origin + balance;
				return fixed3(clamp(origin.r, 0, 1), clamp(origin.g, 0, 1), clamp(origin.b, 0, 1));
			}

			fixed3 calc_saturation(fixed3 origin)
			{
				float increment = _Saturation / 100;
				fixed rgbMax = max(origin.r, max(origin.g, origin.b));
				fixed rgbMin = min(origin.r, min(origin.g, origin.b));
				fixed delta = rgbMax - rgbMin;
				// TODO Delta为0，不处理

				fixed value = rgbMax + rgbMin;
				fixed L = value / 2;
				fixed S = delta / lerp(value, 2 - value, step(0.5, L));
				// step的定义是后者小于前者，而这里的要求是increment大于等于前者，故取-increment
				// if increment >= 0, -increment < 0, sign = 0 else sign = 1
				half sign = step(0, -increment);
				fixed temp = lerp(S, 1 - increment, step(1, -(increment + S)));
				temp = 1 / temp - 1;
				fixed mul = lerp(temp, increment, sign) + sign;
				fixed r = lerp(origin.r, L, sign) + (origin.r - L) * mul;
				fixed g = lerp(origin.g, L, sign) + (origin.g - L) * mul;
				fixed b = lerp(origin.b, L, sign) + (origin.b - L) * mul;

				return fixed3(r, g, b);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				fixed3 rgb = color;
				// 饱和度：计算同等亮度下饱和度最低的值，再和原图进行插值
				rgb = calc_saturation(rgb);
				// 色彩平衡
				rgb = calc_mid_tones_balance(rgb);
				// 明度：直接乘以系数
				rgb = rgb * _Brightness;
				// 对比度：计算对比度最低的值，再和原图进行插值
				rgb = lerp(fixed3(0.5, 0.5, 0.5), rgb, _Contrast);
				color = fixed4(rgb, color.a);

				#ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
				#endif

				#ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
				#endif

				return color;
			}
			ENDCG
		}
	}
}