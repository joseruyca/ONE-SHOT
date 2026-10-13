using System;
using System.Collections.Generic;
using System.IO;
using OneShot.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Client {
 [Serializable] public sealed class PendingRound {public string game,version,parametersJson,eventsJson;public int durationMs;}
 [Serializable] public sealed class PendingRecord {public string owner,shotId,attemptId,configHash,deadlineAt;public PendingRound[] rounds;}
 public sealed class AttemptJournal : IAttemptJournal {
  readonly Func<string> owner,shot;
  public AttemptJournal(Func<string> owner,Func<string> shot){this.owner=owner;this.shot=shot;}
  string PathName {get{if(!Guid.TryParse(owner(),out var identity))throw new InvalidOperationException("Sign in required");return Path.Combine(Application.persistentDataPath,"pending-"+identity.ToString("N")+".json");}}
  public PendingRecord Load(){
   if(!File.Exists(PathName))return null;if(new FileInfo(PathName).Length>131072)throw new InvalidOperationException("Pending data too large");
   var data=JsonUtility.FromJson<PendingRecord>(File.ReadAllText(PathName));
   if(data?.owner!=owner()||!Guid.TryParse(data?.attemptId,out _)||!Guid.TryParse(data?.shotId,out _)||data.rounds==null||data.rounds.Length!=3||string.IsNullOrEmpty(data.configHash)||data.configHash.Length>128)throw new InvalidOperationException("Invalid pending attempt");
   foreach(var round in data.rounds)if(round==null||(round.game!="slice"&&round.game!="park"&&round.game!="mosquito")||round.eventsJson==null||round.parametersJson==null)throw new InvalidOperationException("Invalid pending round");return data;
  }
  public static PendingAttempt Restore(PendingRecord data){var rounds=new List<RoundConfig>();var events=new List<RoundEvidence>();foreach(var r in data.rounds){rounds.Add(new RoundConfig(r.game,r.version,r.durationMs,r.parametersJson));events.Add(new RoundEvidence(r.eventsJson));}return new PendingAttempt(new Attempt(data.attemptId,data.configHash,rounds,data.deadlineAt),events);}
  public void Save(PendingAttempt pending){
   var data=new PendingRecord{owner=owner(),shotId=shot(),attemptId=pending.Attempt.Id,configHash=pending.Attempt.ConfigHash,deadlineAt=pending.Attempt.DeadlineUtc,rounds=new PendingRound[3]};
   for(int i=0;i<3;i++){var r=pending.Attempt.Rounds[i];data.rounds[i]=new PendingRound{game=r.Game,version=r.Version,durationMs=r.DurationMs,parametersJson=r.ParametersJson,eventsJson=pending.Evidence[i].EventsJson};}
   var text=JsonUtility.ToJson(data);if(text.Length>131072)throw new InvalidOperationException("Pending data too large");Directory.CreateDirectory(Application.persistentDataPath);var temp=PathName+".tmp";File.WriteAllText(temp,text);if(File.Exists(PathName))File.Replace(temp,PathName,null);else File.Move(temp,PathName);
  }
  public void Clear(string attemptId){var data=Load();if(data!=null&&data.attemptId==attemptId)File.Delete(PathName);}
  public Action CaptureDiscard(){var name=PathName;return ()=>{if(File.Exists(name))File.Delete(name);};}
  public void Discard(){if(File.Exists(PathName))File.Delete(PathName);}
 }
 public sealed partial class OneShotApp {
  AttemptJournal journal;
  bool PendingScreen(){
   try{var pending=journal.Load();if(pending==null)return false;dailyId=pending.shotId;completedNames.Clear();foreach(var round in pending.rounds)completedNames.Add(round.game);runner=ShotRunner.Restore(gateway,name=>new UnityGame(this),AttemptJournal.Restore(pending),journal);
    Clear();Text(L("pending_title"),44);Text(L("pending_note"),23);Text(L("pending_window_note"),19);Button(L("copy_026"),RetrySubmit,true);Button(L("pending_discard"),ConfirmPendingDiscard);Button(L("copy_014"),Home);return true;
   }catch{Clear();Text(L("pending_title"),44);Text(L("pending_corrupt"),23);Button(L("pending_discard"),ConfirmPendingDiscard);Button(L("copy_014"),Home);return true;}
  }
  void ConfirmPendingDiscard(){Clear();Text(L("pending_discard"),42);Text(L("pending_discard_note"),23);var status=Text("",20);Button(L("pending_confirm"),()=>{try{journal.Discard();runner=null;OpenDaily();}catch{status.text=L("account_unavailable");}});Button(L("cancel"),()=>PendingScreen(),true);}
 }
}
