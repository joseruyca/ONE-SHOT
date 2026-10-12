using UnityEngine;
namespace OneShot.Client {
 [CreateAssetMenu(menuName="ONE SHOT/Art Theme")]
 public sealed class ArtTheme : ScriptableObject {
  public Color paper=new Color32(247,249,246,255),ink=new Color32(24,43,38,255),accent=new Color32(220,245,83,255);
  public Color slice=new Color32(247,170,128,255),park=new Color32(175,213,205,255),mosquito=new Color32(202,190,240,255);
  public Font typography;
  public Texture2D logo,bread,car,mosquitoArtwork;
  public AudioClip success,miss,transition;
  [Range(0,1)] public float audioVolume=.35f;
 }
}
