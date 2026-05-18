Shader "Custom/Blink" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Alpha ("Alpha", Range (0, 1)) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		CGPROGRAM
		#pragma surface surf Lambert alpha
		#pragma target 3.0

		sampler2D _MainTex;
		float _Alpha;

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Alpha = _Alpha;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
