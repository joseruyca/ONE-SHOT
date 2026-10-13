using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Client {
 public sealed partial class OneShotApp {
  bool competitiveSession,gameRunning;
  void ResetCancellation(){if(!lifetime.IsCancellationRequested)return;lifetime.Dispose();lifetime=new System.Threading.CancellationTokenSource();}
  void Header(string section){
   var header=new VisualElement();header.AddToClassList("header");var brand=new Label("ONE SHOT.");brand.AddToClassList("wordmark");header.Add(brand);
   var badge=new Label(section=="today"?DateTime.Now.ToString("dd MMM",Spanish?new CultureInfo("es-ES"):CultureInfo.InvariantCulture).ToUpperInvariant():L(section=="train"?"nav_train":section=="profile"?"nav_profile":section=="crews"?"nav_crews":section=="run"?"run_title":"copy_033").ToUpperInvariant());badge.AddToClassList("eyebrow");header.Add(badge);root.Add(header);
  }
  void GameCard(string game,Action action){
   var card=new Button(action);card.AddToClassList("game-card");
   var artwork=new ToyCanvas{Kind=game,Theme=art,Preview=true,ReducedMotion=ReducedMotion,Config=new GameConfig{target=6500,windows=new[]{new Window{start=0,end=1000,x=5000,y=5000}}},Elapsed=500,Position=6500};artwork.pickingMode=PickingMode.Ignore;artwork.style.overflow=Overflow.Hidden;artwork.style.width=130;artwork.style.height=100;artwork.style.marginTop=0;artwork.style.marginBottom=0;artwork.style.backgroundColor=Color.clear;card.Add(artwork);
   var info=new VisualElement();info.style.flexGrow=1;info.style.justifyContent=Justify.Center;var name=new Label(L(game+"_name"));name.AddToClassList("game-name");var trait=new Label(L(game+"_trait"));trait.AddToClassList("game-trait");info.Add(name);info.Add(trait);card.Add(info);
   var arrow=new Label("↗");arrow.style.fontSize=32;arrow.style.marginRight=16;card.Add(arrow);root.Add(card);
  }
  void RoundHeader(string game,int trial){
   var row=new VisualElement();row.AddToClassList("round-header");var name=new Label(L(game+"_name"));name.AddToClassList("eyebrow");row.Add(name);var steps=new VisualElement();steps.style.flexDirection=FlexDirection.Row;
   for(int i=0;i<3;i++){var step=new VisualElement();step.name="round-step-"+i;step.AddToClassList("round-step");step.style.backgroundColor=i<=trial?art.accent:(Color)new Color32(225,229,240,255);steps.Add(step);}row.Add(steps);var exit=new Button(AskQuit){text="×"};exit.AddToClassList("close-game");row.Add(exit);root.Add(row);
  }
  void HandleCancel(){var modal=shell.Q<VisualElement>("quit-modal");if(modal!=null){modal.RemoveFromHierarchy();return;}if(busy){if(gameRunning)AskQuit();}else Home();}
  void AskQuit(){
   if(!gameRunning||shell.Q<VisualElement>("quit-modal")!=null)return;
   // Clock keeps running: opening a dialog cannot pause a competitive attempt.
   var overlay=new VisualElement{name="quit-modal"};overlay.AddToClassList("modal-overlay");var panel=new VisualElement();panel.AddToClassList("modal-panel");var title=new Label(L("quit_title"));title.AddToClassList("modal-title");panel.Add(title);
   var note=new Label(L(competitiveSession?"quit_daily":"quit_practice"));note.style.whiteSpace=WhiteSpace.Normal;note.style.fontSize=22;panel.Add(note);
   var stay=new Button(()=>overlay.RemoveFromHierarchy()){text=L("continue_game")};stay.AddToClassList("modal-action");panel.Add(stay);var leave=new Button(()=>{overlay.RemoveFromHierarchy();lifetime.Cancel();}){text=L("quit_confirm")};leave.AddToClassList("modal-action");panel.Add(leave);overlay.Add(panel);shell.Add(overlay);
  }
  void History(){Clear();Header("profile");Text(L("history_title"),42);Text(L("history_note"),19);var journal=PracticeJournal.Load();if(journal.sessions.Count==0)Text(L("history_empty"),26);
   for(int i=journal.sessions.Count-1;i>=0;i--){var session=journal.sessions[i];var item=new VisualElement();item.AddToClassList("history-card");DateTime date;var stamp=DateTime.TryParse(session.utc,null,DateTimeStyles.RoundtripKind,out date)?date.ToLocalTime().ToString("dd MMM · HH:mm"):"—";
    var heading=new Label(stamp+"    "+session.total.ToString("N0"));heading.style.fontSize=25;heading.style.unityFontStyleAndWeight=FontStyle.Bold;item.Add(heading);for(int j=0;j<session.games.Length;j++){var line=new Label(session.games[j].ToUpperInvariant()+"   "+session.scores[j].ToString("N0"));line.style.fontSize=18;item.Add(line);}root.Add(item);
   }Button(L("copy_014"),Profile);Navigation("profile");
  }
  void ConfirmReset(){Clear();Text(L("reset_title"),44);Text(L("reset_note"),23);Button(L("confirm_delete"),()=>{foreach(var game in new[]{"slice","park","mosquito"})PlayerPrefs.DeleteKey("oneshot.practice."+game);PlayerPrefs.Save();PracticeJournal.Clear();Profile();});Button(L("cancel"),Profile,true);}
  async void Logout(){if(busy)return;busy=true;try{await gateway.SignOutRemote(lifetime.Token);Profile();}catch{Notice(L("account_signout"),L("account_unavailable"));}finally{busy=false;}}
  void DeleteAccountScreen(){
   Clear();Text(L("delete_account_title"),42);Text(L("delete_account_note"),22);
   var email=new TextField(L("copy_019"));root.Add(email);var password=new TextField(L("copy_020")){isPasswordField=true};root.Add(password);
   var confirmation=new TextField(L("delete_account_type"));root.Add(confirmation);var local=new Toggle(L("delete_local_too")){value=true};root.Add(local);var status=Text("",20);
   Button(L("delete_account"),async()=>{
    if(busy)return;if(confirmation.value!="BORRAR"||string.IsNullOrWhiteSpace(email.value)||string.IsNullOrWhiteSpace(password.value)){status.text=L("delete_account_type");return;}
    busy=true;status.text=L("account_processing");
    try{await gateway.SignIn(email.value.Trim(),password.value,lifetime.Token);password.value="";var clearPending=journal.CaptureDiscard();await gateway.DeleteAccount(lifetime.Token);try{clearPending();}catch(System.IO.IOException){Debug.LogWarning("Pending local data could not be cleared.");}catch(System.UnauthorizedAccessException){Debug.LogWarning("Pending local data could not be cleared.");}
     if(local.value){foreach(var game in new[]{"slice","park","mosquito"})PlayerPrefs.DeleteKey("oneshot.practice."+game);PlayerPrefs.Save();PracticeJournal.Clear();}
     Notice(L("delete_account_done"),L("delete_account_done_note"));
    }catch{password.value="";status.text=L("account_unavailable");}finally{busy=false;}
   });Button(L("cancel"),()=>{if(!busy)Profile();},true);
  }
 }
}
