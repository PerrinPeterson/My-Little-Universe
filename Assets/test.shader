//This will likely not work, and is going to be shelved for the time being. The arrays have a hard limit of very little (by rough math, about 4000 stars)
//A possible fix for this is to use multiple buffers, but then its not as easy to quickly adjust if I want to increase the scale of the universe.
//There'd be a hard limit, and I'd have to come back to this shader any time I wanted to change it. So for now, textures.
Shader "Custom/test"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // _StarCount ("Star count", int) = 8000
        // _UniverseDimensions ("Universe dimensions", Vector) = (1000,1000,1000,0)
        // _CurrentLocation ("Current Location", int) = 1 Where in the array our current star is
        uniform int _StarCount; // How many stars we have in the universe
        uniform float3 _UniverseDimensions; // the size of the 3d array in C#, we can use functions to use a 1D array like a 2d one.
        uniform float3 _CurrentLocation; // Where in the array our current star is
        
        //uniform float3 _StarPos[8000]; // 8000 stars max
        //uniform float3 _StarOffsets[8000]; // 8000 stars max
        //uniform float _StarSize[8000]; // 8000 stars max

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * fixed4(0,1,0,0);
            // o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            // o.Metallic = _Metallic;
            // o.Smoothness = _Glossiness;
            // o.Alpha = c.a;

            // My star code
            // General Idea is that from the point where we are in the universe, we can "draw" and arrow from each pixel out to the rest of the galaxy. When the arrow enters a star section, we see if the arrow intersects with the star. If it does, we output as white.
            // This should mean that we can see stars close to us very clearly, but stars far away will either twinkle, or not be visable at all, due to the light not reaching us.
        }
        ENDCG
    }
    FallBack "Diffuse"
}
