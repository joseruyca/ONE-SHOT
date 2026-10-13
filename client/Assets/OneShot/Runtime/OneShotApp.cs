using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using OneShot.Core;
namespace OneShot.Client {
 public sealed partial class OneShotApp : MonoBehaviour {
  ArtTheme art;BackendConfiguration backend;CompetitionGateway gateway;VisualElement root,shell;ShotRunner runner;
  CancellationTokenSource lifetime=new CancellationTokenSource();bool busy;string dailyId;
  bool Spanish=>PlayerPrefs.HasKey("oneshot.language")?PlayerPrefs.GetInt("oneshot.language")==1:Application.systemLanguage==SystemLanguage.Spanish;
  Localizer copy;
  string L(string key)=>copy.Get(key);
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){
   if(UnityEngine.Object.FindFirstObjectByType<OneShotApp>()==null)new GameObject("ONE SHOT").AddComponent<OneShotApp>();
  }
  void Start(){
   if(Camera.main==null){var cameraObject=new GameObject("ONE SHOT Camera");var camera=cameraObject.AddComponent<Camera>();cameraObject.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.cullingMask=0;camera.backgroundColor=new Color32(247,249,246,255);}
   if(UnityEngine.Object.FindFirstObjectByType<AudioListener>()==null&&Camera.main!=null)Camera.main.gameObject.AddComponent<AudioListener>();
   copy=new Localizer(Spanish);Application.targetFrameRate=60;Screen.orientation=ScreenOrientation.Portrait;
   art=Resources.Load<ArtTheme>("OneShotArt")??ScriptableObject.CreateInstance<ArtTheme>();
   art.LoadStudioDefaults();
   backend=Resources.Load<BackendConfiguration>("OneShotBackend")??ScriptableObject.CreateInstance<BackendConfiguration>();
   var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=new Vector2Int(600,1000);panel.match=1f;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("OneShot");
   var document=gameObject.AddComponent<UIDocument>();document.panelSettings=panel;var surface=document.rootVisualElement;
   surface.style.backgroundColor=art.paper;surface.style.alignItems=Align.Center;var studio=Resources.Load<StyleSheet>("Studio/Studio");if(studio!=null)surface.styleSheets.Add(studio);
   root=new VisualElement();shell=root;root.style.width=Length.Percent(100);root.style.maxWidth=600;root.style.height=Length.Percent(100);surface.Add(root);
   root.style.backgroundColor=art.paper;root.style.color=art.ink;root.style.flexGrow=1;
   if(art.typography!=null)root.style.unityFontDefinition=FontDefinition.FromFont(art.typography);
   float bottom=Screen.safeArea.y/Screen.height*1000,top=(Screen.height-Screen.safeArea.yMax)/Screen.height*1000;
   shell.style.paddingTop=top+14;shell.style.paddingBottom=bottom+10;
   gateway=new CompetitionGateway(this,backend);feedback=gameObject.AddComponent<FeedbackAudio>();feedback.Initialize(art);Home();
  }
  void OnDestroy(){lifetime.Cancel();lifetime.Dispose();gateway?.SignOut();}
  void OnApplicationPause(bool paused){if(paused&&busy)lifetime.Cancel();} // competitive resume not yet implemented
  void ClearGame(){var modal=shell.Q<VisualElement>("quit-modal");if(modal!=null)modal.RemoveFromHierarchy();shell.Clear();root=new VisualElement();root.AddToClassList("page");shell.Add(root);if(modal!=null)shell.Add(modal);}
  void Clear(){shell.Clear();var scroll=new ScrollView(ScrollViewMode.Vertical);scroll.style.flexGrow=1;scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;scroll.verticalScrollerVisibility=ScrollerVisibility.Hidden;shell.Add(scroll);root=new VisualElement();root.style.width=Length.Percent(100);root.AddToClassList("page");scroll.Add(root);}
  Label Text(string text,int size=24){var l=new Label(text);l.AddToClassList("copy");l.style.fontSize=size;if(size>=28)l.style.unityFontStyleAndWeight=FontStyle.Bold;l.style.color=art.ink;l.style.whiteSpace=WhiteSpace.Normal;l.style.marginBottom=12;root.Add(l);return l;}
  Button Button(string title,Action action,bool primary=false){var b=new Button(()=>{if(!busy)action();}){text=title};b.AddToClassList("action");b.style.height=72;b.style.minHeight=64;b.style.flexShrink=0;b.style.marginTop=14;b.style.fontSize=24;b.style.unityFontStyleAndWeight=FontStyle.Bold;
   b.style.borderLeftWidth=0;b.style.borderRightWidth=0;b.style.borderTopWidth=0;b.style.borderBottomWidth=0;b.style.backgroundColor=primary?new Color32(40,78,207,255):Color.white;b.style.color=primary?Color.white:art.ink;b.style.borderTopLeftRadius=18;b.style.borderTopRightRadius=18;b.style.borderBottomLeftRadius=18;b.style.borderBottomRightRadius=18;root.Add(b);return b;}
  void Spacer(){var s=new VisualElement();s.style.flexGrow=1;root.Add(s);}
  void Home(){ResetCancellation();Clear();busy=false;Header("today");
   Text(L("studio_headline"),54);Text(L("studio_subtitle"),21);
   var hero=new Image{image=art.hero,scaleMode=ScaleMode.ScaleAndCrop};hero.AddToClassList("hero");root.Add(hero);
   var row=new VisualElement();row.AddToClassList("chips");foreach(var name in new[]{"SLICE","PARK","MOSQUITO"}){var chip=new Label(name);chip.AddToClassList("chip");row.Add(chip);}root.Add(row);
   Button(L("copy_003"),OpenDaily,true);Button(L("studio_practice"),TrainingHub);Text(L("copy_005"),16);Navigation("today");
  }
  async void OpenDaily(){if(busy)return;
   if(!backend.IsConfigured){Notice(L("copy_006"),L("copy_007"));return;}
   if(!gateway.SignedIn){Login(OpenDaily);return;}
   busy=true;Clear();Text(L("copy_008"),38);
   try{var s=await gateway.Today(lifetime.Token);if(s==null){Notice(L("copy_009"),L("copy_010"));return;}dailyId=s.shotId;Clear();Text(s.label,52);Text(L("copy_011")+Mathf.RoundToInt(s.durationMs/1000f)+" SEC");Text(L("copy_012"));busy=false;Button(L("copy_013"),PlayDaily,true);Button(L("copy_014"),Home);}catch{Notice(L("copy_015"),L("copy_016"));}finally{busy=false;}
  }
  async void PlayDaily(){if(busy)return;ResetCancellation();competitiveSession=true;gameRunning=true;busy=true;completedNames.Clear();practiceScores.Clear();runner=new ShotRunner(gateway,name=>new UnityGame(this));
   try{var r=await runner.RunAsync(dailyId,Guid.NewGuid().ToString(),lifetime.Token);ShowResult(r.Total,false);}
   catch{Clear();Text(L("copy_024"),44);Text(L("copy_025"));if(runner.State==RunnerState.PendingSubmission)Button(L("copy_026"),RetrySubmit,true);Button(L("copy_027"),Home);}finally{gameRunning=false;busy=false;}
  }
  async void RetrySubmit(){if(busy)return;busy=true;try{var r=await runner.SubmitAsync(lifetime.Token);ShowResult(r.Total,false);}catch{Text(L("copy_028"),20);}finally{busy=false;}}
  async void World(){if(busy)return;busy=true;try{var w=await gateway.World(dailyId,lifetime.Token);Text(w.population<20?L("copy_038"):"TOP "+w.topPercent.ToString("0.0")+"%",28);Text(L("copy_039")+w.population+(w.provisional?L("copy_040"):""),20);}catch{Text(L("copy_041"),20);}finally{busy=false;}}
  void Notice(string title,string message){Clear();Text(title,44);Text(message,24);Spacer();Button(L("copy_042"),Home,true);}
  GameConfig[] PracticeConfigs(){var games=new[]{"slice","park","mosquito"};var rounds=new GameConfig[3];for(int r=0;r<3;r++){rounds[r]=new GameConfig{game=games[r],version="2",durationMs=15000,trials=new GameConfig[3]};for(int i=0;i<3;i++){var t=new GameConfig{game=games[r],durationMs=5000,target=games[r]=="slice"?UnityEngine.Random.Range(2500,7500):UnityEngine.Random.Range(5000,8500),tolerance=games[r]=="slice"?5000:3000,speedPerSecond=2200};if(r==2)t.windows=new[]{new Window{start=500,end=1100,x=UnityEngine.Random.Range(2000,8000),y=UnityEngine.Random.Range(2000,8000),radius=1200},new Window{start=1800,end=2400,x=UnityEngine.Random.Range(2000,8000),y=UnityEngine.Random.Range(2000,8000),radius=1200},new Window{start=3200,end=3800,x=UnityEngine.Random.Range(2000,8000),y=UnityEngine.Random.Range(2000,8000),radius=1200}};rounds[r].trials[i]=t;}}return rounds;}
  sealed class UnityGame : IMicrogame {
   readonly OneShotApp app;public int PredictedScore;public UnityGame(OneShotApp a){app=a;}
   public Task<RoundEvidence> PlayAsync(RoundConfig c,CancellationToken token){var completion=new TaskCompletionSource<RoundEvidence>();app.StartCoroutine(app.Round(JsonUtility.FromJson<GameConfig>(c.ParametersJson),token,completion,s=>PredictedScore=s));return completion.Task;}
   public void Dispose(){}
  }

