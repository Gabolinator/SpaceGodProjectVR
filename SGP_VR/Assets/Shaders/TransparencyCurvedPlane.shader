Shader "Unlit/XPR_TransparencyCurvedPlane"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Tint Color", Color) = (1, 1, 1, 1)
        _Alpha("Alpha", Range(0, 1)) = 1.0

        _EdgeSizeX("X Edge Size", Range(0, .5)) = 0.1
        _EdgeSizeY("Y Edge Size", Range(0, .5)) = 0.1
            _Thickness("Thickness", Range(0,1)) = .5
        _Radius("Radius", Range(0,10)) = 1.0
        _Center("Center Offset", Vector) = (0.5, 0.5, 0 ,0)
    }

        SubShader
        {
            Tags {

                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "RenderPipeline" = "UniversalPipeline"
                "IgnoreProjector" = "True"
                "ShaderModel" = "4.5"
        }
           
                   ZWrite Off // Disable depth writing
            Blend SrcAlpha OneMinusSrcAlpha


                LOD 100

                Pass
                {
                         HLSLPROGRAM
                  #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x
                #pragma vertex vert
                #pragma fragment frag
                     
                         #include "UnityCG.cginc"

                    sampler2D _MainTex;

                    float4 _Color;
                    float _Alpha;
                    float _EdgeSizeX;
                    float _EdgeSizeY;
                    float _Thickness;
                    float _Radius;
                    float2 _Center;

                    struct appdata_t
                    {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;

                        UNITY_VERTEX_INPUT_INSTANCE_ID //Insert for stero support
                    };

                    struct v2f
                    {
                        float4 vertex : SV_POSITION;
                        float2 uv : TEXCOORD0;

                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    float OnLine(float a, float b, float line_width, float edge_thickness)
                    {
                        float half_line_width = line_width * 0.5;

                        return smoothstep(a - half_line_width - edge_thickness, a - half_line_width + edge_thickness, b)
                            - smoothstep(a + half_line_width - edge_thickness, a + half_line_width + edge_thickness, b);
                    }

                    float OutsideZone(float pos, float2 center, float edge)
                    {
                        return (1 - smoothstep(edge, 1 - edge, pos));
                    }

                    v2f vert(appdata_t v)
                    {
                        v2f o;


                        UNITY_SETUP_INSTANCE_ID(v); //Insert for stero support
                        UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert for stero support
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert for stero support


                        o.vertex = UnityObjectToClipPos(v.vertex + float4(0, sqrt(_Radius * _Radius - (v.vertex.x) * (v.vertex.x)), 0, 0));

                        o.uv = v.uv;
                        return o;
                    }

                 

                    float4 frag(v2f i) : SV_Target
                    {


                        float2 center = float2(0.5 + _Center.x, 0.5 + _Center.y);
                        float2 dist = abs(i.uv - center);
                        float alpha = OutsideZone(dist.x, center, .5 - _EdgeSizeX);
                        alpha *= OutsideZone(dist.y, center, .5 - _EdgeSizeY);


                        alpha *= smoothstep(_Thickness, 1, alpha);
                        alpha *= _Alpha;

                        half4 texColor = tex2D(_MainTex, i.uv);


                        half4 finalColor = texColor * _Color;
                        finalColor.a = alpha;

                        return finalColor;
                    }
                    ENDHLSL
                }
        }
            FallBack "Diffuse"

}
