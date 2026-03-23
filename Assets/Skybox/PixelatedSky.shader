Shader "Custom/PixelatedSky"
{
    Properties
    {
        [Header(Pixelation)]
        _PixelSize("Pixel Size", Range(1, 512)) = 64

        [Header(Day Sky)]
        _DayTop    ("Day Top",     Color) = (0.10, 0.35, 0.85, 1)
        _DayMid    ("Day Horizon", Color) = (0.55, 0.78, 1.00, 1)
        _DayBot    ("Day Bottom",  Color) = (0.75, 0.88, 1.00, 1)

        [Header(Night Sky)]
        _NightTop  ("Night Top",     Color) = (0.00, 0.00, 0.04, 1)
        _NightMid  ("Night Horizon", Color) = (0.02, 0.03, 0.10, 1)
        _NightBot  ("Night Bottom",  Color) = (0.01, 0.01, 0.06, 1)

        [Header(Sunset)]
        _SunsetColor    ("Sunset Color",     Color)          = (1.00, 0.38, 0.08, 1)
        _SunsetWidth    ("Sunset Width",     Range(0, 1))    = 0.28
        _SunsetStrength ("Sunset Strength",  Range(0, 2))    = 1.0

        [Header(Sun)]
        _SunColor    ("Sun Color",    Color)         = (1.00, 0.97, 0.85, 1)
        _SunSize     ("Sun Size",     Range(0.005, 0.4)) = 0.05
        _SunGlow     ("Sun Glow",     Range(0, 1))   = 0.18
        _SunIntensity("Sun Intensity",Range(0.5, 8)) = 2.5

        [Header(Sun Orbit)]
        _SunPitch("Sun Pitch", Range(-90, 90))   = 0
        _SunYaw  ("Sun Yaw",   Range(-180, 180)) = 0

        [Header(Moon)]
        _MoonColor    ("Moon Color",     Color)          = (0.85, 0.90, 1.00, 1)
        _MoonSize     ("Moon Size",      Range(0.005, 0.3)) = 0.04
        _MoonGlow     ("Moon Glow",      Range(0, 1))    = 0.20
        _MoonIntensity("Moon Intensity", Range(0.5, 8))  = 1.8
        [Toggle]
        _MoonOpposite ("Moon Opposite Sun", Float)       = 1

        [Header(Moon Orbit)]
        _MoonPitch("Moon Pitch", Range(-90, 90))   = 0
        _MoonYaw  ("Moon Yaw",   Range(-180, 180)) = 0

        [Header(Stars)]
        _StarAmount  ("Star Amount",   Range(0, 1))   = 0.55
        _StarBright  ("Star Brightness",Range(0, 3))  = 1.4
        _StarSize    ("Star Size",     Range(0.0001, 0.008)) = 0.0018
        [Toggle]
        _StarTwinkle ("Star Twinkle",  Float)         = 1
        _TwinkleSpeed("Twinkle Speed", Range(0, 10))  = 2.5
        _TwinkleMin  ("Twinkle Min",   Range(0, 1))   = 0.4

        [Header(Day Night Cycle)]
        _Time01    ("Time (0=midnight 0.5=noon)", Range(0, 1)) = 0.25
        [Toggle]
        _AutoCycle ("Auto Cycle", Float) = 0
        _CycleSpeed("Cycle Speed", Range(0.0005, 0.05)) = 0.008

        [Header(Color Banding)]
        _Bands("Color Bands (0=smooth)", Range(0, 32)) = 6
    }

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        ZWrite Off   ZTest Always   Cull Off

        // Pass 0 — cubemap bake
        Pass
        {
            Name "SkyboxCubemap"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma target   4.5
            #define CUBEMAP_PASS
            #include "PixelatedSkyHDRP.hlsl"
            ENDHLSL
        }

        // Pass 1 — fullscreen camera
        Pass
        {
            Name "SkyboxCamera"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma target   4.5
            #include "PixelatedSkyHDRP.hlsl"
            ENDHLSL
        }
    }
    FallBack Off
}
