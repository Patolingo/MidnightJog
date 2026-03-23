#ifndef PIXELATED_SKY_HDRP
#define PIXELATED_SKY_HDRP

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Sky/SkyUtils.hlsl"

// ---------------------------------------------------------------------------
// Properties
// ---------------------------------------------------------------------------
float  _PixelSize;

float4 _DayTop, _DayMid, _DayBot;
float4 _NightTop, _NightMid, _NightBot;

float4 _SunsetColor;
float  _SunsetWidth, _SunsetStrength;

float4 _SunColor;
float  _SunSize, _SunGlow, _SunIntensity;
float  _SunPitch, _SunYaw;

float4 _MoonColor;
float  _MoonSize, _MoonGlow, _MoonIntensity;
float  _MoonOpposite, _MoonPitch, _MoonYaw;

float  _StarAmount, _StarBright, _StarSize;
float  _StarTwinkle, _TwinkleSpeed, _TwinkleMin;

float  _Time01, _AutoCycle, _CycleSpeed;
float  _Bands;

// Textura 1x1 do HDRP com o valor de exposição atual (em EV100, canal R).
// Usamos para dividir a cor final e evitar que o pipeline estoure o céu.
TEXTURE2D(_ExposureTexture);

// ---------------------------------------------------------------------------
// Structs
// ---------------------------------------------------------------------------
struct Attributes { uint id : SV_VertexID; };

struct Varyings
{
    float4 pos       : SV_POSITION;
    float3 viewDirWS : TEXCOORD0;
};

// ---------------------------------------------------------------------------
// Math helpers
// ---------------------------------------------------------------------------
float3x3 RotX(float a) { float s=sin(a),c=cos(a); return float3x3(1,0,0,0,c,-s,0,s,c); }
float3x3 RotY(float a) { float s=sin(a),c=cos(a); return float3x3(c,0,s,0,1,0,-s,0,c); }

float3 OrbitDir(float t, float pitchDeg, float yawDeg)
{
    float angle  = t * TWO_PI - HALF_PI;
    float3 base  = float3(0, sin(angle), cos(angle));
    return normalize(mul(mul(RotY(radians(yawDeg)), RotX(radians(pitchDeg))), base));
}

float3 SnapDir(float3 d, float gridSize)
{
    float theta = atan2(d.z, d.x);
    float phi   = asin(clamp(d.y / max(length(d), 1e-5), -1.0, 1.0));
    float s     = PI / gridSize;
    theta = (floor(theta / s) + 0.5) * s;
    phi   = (floor(phi   / s) + 0.5) * s;
    float cp = cos(phi);
    return float3(cos(theta)*cp, sin(phi), sin(theta)*cp);
}

float3 SkyGrad(float t, float3 bot, float3 mid, float3 top)
{
    return t < 0.5 ? lerp(bot, mid, t*2.0) : lerp(mid, top, (t-0.5)*2.0);
}

float3 Band(float3 c, float n)
{
    if (n < 1.0) return c;
    return floor(c * n + 0.5) / n;
}

float h31(float3 p)
{
    p = frac(p * float3(443.9,441.4,437.2));
    p += dot(p, p.yzx + 19.19);
    return frac((p.x+p.y)*p.z);
}
float h31b(float3 p)
{
    p = frac(p * float3(127.1,311.7,74.7));
    p += dot(p, p.zyx + 31.3);
    return frac((p.y+p.z)*p.x);
}

float DiscMask(float3 dir, float3 body, float size, out float2 uv)
{
    float3 up    = abs(body.y) > 0.999 ? float3(0,0,1) : float3(0,1,0);
    float3 right = normalize(cross(body, up));
    float3 upT   = normalize(cross(right, body));
    float3 delta = dir - body;
    uv = float2(dot(delta,right), dot(delta,upT)) / (size*2.0) + 0.5;
    return step(acos(saturate(dot(dir,body))), size * PI);
}

// ---------------------------------------------------------------------------
// Vertex
// ---------------------------------------------------------------------------
Varyings Vert(Attributes IN)
{
    Varyings OUT;
    OUT.pos       = GetFullScreenTriangleVertexPosition(IN.id, UNITY_RAW_FAR_CLIP_VALUE);
    OUT.viewDirWS = GetSkyViewDirWS(OUT.pos.xy);
    return OUT;
}

