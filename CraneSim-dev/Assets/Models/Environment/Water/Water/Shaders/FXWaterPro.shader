// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "FX/Water" {
Properties {
	_WaveScale ("Wave scale", Range (0.01,1.00)) = 0.063
	_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
	_RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
	_RefrColor ("Refraction color", COLOR)  = ( .34, .85, .92, 1)
	[NoScaleOffset] _Fresnel ("Fresnel (A) ", 2D) = "gray" {}
	[NoScaleOffset] _BumpMap ("Normalmap ", 2D) = "bump" {}
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	[NoScaleOffset] _ReflectiveColor ("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
	_HorizonColor ("Simple water horizon color", COLOR)  = ( .172, .463, .435, 1)
	[HideInInspector] _ReflectionTex ("Internal Reflection", 2D) = "" {}
	[HideInInspector] _RefractionTex ("Internal Refraction", 2D) = "" {}
}

Subshader {
	Tags { "WaterMode"="Refractive" "RenderType"="Opaque" }
	Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE

#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
#define HAS_REFLECTION 1
#endif
#if defined (WATER_REFRACTIVE)
#define HAS_REFRACTION 1
#endif

#include "UnityCG.cginc"

uniform float4 _WaveScale4;
uniform float4 _WaveOffset;

#if HAS_REFLECTION
uniform float _ReflDistort;
#endif
#if HAS_REFRACTION
uniform float _RefrDistort;
#endif

struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	UNITY_VERTEX_INPUT_INSTANCE_ID // Магический макрос 1: Получение ID инстанса/глаза на входе [1]
};

struct v2f {
	float4 pos : SV_POSITION;
	#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
		float4 ref : TEXCOORD0;
		float2 bumpuv0 : TEXCOORD1;
		float2 bumpuv1 : TEXCOORD2;
		float3 viewDir : TEXCOORD3;
	#else
		float2 bumpuv0 : TEXCOORD0;
		float2 bumpuv1 : TEXCOORD1;
		float3 viewDir : TEXCOORD2;
	#endif
	UNITY_FOG_COORDS(4)
	UNITY_VERTEX_OUTPUT_STEREO // Магический макрос 2: Передача ID глаза в фрагментный шейдер [1]
};

v2f vert(appdata v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v); // Магический макрос 3: Инициализация ID инстанса [1]
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // Магический макрос 4: Выделение памяти под стерео-выход [1]
	
	o.pos = UnityObjectToClipPos (v.vertex);

	// scroll bump waves
	float4 temp;
	float4 wpos = mul (unity_ObjectToWorld, v.vertex);
	temp.xyzw = wpos.xzxz * _WaveScale4 + _WaveOffset;
	o.bumpuv0 = temp.xy;
	o.bumpuv1 = temp.wz;
	
	// object space view direction
	o.viewDir.xzy = WorldSpaceViewDir(v.vertex);
	
	#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
	// ComputeScreenPos автоматически учитывает VR при правильных макросах выше
	o.ref = ComputeScreenPos(o.pos);
	#endif

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
// Используем макрос для текстуры экрана, чтобы Unity автоматически применила Texture2DArray в VR
UNITY_DECLARE_SCREENSPACE_TEXTURE(_ReflectionTex);
#endif

#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
sampler2D _ReflectiveColor;
#endif

#if defined (WATER_REFRACTIVE)
sampler2D _Fresnel;
UNITY_DECLARE_SCREENSPACE_TEXTURE(_RefractionTex);
uniform float4 _RefrColor;
#endif

#if defined (WATER_SIMPLE)
uniform float4 _HorizonColor;
#endif
sampler2D _BumpMap;

half4 frag( v2f i ) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // Магический макрос 5: Вычисление глаза для Single Pass Instanced [1]

	i.viewDir = normalize(i.viewDir);
	
	// combine two scrolling bumpmaps into one
	half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
	half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
	half3 bump = (bump1 + bump2) * 0.5;
	
	// fresnel factor
	half fresnelFac = dot( i.viewDir, bump );
	
	// perturb reflection/refraction UVs by bumpmap, and lookup colors
	
	#if HAS_REFLECTION
	float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
	// Вместо tex2Dproj используем безопасный для VR макрос UNITY_SAMPLE_SCREENSPACE_TEXTURE
	float2 projectedUV1 = uv1.xy / uv1.w;
	half4 refl = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ReflectionTex, projectedUV1);
	#endif

	#if HAS_REFRACTION
	float4 uv2 = i.ref; uv2.xy -= bump * _RefrDistort;
	float2 projectedUV2 = uv2.xy / uv2.w;
	half4 refr = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_RefractionTex, projectedUV2) * _RefrColor;
	#endif
	
	// final color is between refracted and reflected based on fresnel
	half4 color;
	
	#if defined(WATER_REFRACTIVE)
	half fresnel = UNITY_SAMPLE_1CHANNEL( _Fresnel, float2(fresnelFac,fresnelFac) );
	color = lerp( refr, refl, fresnel );
	#endif
	
	#if defined(WATER_REFLECTIVE)
	half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
	color.rgb = lerp( water.rgb, refl.rgb, water.a );
	color.a = refl.a * water.a;
	#endif
	
	#if defined(WATER_SIMPLE)
	half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
	color.rgb = lerp( water.rgb, _HorizonColor.rgb, water.a );
	color.a = _HorizonColor.a;
	#endif

	UNITY_APPLY_FOG(i.fogCoord, color);
	return color;
}
ENDCG

	}
}
}
