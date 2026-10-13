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
  public void LoadStudioDefaults(){
   if(hero==null)hero=Resources.Load<Texture2D>("Studio/Hero");
   if(spriteAtlas==null)spriteAtlas=Resources.Load<Texture2D>("Studio/Toys");
   if(useStudioPalette){paper=new Color32(255,247,232,255);ink=new Color32(29,43,77,255);accent=new Color32(255,176,49,255);slice=new Color32(255,189,115,255);park=new Color32(71,109,233,255);mosquito=new Color32(185,167,237,255);}
  }
  public AudioClip success,miss,transition;
  [Range(0,1)] public float audioVolume=.35f;
 }
}
