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
class Program {
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static async Task MustFail(Func<Task> f){bool failed=false;try{await f();}catch{failed=true;}if(!failed)throw new Exception("Expected failure");}
 static async Task Main(){
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
 }
}
