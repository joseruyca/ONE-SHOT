using UnityEngine;
namespace OneShot.Client {
 public sealed class FeedbackAudio:MonoBehaviour {
  AudioSource source;AudioClip success,miss,transition;float volume;
  public bool Enabled {get=>PlayerPrefs.GetInt("oneshot.audio",1)==1;set{PlayerPrefs.SetInt("oneshot.audio",value?1:0);PlayerPrefs.Save();}}
  public void Initialize(ArtTheme theme){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;volume=theme.audioVolume;success=theme.success??Tone(660,880,.13f);miss=theme.miss??Tone(180,140,.10f);transition=theme.transition??Tone(440,440,.055f);}
  public void Impact(bool positive){if(Enabled&&source!=null)source.PlayOneShot(positive?success:miss,volume);}
  public void Transition(){if(Enabled&&source!=null)source.PlayOneShot(transition,volume*.6f);}
  static AudioClip Tone(float start,float end,float seconds){int count=Mathf.RoundToInt(48000*seconds);var samples=new float[count];float phase=0;for(int i=0;i<count;i++){float t=i/(float)count;phase+=2*Mathf.PI*Mathf.Lerp(start,end,t)/48000;samples[i]=Mathf.Sin(phase)*Mathf.Sin(Mathf.PI*t)*.12f;}var clip=AudioClip.Create("OneShotTone",count,1,48000,false);clip.SetData(samples,0);return clip;}
 }
}
