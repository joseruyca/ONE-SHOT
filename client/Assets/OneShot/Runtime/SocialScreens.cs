using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Client {
 [Serializable] public sealed class CrewInfo {public string id,name,code;public bool owner;public int members;}
 [Serializable] public sealed class CrewList {public CrewInfo[] crews;}
 [Serializable] public sealed class CrewResult {public string nickname;public int total,rank;public bool you;}
 [Serializable] public sealed class CrewResults {public CrewResult[] results;}
 [Serializable] public sealed class RunResult {public string startsAt,endsAt;public int total,played,rank,population;public bool qualified,provisional;}
 [Serializable] public sealed class SocialRequest {public string action,name,nickname,code,crewId,shotId;public bool deleteCrew;}
 public sealed partial class OneShotApp {
  async void Crews(){
   if(busy)return;Clear();Header("crews");Text("Crews.",52);Text(L("crews_note"),21);
   if(!gateway.SignedIn){Button(L("account_signin"),()=>Login(Crews),true);Navigation("crews");return;}
   busy=true;var status=Text(L("account_processing"),20);
   try{var crews=await gateway.GetCrews(lifetime.Token);status.RemoveFromHierarchy();if(crews.Length==0)Text(L("crews_empty"),26);
    foreach(var crew in crews){var selected=crew;var card=Button(crew.name+"  ·  "+crew.members+" / 32  →",()=>CrewDetail(selected));card.style.whiteSpace=WhiteSpace.Normal;}
    Button(L("crews_create"),()=>CrewForm(true),true);Button(L("crews_join"),()=>CrewForm(false));
   }catch{status.text=L("account_unavailable");}finally{busy=false;}Navigation("crews");
  }
  void CrewForm(bool creating){Clear();Header("crews");Text(L(creating?"crews_create":"crews_join"),42);Text(L("crew_limits"),20);
   var name=new TextField(L(creating?"crew_name":"crew_code"));name.maxLength=creating?30:16;root.Add(name);var nickname=new TextField(L("crew_nickname")){maxLength=20};root.Add(nickname);var status=Text("",20);
   Button(L(creating?"crews_create":"crews_join"),async()=>{if(busy)return;busy=true;bool navigate=false;status.text=L("account_processing");try{await gateway.Social(new SocialRequest{action=creating?"crew_create":"crew_join",name=creating?name.value:null,code=creating?null:name.value,nickname=nickname.value},lifetime.Token);navigate=true;}catch{status.text=L("crew_failed");}finally{busy=false;}if(navigate)Crews();},true);Button(L("cancel"),()=>{if(!busy)Crews();});
  }
  async void CrewDetail(CrewInfo crew){
   if(busy)return;Clear();Header("crews");Text(crew.name,44);Text(crew.members+" / 32 · "+L("crew_private"),20);
   if(crew.owner&&!string.IsNullOrEmpty(crew.code)){Text(L("crew_code"),18);Text(crew.code,28);Button(L("crew_copy"),()=>{GUIUtility.systemCopyBuffer=crew.code;});Text(L("crew_invite_note"),18);}
   busy=true;var status=Text(L("account_processing"),20);
   try{var today=await gateway.Today(lifetime.Token);if(today==null){status.text=L("copy_009");}else{var results=await gateway.GetCrewResults(crew.id,today.shotId,lifetime.Token);status.RemoveFromHierarchy();Text(L("crew_standings"),28);foreach(var result in results){var row=Text(result.rank+"  "+result.nickname+(result.you?" · "+L("you"):"")+"   "+result.total.ToString("N0"),25);if(result.you)row.style.color=new Color32(40,78,207,255);}}}
   catch{status.text=L("crew_complete_first");}finally{busy=false;}
   Button(L(crew.owner?"crew_delete":"crew_leave"),()=>ConfirmCrewRemove(crew));Button(L("copy_014"),Crews);Navigation("crews");
  }
  void ConfirmCrewRemove(CrewInfo crew){Clear();Text(L(crew.owner?"crew_delete":"crew_leave"),42);Text(L(crew.owner?"crew_delete_note":"crew_leave_note"),22);var status=Text("",20);
   Button(L("crew_confirm"),async()=>{if(busy)return;busy=true;bool navigate=false;try{await gateway.Social(new SocialRequest{action="crew_remove",crewId=crew.id,deleteCrew=crew.owner},lifetime.Token);navigate=true;}catch{status.text=L("account_unavailable");}finally{busy=false;}if(navigate)Crews();});Button(L("cancel"),()=>{if(!busy)CrewDetail(crew);},true);
  }
  async void Runs(){if(busy)return;Clear();Header("run");Text("Run.",54);Text(L("run_note"),22);
   if(!gateway.SignedIn){Button(L("account_signin"),()=>Login(Runs),true);Button(L("copy_014"),Profile);return;}
   busy=true;var status=Text(L("account_processing"),20);
   try{var run=await gateway.GetRun(lifetime.Token);status.RemoveFromHierarchy();Text(run.total.ToString("N0"),78);Text(run.startsAt+" → "+run.endsAt,20);Text(run.played+" / 14 · "+L("run_days"),25);
    Text(L(run.qualified?"run_qualified":"run_pending"),24);if(run.qualified)Text("#"+run.rank+" · "+run.population+" · "+L("run_provisional"),24);
   }catch{status.text=L("account_unavailable");}finally{busy=false;}Button(L("copy_014"),Profile,true);Navigation("profile");
  }
 }
}
