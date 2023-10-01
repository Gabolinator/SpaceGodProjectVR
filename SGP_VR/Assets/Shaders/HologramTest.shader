Shader "Unlit/HologramTest"
{
    Properties
    {
        [HDR] _HologramColor("Hologram Color", Color) = (1, 1, 1, 0)
        _HologramAlpha("Hologram Alpha", Range(0.0, 1.0)) = 1.0
        // The main texture acts as a color mask
        _HologramMaskMap("Hologram Mask", 2D) = "white"{}
        _HologramMaskAffect("Hologram Mask Affect", Range(0.0, 1.0)) = 0.5
            // Holographic jitter parameter setting: x represents velocity, y represents jitter range, z represents jitter offset, w represents frequency (0 ~ 0.99)
            _HologramGliterData1("Hologram Gliter Data1", Vector) = (0, 1, 0, 0)
            _HologramGliterData2("Hologram Gliter Data2", Vector) = (0, 1, 0, 0)
            // Scan line
            _HologramLine1("HologramLine1", 2D) = "white" {}
            _HologramLine1Speed("Hologram Line1 Speed", Range(-10.0, 10.0)) = 1.0
            _HologramLine1Frequency("Hologram Line1 Frequency", Range(0.0, 100.0)) = 20.0
            _HologramLine1Alpha("Hologram Line 1 Alpha", Range(0.0, 1.0)) = 0.15

            [Toggle(_USE_SCANLINE2)]_HologramLine2Tog("Hologram Line2 Toggle", float) = 0.0
            _HologramLine2("HologramLine2", 2D) = "white" {}
            _HologramLine2Speed("Hologram Line2 Speed", Range(-10.0, 10.0)) = 1.0
            _HologramLine2Frequency("Hologram Line2 Frequency", Range(0.0, 100.0)) = 20.0
            _HologramLine2Alpha("Hologram Line 2 Alpha", Range(0.0, 1.0)) = 0.15
                // Holographic Fresnel
                _FresnelScale("Fresnel Scale", Float) = 1
                _FresnelPower("Fresnel Power", Float) = 2

                _HologramNoiseMap("Hologram Noise Map", 2D) = "white"{}
            // Granule effect
            // xy: noise sampling tiling, zw: noise color range (0 ~ 1)
            _HologramGrainData("Hologram Grain Data", Vector) = (20, 20, 0, 1)
            _HologramGrainSpeed("Hologram Grain Speed", Float) = 1.0
            _HologramGrainAffect("Hologram Grain Affect", Range(0 , 1)) = 1

                // Holographic color fault effect
                [Toggle] _HologramColorGlitchTog("Enable Hologram Color Glitch", Float) = 0
                // Noise velocity (using XY component)
                _HologramColorGlitch("Hologram Color Glitch", Range(0.0, 1.0)) = 0.5
                _HologramColorGlitchData("Hologram Color Glitch Data", Vector) = (1, 1, 0, 0)
                _HologramColorGlitchMin("Hologram Color Glitch Min", Range(0.0, 1.0)) = 0.5

    }
        SubShader
            {
                Tags{"Queue" = "Transparent" "RenderType" = "Transparent"}
                CGINCLUDE
                    struct a2v_hg
                    {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;
                        float3 normal:NORMAL;
                    };
                    struct v2f_hg
                    {
                        float4 pos : SV_POSITION;
                        float2 uv : TEXCOORD0;
                        float4 posWorld : TEXCOORD1;
                        float3 normalDir : TEXCOORD2;
                    };

                    // Color fault effect
                    half _HologramColorGlitchTog, _HologramColorGlitch, _HologramColorGlitchMin;
                    half4 _HologramColorGlitchData;

                    sampler2D _HologramNoiseMap;
                    // Holographic color particles
                    half4 _HologramGrainData;
                    half _HologramGrainSpeed, _HologramGrainAffect;
                    // Sampling noise map
                    float SampleNoiseMap(float2 uv)
                    {
                        return tex2D(_HologramNoiseMap, uv).r;
                    }


                    // Holographic Fresnel
                    half _FresnelScale, _FresnelPower;
                    float4 _HologramColor;
                    fixed _HologramAlpha;
                    // Holographic mask
                    sampler2D _HologramMaskMap;
                    float4 _HologramMaskMap_ST;
                    half _HologramMaskAffect;

                    half4 _HologramGliterData1, _HologramGliterData2;
                    half3 VertexHologramOffset(float3 vertex, half4 offsetData)
                    {
                        half speed = offsetData.x;
                        half range = offsetData.y;
                        half offset = offsetData.z;
                        half frequency = offsetData.w;

                        half offset_time = sin(_Time.y * speed);
                        // step(y, x) if x > = y, it returns 1, otherwise it returns 0, which is used to determine that vertex dithering starts at a certain place in sine time
                        half timeToGliter = step(frequency, offset_time);
                        half gliterPosY = sin(vertex.y + _Time.z);
                        half gliterPosYRange = step(0, gliterPosY) * step(gliterPosY, range);
                        // Get offset
                        half res = gliterPosYRange * offset * timeToGliter * gliterPosY;

                        // Define this offset as the offset of the view coordinates, and then go to the model coordinates
                        float3 view_offset = float3(res, 0, 0);
                        return mul((float3x3)UNITY_MATRIX_T_MV, view_offset);
                    }

                    // Because the Pass of vertex deep writing also uses the same vertex function as holographic Pass, so move to this side
                    // Holographic scanning line
                    sampler2D _HologramLine1;
                    half _HologramLine1Speed, _HologramLine1Frequency, _HologramLine1Alpha;
                    sampler2D _HologramLine2;
                    half _HologramLine2Speed, _HologramLine2Frequency, _HologramLine2Alpha;

                    v2f_hg HologramVertex(a2v_hg v)
                    {
                        v2f_hg o;
                        // The distortion coefficient in the vertex direction of the model is generated
                        v.vertex.xyz += VertexHologramOffset(v.vertex.xyz, _HologramGliterData1);
                        v.vertex.xyz += VertexHologramOffset(v.vertex.xyz, _HologramGliterData2);
                        // o.pos = UnityObjectToClipPos(v.vertex);
                        o.uv = v.uv;
                        o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                        o.pos = mul(UNITY_MATRIX_VP, o.posWorld);
                        o.normalDir = mul((float3x3)unity_ObjectToWorld, v.normal);
                        return o;
                    }
                    ENDCG
                Pass
                     {
                         Name "Depth Mask"
                         // Turn on the depth write, and set the color mask. 0 means no color output
                         ZWrite On
                         ColorMask 0

                         CGPROGRAM
                             #pragma target 3.0
                             #pragma vertex HologramVertex
                             #pragma fragment HologramMaskFragment

                             float4 HologramMaskFragment(v2f_hg i) : SV_TARGET
                             {
                                 return 0;
                             }
                         ENDCG
                     }
                Pass
                {
                    Name "Hologram Effect"
                    Blend SrcAlpha OneMinusSrcAlpha
                    ZWrite Off
                    CGPROGRAM
                        #pragma target 3.0
                        #pragma shader_feature _USE_SCANLINE2
                        #pragma vertex HologramVertex
                        #pragma fragment HologramFragment

                        float4 HologramFragment(v2f_hg i) : SV_Target
                        {
                            float4 main_color = _HologramColor;
                            // Using r channel of main texture to make color mask
                            float2 mask_uv = i.uv.xy * _HologramMaskMap_ST.xy + _HologramMaskMap_ST.zw;
                            float4 mask = tex2D(_HologramMaskMap, mask_uv);
                            // Add a parameter to control the mask effect
                            float mask_alpha = lerp(1, mask.r, _HologramMaskAffect);

                            // Color fault effect
                            float color_glicth_noise = SampleNoiseMap(float2(_Time.x * _HologramColorGlitchData.x, _Time.x * _HologramColorGlitchData.y));
                            color_glicth_noise = color_glicth_noise * (1.0 - _HologramColorGlitchMin) + _HologramColorGlitchMin;
                            color_glicth_noise = clamp(color_glicth_noise, 0.0, 1.0);
                            float color_glitch = lerp(1.0, color_glicth_noise, _HologramColorGlitch * _HologramColorGlitchTog);

                            // Holographic effect scanning line
                            float2 line1_uv = (i.posWorld.y * _HologramLine1Frequency + _Time.y * _HologramLine1Speed).xx;
                            float line1 = clamp(tex2D(_HologramLine1, line1_uv).r, 0.0, 1.0);
                            float4 line1_color = float4((main_color * line1).rgb, line1) * _HologramLine1Alpha;
                            float line1_alpha = clamp(((main_color).a + (line1_color).w), 0.0, 1.0);

                            #if defined (_USE_SCANLINE2)
                                float2 line2_uv = (i.posWorld.y * _HologramLine2Frequency + _Time.y * _HologramLine2Speed).xx;
                                float line2 = clamp(tex2D(_HologramLine2, line2_uv).r, 0.0, 1.0);
                                float4 line2_color = float4((main_color * line2).rgb, line2) * _HologramLine2Alpha;
                                float line2_alpha = clamp(((main_color).a + (line2_color).w), 0.0, 1.0);
                            #else
                                float4 line2_color = 0.0;
                                float line2_alpha = 1.0;
                            #endif

                                // Fresnel reflection
                                float3 w_viewDir = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                                float3 w_normal = normalize(i.normalDir);
                                float nDotV = dot(w_normal, w_viewDir);
                                float rim_nDotV = 1.0 - nDotV;
                                float4 fresnel = _FresnelScale * pow(rim_nDotV, _FresnelPower);
                                fresnel.a = clamp(fresnel.a, 0.0, 1.0);
                                float4 fresnel_color = (float4(fresnel.rgb, 1.0) * float4(main_color.rgb, 1.0)) * fresnel.a;

                                // Granule effect
                                float grain_noise = SampleNoiseMap((i.posWorld.xy * _HologramGrainData.xy + _Time.y * _HologramGrainSpeed));
                                float grain_amount = lerp(_HologramGrainData.z, _HologramGrainData.w, grain_noise) * _HologramGrainAffect;

                                float4 resultColor = float4(
                                    // rgb
                                    main_color.rgb + line1_color.rgb * line1_alpha + line2_color.rgb * line2_alpha + fresnel_color.rgb + grain_amount,
                                    // alpha
                                    _HologramAlpha * mask_alpha
                                    );

                                // Apply global color fault effect
                                resultColor *= color_glitch;

                                return resultColor;
                            }

                        ENDCG
                    }
            }
}