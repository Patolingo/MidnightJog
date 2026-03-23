using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PixelatedSkyRenderer : SkyRenderer
{
    static readonly int k_PixelCoordToViewDirWS = Shader.PropertyToID("_PixelCoordToViewDirWS");

    // O HDRP expõe a textura de exposição atual através deste nome.
    // É uma RenderTexture 1x1 com o valor de exposição em EV100.
    // O shader lê ela e divide sua cor por exp2(EV), compensando o pipeline.
    static readonly int k_ExposureTexture = Shader.PropertyToID("_ExposureTexture");

    static MaterialPropertyBlock s_Mpb;

    public override void Build() { }
    public override void Cleanup() { }
    protected override bool Update(BuiltinSkyParameters p) => true;

    public override void RenderSky(BuiltinSkyParameters p, bool forCubemap, bool renderSunDisk)
    {
        var settings = p.skySettings as PixelatedSkySettings;
        if (settings == null) return;

        var mat = settings.skyMaterial.value;
        if (mat == null) return;

        if (s_Mpb == null) s_Mpb = new MaterialPropertyBlock();
        s_Mpb.Clear();

        s_Mpb.SetMatrix(k_PixelCoordToViewDirWS, p.pixelCoordToViewDirMatrix);

        // Passa a textura de exposição do HDRP para o shader poder compensar.
        // p.colorBuffer é o buffer de destino; a exposure texture fica em
        // BuiltinSkyParameters e pode ser acessada via HDCamera se necessário.
        // A forma mais simples e robusta é ler direto do Shader.GetGlobalTexture:
        var exposureTex = Shader.GetGlobalTexture(k_ExposureTexture);
        if (exposureTex != null)
            s_Mpb.SetTexture(k_ExposureTexture, exposureTex);

        CoreUtils.DrawFullScreen(p.commandBuffer, mat, s_Mpb, forCubemap ? 0 : 1);
    }
}
