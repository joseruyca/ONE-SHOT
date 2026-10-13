using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneShot.Core;
class Gateway : ICompetitionGateway {
 public int Starts, Submits; public bool FailSubmit;
 public Task<Attempt> StartAsync(string s,string k,CancellationToken c) {
  Starts++;
  return Task.FromResult(new Attempt("id","hash",new List<RoundConfig>{new RoundConfig("slice","1",15000,"{}"),new RoundConfig("park","1",15000,"{}"),new RoundConfig("mosquito","1",15000,"{}")}));
 }
 public Task<VerifiedResult> SubmitAsync(Attempt a,IReadOnlyList<RoundEvidence> e,CancellationToken c) {
  Submits++;if(FailSubmit)throw new InvalidOperationException("Network failure");
  if(e.Count!=3)throw new Exception("Missing rounds"); return Task.FromResult(new VerifiedResult(20000));
 }
}
class Game : IMicrogame {
 public static int Played,Disposed;public bool Fail;
 public Task<RoundEvidence> PlayAsync(RoundConfig c,CancellationToken token) {
  Played++;if(Fail)throw new Exception("Game failure");return Task.FromResult(new RoundEvidence("[]"));
 }
 public void Dispose(){Disposed++;}
}
class Journal : IAttemptJournal {
 public PendingAttempt Pending;public bool FailSave,FailClear;public int Saves,Clears;
 public void Save(PendingAttempt pending){Saves++;if(FailSave)throw new Exception("Disk failure");Pending=pending;}
 public void Clear(string attemptId){Clears++;if(FailClear)throw new Exception("Disk failure");if(Pending?.Attempt.Id!=attemptId)throw new Exception("Wrong attempt cleared");Pending=null;}
}
class Program {
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static async Task MustFail(Func<Task> f){bool failed=false;try{await f();}catch{failed=true;}if(!failed)throw new Exception("Expected failure");}
 static async Task Main(){
  ResultValidation.Validate(27000,new[]{10000,9000,8000});
  Check(true,"verified round breakdown");
  await MustFail(()=>{ResultValidation.Validate(30000,new[]{10000,9000,8000});return Task.CompletedTask;});
  await MustFail(()=>{ResultValidation.Validate(0,new[]{-1,0,0});return Task.CompletedTask;});
  await MustFail(()=>{ResultValidation.Validate(0,null);return Task.CompletedTask;});
  Check(SliceGeometry.TryCut(100,100,40.9,0,40.9,100,out var cut)&&cut==3700,"slice crossing ratio");
  Check(!SliceGeometry.TryCut(100,100,0,0,0,100,out _),"outside object is not a cut");
  Check(!SliceGeometry.TryCut(100,100,40,40,40,60,out _),"short gesture is not a cut");
  Check(!SliceGeometry.TryCut(100,100,double.NaN,0,40,100,out _),"nonfinite gesture rejected");
  Check(Scoring.Precision(3700,3700,5000)==10000,"perfect precision");
  Check(Scoring.Precision(9000,3700,5000)==0,"bounded precision");
  var gateway=new Gateway();var runner=new ShotRunner(gateway,_=>new Game());
  var result=await runner.RunAsync("today","key",CancellationToken.None);
  Check(result.Total==20000&&runner.State==RunnerState.Completed,"verified result only");
  Check(Game.Played==3&&Game.Disposed==3,"three rounds disposed");
  await MustFail(()=>runner.RunAsync("today","key",CancellationToken.None));
  Check(gateway.Starts==1,"double start prevented");
  gateway=new Gateway{FailSubmit=true};runner=new ShotRunner(gateway,_=>new Game());
  await MustFail(()=>runner.RunAsync("today","key",CancellationToken.None));
  Check(runner.State==RunnerState.PendingSubmission,"network failure keeps pending evidence");
  gateway.FailSubmit=false;await runner.SubmitAsync(CancellationToken.None);
  Check(gateway.Starts==1&&gateway.Submits==2&&runner.State==RunnerState.Completed,"submit retry does not replay");
  gateway=new Gateway();runner=new ShotRunner(gateway,_=>new Game{Fail=true});
  await MustFail(()=>runner.RunAsync("today","key",CancellationToken.None));
  Check(runner.State==RunnerState.Interrupted&&gateway.Submits==0,"incomplete shot never submitted");
  await MustFail(()=>runner.RunAsync("today","new-key",CancellationToken.None));
  Check(gateway.Starts==1,"interrupted attempt cannot locally restart");
  var journal=new Journal();gateway=new Gateway{FailSubmit=true};runner=new ShotRunner(gateway,_=>new Game(),journal);
  await MustFail(()=>runner.RunAsync("today","key",CancellationToken.None));
  Check(journal.Pending!=null&&journal.Pending.Evidence.Count==3,"complete evidence checkpoint survives failed submit");
  int before=Game.Played;gateway=new Gateway();var restored=ShotRunner.Restore(gateway,_=>new Game(),journal.Pending,journal);
  await MustFail(()=>restored.RunAsync("today","other-key",CancellationToken.None));
  await restored.SubmitAsync(CancellationToken.None);
  Check(Game.Played==before&&gateway.Starts==0&&gateway.Submits==1,"restored submission cannot replay or start a new attempt");
  Check(journal.Pending==null&&journal.Clears==1,"checkpoint cleared only after verified acknowledgement");
  journal=new Journal{FailSave=true};gateway=new Gateway();runner=new ShotRunner(gateway,_=>new Game(),journal);
  await MustFail(()=>runner.RunAsync("today","key",CancellationToken.None));
  Check(gateway.Submits==0&&runner.State==RunnerState.PendingSubmission,"failed durable write prevents uncheckpointed submission");
  journal.FailSave=false;journal.FailClear=true;var accepted=await runner.SubmitAsync(CancellationToken.None);
  Check(accepted.Total==20000&&runner.State==RunnerState.Completed&&runner.JournalCleanupFailed,"local cleanup failure does not undo verified server result");
  await MustFail(()=>{new PendingAttempt(journal.Pending.Attempt,new List<RoundEvidence>{new RoundEvidence("[]")});return Task.CompletedTask;});
  Check(true,"incomplete recovery data is rejected");
 }
}
