using System;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UIElements;
using OneShot.Core;
namespace OneShot.Client {
 public sealed partial class OneShotApp {
  FeedbackAudio feedback;
  readonly List<int> practiceScores=new List<int>();
  readonly List<string> completedNames=new List<string>();
  bool ReducedMotion {get=>PlayerPrefs.GetInt("oneshot.reducedMotion",0)==1;set{PlayerPrefs.SetInt("oneshot.reducedMotion",value?1:0);PlayerPrefs.Save();shell?.EnableInClassList("reduced-motion",value);}}
  void Navigation(string selected){
   var bar=new VisualElement();bar.style.flexDirection=FlexDirection.Row;bar.AddToClassList("navigation");bar.style.marginTop=8;bar.style.justifyContent=Justify.SpaceBetween;
   var titles=new[]{"nav_today","nav_train","nav_crews","nav_profile"};var actions=new Action[]{Home,TrainingHub,Crews,Profile};var ids=new[]{"today","train","crews","profile"};
   for(int i=0;i<4;i++){var action=actions[i];var b=new Button(()=>{if(!busy)action();}){text=L(titles[i])};b.style.fontSize=17;b.style.height=52;b.style.flexGrow=1;b.style.color=art.ink;b.style.backgroundColor=ids[i]==selected?(Color)new Color32(255,231,185,255):Color.clear;b.style.borderTopLeftRadius=18;b.style.borderTopRightRadius=18;b.style.borderBottomLeftRadius=18;b.style.borderBottomRightRadius=18;b.style.borderTopWidth=0;b.style.borderBottomWidth=0;b.style.borderLeftWidth=0;b.style.borderRightWidth=0;bar.Add(b);}shell.Add(bar);
  }
  void TrainingHub(){ResetCancellation();Clear();busy=false;Header("train");Text(L("training_title"),48);Text(L("training_note"),21);
   foreach(var game in new[]{"slice","park","mosquito"}){var selected=game;GameCard(game,()=>Guide(selected));}
   Button(L("training_full"),()=>PlayPractice(),true);Navigation("train");
  }
  void Guide(string game){Clear();Header("train");Text(game.ToUpperInvariant(),52);Text(L(game+"_guide"),25);var config=PracticeConfigs();var match=Array.Find(config,c=>c.game==game).trials[0];
   var toy=new ToyCanvas{Kind=game,Theme=art,Config=match,Elapsed=game=="mosquito"?600:0,Position=match.target};toy.style.height=320;root.Add(toy);
   Text(L("guide_rules"),19);Button(L("guide_start"),()=>PlayPractice(game),true);Button(L("guide_back"),TrainingHub);}
  void Profile(){Clear();Header("profile");Text(L("profile_title"),48);Text(L("profile_note"),22);foreach(var game in new[]{"slice","park","mosquito"}){int best=PlayerPrefs.GetInt("oneshot.practice."+game,0);Text(game.ToUpperInvariant()+"  ·  "+(best>0?best.ToString("N0"):L("no_record")),24);}
   var audio=new Toggle(L("settings_audio")){value=feedback.Enabled};audio.style.fontSize=22;audio.style.marginTop=18;audio.RegisterValueChangedCallback(e=>feedback.Enabled=e.newValue);root.Add(audio);
   var motion=new Toggle(L("settings_motion")){value=ReducedMotion};motion.style.fontSize=22;motion.style.marginTop=18;motion.RegisterValueChangedCallback(e=>ReducedMotion=e.newValue);root.Add(motion);
   Button(L("run_title"),Runs);Button(L("history_title"),History);Button(L("settings_language"),()=>{PlayerPrefs.SetInt("oneshot.language",Spanish?0:1);PlayerPrefs.Save();copy=new Localizer(Spanish);Profile();});Button(L("reset_practice"),ConfirmReset);Spacer();Button(gateway.SignedIn?L("account_signout"):L("account_signin"),()=>{if(gateway.SignedIn){Logout();}else Login(Profile);});if(gateway.SignedIn)Button(L("delete_account"),DeleteAccountScreen);Navigation("profile");
  }
  Action accountDestination;
  void Login(Action destination=null){accountDestination=destination??(Action)OpenDaily;Account(false);}
  void Account(bool registering){
   Clear();Text(L(registering?"account_signup_title":"copy_017"),44);Text(L(registering?"account_signup_note":"copy_018"),22);
   var email=new TextField(L("copy_019"));email.style.fontSize=22;email.style.marginBottom=16;root.Add(email);
   var password=new TextField(L("copy_020")){isPasswordField=true};password.style.fontSize=22;password.style.marginBottom=16;root.Add(password);
   var status=Text("",20);Button submit=null;
   submit=Button(L(registering?"account_create":"copy_021"),async()=>{
    if(busy)return;string address=email.value?.Trim(),secret=password.value;
    try{var parsed=new MailAddress(address);if(parsed.Address!=address||string.IsNullOrWhiteSpace(secret)||(registering&&secret.Length<12)){status.text=L("account_invalid");return;}}catch{status.text=L("account_invalid");return;}
    busy=true;submit.SetEnabled(false);status.text=L("account_processing");
    bool navigate=false;
    try{if(registering){bool signedIn=await gateway.Register(address,secret,lifetime.Token);password.value="";if(signedIn)navigate=true;else status.text=L("account_pending");}else{await gateway.SignIn(address,secret,lifetime.Token);password.value="";navigate=true;}}
    catch{password.value="";status.text=L("account_unavailable");}finally{busy=false;submit.SetEnabled(true);}
    if(navigate)accountDestination?.Invoke();
   },true);
   Button(L(registering?"account_switch_login":"account_create"),()=>{if(!busy)Account(!registering);});Spacer();Button(L("copy_023"),()=>{if(!busy)Home();});
  }
  async void PlayPractice(string selected=null){
   if(busy)return;ResetCancellation();competitiveSession=false;gameRunning=true;busy=true;practiceScores.Clear();completedNames.Clear();int total=0;
   try{foreach(var config in PracticeConfigs()){if(selected!=null&&selected!=config.game)continue;using(var g=new UnityGame(this)){await g.PlayAsync(new RoundConfig(config.game,"2",config.durationMs,JsonUtility.ToJson(config)),lifetime.Token);total+=g.PredictedScore;}}bool saved=PracticeJournal.Record(completedNames,practiceScores,total);if(!saved)Debug.LogWarning("Practice history could not be saved.");ShowResult(total,true);}
   catch{Notice(L("copy_029"),L("copy_030"));}finally{gameRunning=false;busy=false;}
  }
  void ShowResult(int total,bool practice){
   Clear();Header("results");Text(practice?L("copy_031"):L("copy_032"),18);var score=Text(ReducedMotion?total.ToString("N0"):"0",86);
   if(!ReducedMotion){float began=Time.realtimeSinceStartup;IVisualElementScheduledItem animation=null;animation=score.schedule.Execute(()=>{float t=Mathf.Clamp01((Time.realtimeSinceStartup-began)/.55f);score.text=Mathf.RoundToInt(total*(1-Mathf.Pow(1-t,3))).ToString("N0");if(t>=1)animation.Pause();}).Every(16);}
   Text(L("results_breakdown"),28);bool record=false;var scores=practice?practiceScores.ToArray():gateway.LastScores;
   for(int i=0;i<scores.Length;i++){string name=i<completedNames.Count?completedNames[i]:"round";Text(name.ToUpperInvariant()+"  ·  "+scores[i].ToString("N0")+" / 10.000",24);if(practice&&scores[i]>PlayerPrefs.GetInt("oneshot.practice."+name,0)){PlayerPrefs.SetInt("oneshot.practice."+name,scores[i]);record=true;}}
   if(practice){PlayerPrefs.Save();Text(L(record?"results_new_best":"results_practice_note"),20);}Spacer();
   if(!practice)Button(L("copy_034"),World);Button(practice?L("copy_035"):L("copy_036"),practice?(Action)(()=>PlayPractice(completedNames.Count==1?completedNames[0]:null)):Home,true);if(practice)Button(L("guide_back"),TrainingHub);
  }
 }
}
