using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using OneShot.Core;
namespace OneShot.Client {
 public sealed class OneShotApp : MonoBehaviour {
  ArtTheme art;BackendConfiguration backend;CompetitionGateway gateway;VisualElement root;ShotRunner runner;
  CancellationTokenSource lifetime=new CancellationTokenSource();bool busy;string dailyId;
  bool Spanish=>Application.systemLanguage==SystemLanguage.Spanish;
  Localizer copy;
  string L(string key)=>copy.Get(key);
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){
   if(UnityEngine.Object.FindFirstObjectByType<OneShotApp>()==null)new GameObject("ONE SHOT").AddComponent<OneShotApp>();
  }
  void Start(){
   copy=new Localizer(Spanish);Application.targetFrameRate=60;Screen.orientation=ScreenOrientation.Portrait;
   art=Resources.Load<ArtTheme>("OneShotArt")??ScriptableObject.CreateInstance<ArtTheme>();
   backend=Resources.Load<BackendConfiguration>("OneShotBackend")??ScriptableObject.CreateInstance<BackendConfiguration>();
   var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=new Vector2Int(600,1000);panel.match=0.5f;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("OneShot");
   var document=gameObject.AddComponent<UIDocument>();document.panelSettings=panel;root=document.rootVisualElement;
   root.style.backgroundColor=art.paper;root.style.color=art.ink;root.style.flexGrow=1;
   if(art.typography!=null)root.style.unityFontDefinition=FontDefinition.FromFont(art.typography);
   float bottom=Screen.safeArea.y/Screen.height*1000,top=(Screen.height-Screen.safeArea.yMax)/Screen.height*1000;
   root.style.paddingTop=top+30;root.style.paddingBottom=bottom+24;root.style.paddingLeft=32;root.style.paddingRight=32;
   gateway=new CompetitionGateway(this,backend);Home();
  }
  void OnDestroy(){lifetime.Cancel();lifetime.Dispose();gateway?.SignOut();}
  void OnApplicationPause(bool paused){if(paused&&busy)lifetime.Cancel();} // competitive resume not yet implemented
  void Clear(){root.Clear();}
  Label Text(string text,int size=24){var l=new Label(text);l.style.fontSize=size;l.style.color=art.ink;l.style.whiteSpace=WhiteSpace.Normal;l.style.marginBottom=12;root.Add(l);return l;}
  Button Button(string title,Action action,bool primary=false){var b=new Button(action){text=title};b.style.height=72;b.style.marginTop=14;b.style.fontSize=24;b.style.backgroundColor=primary?art.ink:Color.white;b.style.color=primary?art.accent:art.ink;b.style.borderTopLeftRadius=18;b.style.borderTopRightRadius=18;b.style.borderBottomLeftRadius=18;b.style.borderBottomRightRadius=18;root.Add(b);return b;}
  void Spacer(){var s=new VisualElement();s.style.flexGrow=1;root.Add(s);}
  void Home(){if(lifetime.IsCancellationRequested){lifetime.Dispose();lifetime=new CancellationTokenSource();}Clear();busy=false;if(art.logo!=null){var logo=new Image{image=art.logo};logo.style.height=48;logo.style.width=180;root.Add(logo);}else Text("ONE SHOT",28);Text(L("copy_001"),54);Text(L("copy_002"));
   var visual=new ToyCanvas{Kind="collection",Theme=art};visual.style.height=310;root.Add(visual);Spacer();
   Button(L("copy_003"),OpenDaily,true);
   Button(L("copy_004"),()=>PlayPractice());
   Text(L("copy_005"),18);
  }
  async void OpenDaily(){if(busy)return;
   if(!backend.IsConfigured){Notice(L("copy_006"),L("copy_007"));return;}
   if(!gateway.SignedIn){Login();return;}
   busy=true;Clear();Text(L("copy_008"),38);
   try{var s=await gateway.Today(lifetime.Token);if(s==null){Notice(L("copy_009"),L("copy_010"));return;}dailyId=s.shotId;Clear();Text(s.label,52);Text(L("copy_011")+Mathf.RoundToInt(s.durationMs/1000f)+" SEC");Text(L("copy_012"));busy=false;Button(L("copy_013"),PlayDaily,true);Button(L("copy_014"),Home);}catch{Notice(L("copy_015"),L("copy_016"));}finally{busy=false;}
  }
  void Login(){Clear();Text(L("copy_017"),44);Text(L("copy_018"));
   var email=new TextField(L("copy_019"));email.style.height=64;root.Add(email);var password=new TextField(L("copy_020")){isPasswordField=true};password.style.height=64;root.Add(password);
   Button(L("copy_021"),async()=>{if(busy)return;busy=true;try{await gateway.SignIn(email.value,password.value,lifetime.Token);password.value="";busy=false;OpenDaily();}catch{password.value="";Text(L("copy_022"),20);}finally{busy=false;}},true);Button(L("copy_023"),Home);
  }
  async void PlayDaily(){if(busy)return;busy=true;runner=new ShotRunner(gateway,name=>new UnityGame(this));
   try{var r=await runner.RunAsync(dailyId,Guid.NewGuid().ToString(),lifetime.Token);ShowResult(r.Total,false);}
   catch{Clear();Text(L("copy_024"),44);Text(L("copy_025"));if(runner.State==RunnerState.PendingSubmission)Button(L("copy_026"),RetrySubmit,true);Button(L("copy_027"),Home);}finally{busy=false;}
  }
  async void RetrySubmit(){if(busy)return;busy=true;try{var r=await runner.SubmitAsync(lifetime.Token);ShowResult(r.Total,false);}catch{Text(L("copy_028"),20);}finally{busy=false;}}
  async void PlayPractice(){if(busy)return;busy=true;int total=0;try{foreach(var config in PracticeConfigs()){using(var g=new UnityGame(this)){await g.PlayAsync(new RoundConfig(config.game,"2",config.durationMs,JsonUtility.ToJson(config)),lifetime.Token);total+=g.PredictedScore;}}ShowResult(total,true);}catch{Notice(L("copy_029"),L("copy_030"));}finally{busy=false;}}
  void ShowResult(int total,bool practice){Clear();Text(practice?L("copy_031"):L("copy_032"),20);Text(total.ToString("N0"),92);Text(L("copy_033"),30);Spacer();if(!practice)Button(L("copy_034"),World);Button(practice?L("copy_035"):L("copy_036"),practice?(Action)PlayPractice:Home,true);if(practice)Button(L("copy_037"),Home);}
  async void World(){if(busy)return;busy=true;try{var w=await gateway.World(dailyId,lifetime.Token);Text(w.population<20?L("copy_038"):"TOP "+w.topPercent.ToString("0.0")+"%",28);Text(L("copy_039")+w.population+(w.provisional?L("copy_040"):""),20);}catch{Text(L("copy_041"),20);}finally{busy=false;}}
  void Notice(string title,string message){Clear();Text(title,44);Text(message,24);Spacer();Button(L("copy_042"),Home,true);}
  GameConfig[] PracticeConfigs(){var games=new[]{"slice","park","mosquito"};var rounds=new GameConfig[3];for(int r=0;r<3;r++){rounds[r]=new GameConfig{game=games[r],version="2",durationMs=15000,trials=new GameConfig[3]};for(int i=0;i<3;i++){var t=new GameConfig{game=games[r],durationMs=5000,target=games[r]=="slice"?UnityEngine.Random.Range(2500,7500):UnityEngine.Random.Range(5000,8500),tolerance=games[r]=="slice"?5000:3000,speedPerSecond=2200};if(r==2)t.windows=new[]{new Window{start=500,end=1100,x=UnityEngine.Random.Range(2000,8000),y=UnityEngine.Random.Range(2000,8000),radius=1200},new Window{start=1800,end=2400,x=UnityEngine.Random.Range(2000,8000),y=UnityEngine.Random.Range(2000,8000),radius=1200},new Window{start=3200,end=3800,x=UnityEngine.Random.Range(2000,8000),y=UnityEngine.Random.Range(2000,8000),radius=1200}};rounds[r].trials[i]=t;}}return rounds;}
  sealed class UnityGame : IMicrogame {
   readonly OneShotApp app;public int PredictedScore;public UnityGame(OneShotApp a){app=a;}
   public Task<RoundEvidence> PlayAsync(RoundConfig c,CancellationToken token){var completion=new TaskCompletionSource<RoundEvidence>();app.StartCoroutine(app.Round(JsonUtility.FromJson<GameConfig>(c.ParametersJson),token,completion,s=>PredictedScore=s));return completion.Task;}
   public void Dispose(){}
  }
  IEnumerator Round(GameConfig round,CancellationToken token,TaskCompletionSource<RoundEvidence> completion,Action<int> prediction){
   var evidence=new List<string>();int combined=0;
   if(round.trials==null||round.trials.Length!=3){completion.TrySetException(new Exception("Unsupported config"));yield break;}
   for(int i=0;i<3;i++){
    Clear();Text(round.game.ToUpperInvariant()+" · "+(i+1)+"/3",22);
    Text(round.game=="slice"?L("copy_043"):round.game=="park"?L("copy_044"):L("copy_045"),38);
    var config=round.trials[i];Text(round.game=="slice"?(config.target/100f).ToString("0")+" / "+(100-config.target/100f).ToString("0"):round.game=="park"?L("copy_046"):L("copy_047"),24);
    var toy=new ToyCanvas{Kind=round.game,Theme=art,Config=config};toy.style.height=430;toy.style.flexGrow=1;root.Add(toy);var progress=Text("5.0 SEC",20);
    var events=new List<GameEvent>();var hits=new HashSet<int>();bool used=false;int trialScore=0;double start=Time.realtimeSinceStartupAsDouble;Vector2 down=Vector2.zero;
    toy.RegisterCallback<PointerDownEvent>(e=>{down=e.localPosition;toy.CapturePointer(e.pointerId);});
    toy.RegisterCallback<PointerUpEvent>(e=>{
     toy.ReleasePointer(e.pointerId);int time=(int)((Time.realtimeSinceStartupAsDouble-start)*1000);if(time<0||time>config.durationMs)return;
     int x=Mathf.Clamp(Mathf.RoundToInt(e.localPosition.x/toy.resolvedStyle.width*10000),0,10000),y=Mathf.Clamp(Mathf.RoundToInt(e.localPosition.y/toy.resolvedStyle.height*10000),0,10000);
     if(events.Count>0&&time<=events[events.Count-1].t)return;
     if(round.game=="slice"){
      if(used||Mathf.Abs(e.localPosition.y-down.y)<40)return;used=true;
      // Object spans x=.15..85 of stage. Cut must cross its vertical extent.
      float h=toy.resolvedStyle.height;if(Mathf.Min(down.y,e.localPosition.y)>h*.32f||Mathf.Max(down.y,e.localPosition.y)<h*.68f){used=false;return;}
      float crossing=Mathf.Lerp(down.x,e.localPosition.x,(h*.5f-down.y)/(e.localPosition.y-down.y));
      int cut=Mathf.Clamp(Mathf.RoundToInt((crossing/toy.resolvedStyle.width-.15f)/.7f*10000),0,10000);events.Add(new GameEvent{type="cut",t=time,x=cut});trialScore=Scoring.Precision(cut,config.target,config.tolerance);toy.Cut=cut;
     }else if(round.game=="park"){
      if(used)return;used=true;events.Add(new GameEvent{type="brake",t=time});toy.Stopped=true;toy.Position=(int)((long)time*config.speedPerSecond/1000);trialScore=toy.Position>10000?0:Scoring.Precision(toy.Position,config.target,config.tolerance);
     }else{
      if(events.Count>=128)return;events.Add(new GameEvent{type="tap",t=time,x=x,y=y});int index=Array.FindIndex(config.windows,w=>time>=w.start&&time<=w.end&&(long)(x-w.x)*(x-w.x)+(long)(y-w.y)*(y-w.y)<=(long)w.radius*w.radius);if(index>=0&&hits.Add(index))trialScore=Mathf.Max(0,trialScore+10000/config.windows.Length);else trialScore=Mathf.Max(0,trialScore-500);
     }toy.MarkDirtyRepaint();
    });
    while((Time.realtimeSinceStartupAsDouble-start)*1000<config.durationMs){if(token.IsCancellationRequested){completion.TrySetCanceled();yield break;}toy.Elapsed=(int)((Time.realtimeSinceStartupAsDouble-start)*1000);if(!toy.Stopped)toy.Position=(int)((long)toy.Elapsed*config.speedPerSecond/1000);progress.text=Mathf.Max(0,(config.durationMs-toy.Elapsed)/1000f).ToString("0.0")+" SEC";toy.MarkDirtyRepaint();yield return null;}
    if(round.game=="mosquito"){int misses=events.Count-hits.Count;trialScore=Mathf.Max(0,hits.Count*10000/config.windows.Length-misses*500);}
    var parts=new List<string>();foreach(var ev in events)parts.Add(JsonUtility.ToJson(ev));evidence.Add("["+string.Join(",",parts)+"]");combined+=trialScore;
    Clear();Text(L("copy_048"),18);Text(trialScore.ToString("N0"),78);yield return new WaitForSecondsRealtime(.7f);
   }
   prediction(combined/3);completion.TrySetResult(new RoundEvidence("["+string.Join(",",evidence)+"]"));
  }
 }
 public sealed class ToyCanvas:VisualElement {
  public string Kind;public ArtTheme Theme;public GameConfig Config;public int Elapsed,Position,Cut=-1;public bool Stopped;
  public ToyCanvas(){generateVisualContent+=Draw;style.backgroundColor=new Color(1,1,1,.65f);style.borderTopLeftRadius=30;style.borderTopRightRadius=30;style.borderBottomLeftRadius=30;style.borderBottomRightRadius=30;}
  static void TextureQuad(MeshGenerationContext context,Rect rect,Texture2D texture){
   var mesh=context.Allocate(4,6,texture);var uv=mesh.uvRegion;
   mesh.SetNextVertex(new Vertex{position=new Vector3(rect.xMin,rect.yMin,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMin,uv.yMax)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(rect.xMax,rect.yMin,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMax,uv.yMax)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(rect.xMax,rect.yMax,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMax,uv.yMin)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(rect.xMin,rect.yMax,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMin,uv.yMin)});
   foreach(ushort index in new ushort[]{0,1,2,2,3,0})mesh.SetNextIndex(index);
  }
  void Draw(MeshGenerationContext context){var p=context.painter2D;float w=contentRect.width,h=contentRect.height;if(w<1||h<1||Theme==null)return;
   void Box(float x,float y,float width,float height,Color color){p.fillColor=color;p.BeginPath();p.MoveTo(new Vector2(x,y));p.LineTo(new Vector2(x+width,y));p.LineTo(new Vector2(x+width,y+height));p.LineTo(new Vector2(x,y+height));p.ClosePath();p.Fill();}
   void Circle(float x,float y,float r,Color c){p.fillColor=c;p.BeginPath();p.Arc(new Vector2(x,y),r,new Angle(0),new Angle(360),ArcDirection.Clockwise);p.ClosePath();p.Fill();}
   void Line(float x,float y,float xx,float yy,Color color,float thickness){p.strokeColor=color;p.lineWidth=thickness;p.BeginPath();p.MoveTo(new Vector2(x,y));p.LineTo(new Vector2(xx,yy));p.Stroke();}
   var texture=Kind=="slice"?Theme.bread:Kind=="park"?Theme.car:Theme.mosquitoArtwork;
   if(Kind=="collection"){Circle(w*.25f,h*.5f,65,Theme.slice);Box(w*.5f,h*.32f,100,110,Theme.park);Circle(w*.79f,h*.58f,48,Theme.mosquito);return;}
   if(Kind=="slice"){
    Box(w*.15f,h*.32f,w*.7f,h*.36f,Theme.slice);Box(w*.18f,h*.35f,w*.64f,h*.30f,new Color(1f,.89f,.66f));for(int j=0;j<7;j++)Circle(w*(.23f+j*.09f),h*.47f,5,Theme.slice);
    if(texture!=null)TextureQuad(context,new Rect(w*.15f,h*.32f,w*.7f,h*.36f),texture);
    if(Cut>=0)Line(w*(.15f+.7f*Cut/10000f),h*.3f,w*(.15f+.7f*Cut/10000f),h*.7f,Theme.ink,5);
   }else if(Kind=="park"){
    float lane=w*.8f,left=w*.1f,target=left+lane*Config.target/10000f;Box(left,h*.3f,lane,h*.4f,Theme.park);
    Line(target,h*.34f,target,h*.66f,Color.white,5);float x=left+lane*Mathf.Min(Position,11000)/10000f;
    Box(x-28,h*.41f,56,h*.18f,Theme.ink);Box(x-18,h*.43f,36,h*.08f,Theme.accent);if(texture!=null)TextureQuad(context,new Rect(x-28,h*.4f,56,h*.2f),texture);
   }else{
    foreach(var window in Config.windows)if(Elapsed>=window.start&&Elapsed<=window.end){float x=w*window.x/10000f,y=h*window.y/10000f,r=Mathf.Min(w,h)*.035f;Circle(x-r,y,r,Theme.mosquito);Circle(x+r,y,r,Theme.mosquito);Circle(x,y,r*.6f,Theme.ink);Line(x,y,x,y+r*1.6f,Theme.ink,3);if(texture!=null)TextureQuad(context,new Rect(x-r*2,y-r*2,r*4,r*4),texture);}
   }
  }
 }
}
