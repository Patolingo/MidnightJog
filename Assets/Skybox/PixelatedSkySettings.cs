using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[VolumeComponentMenu("Sky/Pixelated Sky")]
[SkyUniqueID(779)]
public class PixelatedSkySettings : SkySettings
{
    public MaterialParameter skyMaterial = new MaterialParameter(null);

    public override System.Type GetSkyRendererType() => typeof(PixelatedSkyRenderer);

    public override int GetHashCode()
    {
        int hash = base.GetHashCode();
        unchecked { hash = hash * 23 + skyMaterial.GetHashCode(); }
        return hash;
    }
}