  IEnumerator Round(GameConfig round,CancellationToken token,TaskCompletionSource<RoundEvidence> completion,Action<int> prediction){
   completedNames.Add(round.game);
   ClearGame();Text(L("round_ready"),20);Text(round.game.ToUpperInvariant(),60);Text(L(round.game+"_guide"),26);Spacer();var ready=Text("",72);
   for(int count=2;count>=1;count--){ready.text=count.ToString();feedback.Transition();yield return new WaitForSecondsRealtime(1);if(token.IsCancellationRequested){completion.TrySetCanceled();yield break;}}
   var evidence=new List<string>();int combined=0;
   if(round.trials==null||round.trials.Length!=3){completion.TrySetException(new Exception("Unsupported config"));yield break;}
   for(int i=0;i<3;i++){
    ClearGame();RoundHeader(round.game,i);
    Text(round.game=="slice"?L("copy_043"):round.game=="park"?L("copy_044"):L("copy_045"),38);
    var config=round.trials[i];Text(round.game=="slice"?(config.target/100f).ToString("0")+" / "+(100-config.target/100f).ToString("0"):round.game=="park"?L("copy_046"):L("copy_047"),24);
    var toy=new ToyCanvas{Kind=round.game,Theme=art,Config=config};toy.style.height=380;toy.style.minHeight=240;toy.style.flexShrink=0;root.Add(toy);var timerTrack=new VisualElement();timerTrack.AddToClassList("timer-track");var timerFill=new VisualElement();timerFill.AddToClassList("timer-fill");timerTrack.Add(timerFill);root.Add(timerTrack);var progress=Text("5.0 SEC",20);
    var performance=Text("",24);performance.style.minHeight=32;
    var events=new List<GameEvent>();var hits=new HashSet<int>();bool used=false;int trialScore=0;double start=Time.realtimeSinceStartupAsDouble;Vector2 down=Vector2.zero;
    int activePointer=-1;
    toy.RegisterCallback<PointerDownEvent>(e=>{if(activePointer!=-1)return;activePointer=e.pointerId;down=e.localPosition;toy.CapturePointer(e.pointerId);});
    toy.RegisterCallback<PointerCancelEvent>(e=>{if(e.pointerId==activePointer){activePointer=-1;toy.ReleasePointer(e.pointerId);}});
    toy.RegisterCallback<PointerUpEvent>(e=>{
     if(e.pointerId!=activePointer)return;activePointer=-1;toy.ReleasePointer(e.pointerId);int time=(int)((Time.realtimeSinceStartupAsDouble-start)*1000);if(time<0||time>config.durationMs)return;
     int x=Mathf.Clamp(Mathf.RoundToInt(e.localPosition.x/toy.resolvedStyle.width*10000),0,10000),y=Mathf.Clamp(Mathf.RoundToInt(e.localPosition.y/toy.resolvedStyle.height*10000),0,10000);
     if(events.Count>0&&time<=events[events.Count-1].t)return;
     bool positive=false;
     if(round.game=="slice"){
      if(used)return;
      if(!SliceGeometry.TryCut(toy.resolvedStyle.width,toy.resolvedStyle.height,down.x,down.y,e.localPosition.x,e.localPosition.y,out int cut))return;
      used=true;
      events.Add(new GameEvent{type="cut",t=time,x=cut});trialScore=Scoring.Precision(cut,config.target,config.tolerance);toy.Cut=cut;positive=trialScore>=7500;
     }else if(round.game=="park"){
      if(used)return;used=true;events.Add(new GameEvent{type="brake",t=time});toy.Stopped=true;toy.Position=(int)((long)time*config.speedPerSecond/1000);trialScore=toy.Position>10000?0:Scoring.Precision(toy.Position,config.target,config.tolerance);positive=trialScore>=7500;
     }else{
      if(events.Count>=128)return;events.Add(new GameEvent{type="tap",t=time,x=x,y=y});int index=Array.FindIndex(config.windows,w=>time>=w.start&&time<=w.end&&(long)(x-w.x)*(x-w.x)+(long)(y-w.y)*(y-w.y)<=(long)w.radius*w.radius);positive=index>=0&&hits.Add(index);int misses=events.Count-hits.Count;trialScore=Mathf.Max(0,hits.Count*10000/config.windows.Length-misses*500);
     }
     toy.FeedbackAt=time;toy.Hit=positive;feedback.Impact(positive);performance.text=round.game=="mosquito"?hits.Count+" / "+config.windows.Length:trialScore.ToString("N0")+" PTS";toy.MarkDirtyRepaint();
    });
    while((Time.realtimeSinceStartupAsDouble-start)*1000<config.durationMs){if(token.IsCancellationRequested){completion.TrySetCanceled();yield break;}toy.Elapsed=(int)((Time.realtimeSinceStartupAsDouble-start)*1000);if(!toy.Stopped)toy.Position=(int)((long)toy.Elapsed*config.speedPerSecond/1000);toy.style.backgroundColor=Color.Lerp(Color.white,round.game=="slice"?art.slice:round.game=="park"?art.park:art.mosquito,.16f);timerFill.style.width=Length.Percent(100f*Mathf.Clamp01(1-toy.Elapsed/(float)config.durationMs));progress.text=Mathf.Max(0,(config.durationMs-toy.Elapsed)/1000f).ToString("0.0")+" SEC";toy.MarkDirtyRepaint();yield return null;}
    if(round.game=="mosquito"){int misses=events.Count-hits.Count;trialScore=Mathf.Max(0,hits.Count*10000/config.windows.Length-misses*500);}
    var parts=new List<string>();foreach(var ev in events)parts.Add(JsonUtility.ToJson(ev));evidence.Add("["+string.Join(",",parts)+"]");combined+=trialScore;
    ClearGame();Text(L("copy_048"),18);Text(trialScore.ToString("N0"),78);yield return new WaitForSecondsRealtime(.7f);
   }
   if(token.IsCancellationRequested){completion.TrySetCanceled();yield break;}practiceScores.Add(combined/3);prediction(combined/3);completion.TrySetResult(new RoundEvidence("["+string.Join(",",evidence)+"]"));
  }
 }
}
