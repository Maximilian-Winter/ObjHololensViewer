Shader "Custom/TestShader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{ 
			"RenderType" = "Opaque" 
		}
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input 
		{
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) 
		{
			o.Albedo = IN.color;
		}
	ENDCG
	}
	Fallback "Diffuse"
}