// ---------------------------------------------------------------------------
// Fragment
// ---------------------------------------------------------------------------
float4 Frag(Varyings IN) : SV_Target
{
    float3 dir    = normalize(IN.viewDirWS);
    float3 pixDir = normalize(SnapDir(dir, _PixelSize));

    float t = (_AutoCycle > 0.5) ? frac(_Time.y * _CycleSpeed) : _Time01;

    float3 sunDir  = OrbitDir(t, _SunPitch, _SunYaw);
    float3 moonDir = (_MoonOpposite > 0.5) ? -sunDir : OrbitDir(t + 0.5, _MoonPitch, _MoonYaw);

    float dayT   = saturate(sunDir.y * 2.5 + 0.5);
    float nightT = 1.0 - dayT;

    // Gradiente
    float v = pixDir.y * 0.5 + 0.5;
    float3 sky = lerp(
        SkyGrad(v, _NightBot.rgb, _NightMid.rgb, _NightTop.rgb),
        SkyGrad(v, _DayBot.rgb,   _DayMid.rgb,   _DayTop.rgb),
        dayT);

    // Sunset
    float sunsetB = pow(saturate(1.0 - abs(sunDir.y)), 2.0) * _SunsetStrength;
    float hMask   = pow(saturate(1.0 - abs(pixDir.y)), 2.0);
    float sMask   = saturate(dot(
        normalize(float3(pixDir.x,0,pixDir.z)+1e-5),
        normalize(float3(sunDir.x, 0,sunDir.z)+1e-5)) * 0.5 + 0.5);
    sky = lerp(sky, _SunsetColor.rgb, saturate(hMask * sMask * sunsetB * _SunsetWidth * 3.0));

    // Banding
    sky = Band(sky, _Bands);

    // Estrelas
    [branch]
    if (nightT > 0.01 && pixDir.y > -0.15)
    {
        float3 cid = floor(pixDir * 120.0);
        float stars = 0.0;
        for (int a=-1;a<=1;a++) for (int b=-1;b<=1;b++) for (int c2=-1;c2<=1;c2++)
        {
            float3 nb = cid + float3(a,b,c2);
            if (h31(nb) > _StarAmount) continue;
            float3 sp = normalize(nb + float3(h31(nb+1),h31(nb+2),h31(nb+3)));
            if ((1.0 - dot(pixDir,sp)) > _StarSize) continue;
            float twink = 1.0;
            if (_StarTwinkle > 0.5)
            {
                float ph = h31(nb+50)*TWO_PI, frq = h31(nb+60)*1.5+0.5;
                twink = lerp(_TwinkleMin, 1.0, sin(_Time.y*_TwinkleSpeed*frq+ph)*0.5+0.5);
            }
            stars += h31b(nb) * twink * _StarBright;
        }
        sky += stars * nightT * smoothstep(-0.15, 0.2, pixDir.y);
    }

    // Sol
    [branch]
    if (sunDir.y > -0.25)
    {
        float vis   = saturate(sunDir.y * 4.0 + 1.0);
        float glowR = _SunSize * PI + _SunGlow * 0.8;
        float glow  = pow(saturate((dot(dir,sunDir) - cos(glowR)) / max(1.0-cos(glowR),1e-5)), 3.0);
        sky += _SunColor.rgb * glow * _SunIntensity * 0.4 * vis * dayT;
        float2 suv;
        if (DiscMask(dir, sunDir, _SunSize, suv) > 0 && all(suv>0) && all(suv<1))
        {
            float ld = 1.0 - saturate(dot(suv-0.5,suv-0.5)*3.5);
            sky = lerp(sky, _SunColor.rgb * _SunIntensity * (0.7+0.3*ld), vis);
        }
    }

    // Lua
    [branch]
    if (moonDir.y > -0.25)
    {
        float mvis   = saturate(moonDir.y * 4.0 + 1.0);
        float mglowR = _MoonSize * PI + _MoonGlow * 0.8;
        float mglow  = pow(saturate((dot(dir,moonDir) - cos(mglowR)) / max(1.0-cos(mglowR),1e-5)), 3.0);
        sky += _MoonColor.rgb * mglow * _MoonIntensity * 0.3 * mvis * nightT;
        float2 muv;
        if (DiscMask(dir, moonDir, _MoonSize, muv) > 0 && all(muv>0) && all(muv<1))
        {
            float ld = 1.0 - saturate(dot(muv-0.5,muv-0.5)*3.5);
            sky = lerp(sky, _MoonColor.rgb * _MoonIntensity * (0.6+0.4*ld), mvis * max(nightT,0.12));
        }
    }

    // ---------------------------------------------------------------------------
    // Compensação de exposição
    // O HDRP renderiza o sky em linear HDR e depois aplica a exposição da câmera.
    // _ExposureTexture é uma textura 1x1 onde R = valor atual de exposição em EV100.
    // Dividir por exp2(EV) aqui faz o sky "sair pré-compensado", de forma que
    // após a câmera multiplicar por exp2(EV) o resultado final seja exatamente
    // as cores que definimos — sem estouro e sem escurecimento.
    // ---------------------------------------------------------------------------
    float ev100 = LOAD_TEXTURE2D(_ExposureTexture, int2(0,0)).r;
    sky /= max(exp2(ev100), 1e-4);

    return float4(sky, 1.0);
}

#endif
