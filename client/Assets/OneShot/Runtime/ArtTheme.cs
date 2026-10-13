using UnityEngine;
namespace OneShot.Client {
 [CreateAssetMenu(menuName="ONE SHOT/Art Theme")]
 public sealed class ArtTheme : ScriptableObject {
  public Color paper=new Color32(255,247,232,255),ink=new Color32(29,43,77,255),accent=new Color32(255,176,49,255);
  public Color slice=new Color32(247,170,128,255),park=new Color32(175,213,205,255),mosquito=new Color32(202,190,240,255);
  public Font typography;
  public Texture2D logo,bread,car,mosquitoArtwork;
  public Texture2D hero,spriteAtlas;
  public bool useStudioPalette=true;
  public bool useCustomArtwork=false;
  public void LoadStudioDefaults(){
   // Motion Garden has no raster dependencies. Texture overrides are explicitly optional.
   if(useStudioPalette){paper=new Color32(249,250,255,255);ink=new Color32(31,43,72,255);accent=new Color32(249,91,107,255);slice=new Color32(255,195,106,255);park=new Color32(80,126,237,255);mosquito=new Color32(168,135,221,255);}
  }
  public AudioClip success,miss,transition;
  [Range(0,1)] public float audioVolume=.35f;
 }
}
