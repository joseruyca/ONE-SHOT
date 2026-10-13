using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace OneShot.Core
{
    public sealed class RoundConfig
    {
        public string Game { get; }
        public string Version { get; }
        public int DurationMs { get; }
        public string ParametersJson { get; }
        public RoundConfig(string game, string version, int durationMs, string parametersJson)
        {
            if (string.IsNullOrWhiteSpace(game) || string.IsNullOrWhiteSpace(version) || durationMs < 1000 || durationMs > 25000 || parametersJson == null)
                throw new ArgumentException("Invalid round config");
            Game=game; Version=version; DurationMs=durationMs; ParametersJson=parametersJson;
        }
    }
    public sealed class Attempt
    {
        public string Id { get; }
        public string ConfigHash { get; }
        public string DeadlineUtc { get; }
        public IReadOnlyList<RoundConfig> Rounds { get; }
        public Attempt(string id, string hash, IList<RoundConfig> rounds,string deadlineUtc=null)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(hash) || rounds == null || rounds.Count != 3)
                throw new ArgumentException("Invalid attempt");
            foreach (var round in rounds) if (round == null) throw new ArgumentException("Null round");
            Id=id; ConfigHash=hash;DeadlineUtc=deadlineUtc; Rounds=new List<RoundConfig>(rounds).AsReadOnly();
        }
    }
    public sealed class RoundEvidence
    {
        public string EventsJson { get; }
        public RoundEvidence(string json) { EventsJson=json ?? throw new ArgumentNullException(nameof(json)); }
    }
    public sealed class VerifiedResult
    {
        public int Total { get; }
        public VerifiedResult(int total)
        { if(total<0 || total>30000) throw new ArgumentOutOfRangeException(nameof(total)); Total=total; }
    }
    public interface IMicrogame : IDisposable
    {
        Task<RoundEvidence> PlayAsync(RoundConfig config, CancellationToken cancellation);
    }
    public interface ICompetitionGateway
    {
        // MUST durably consume attempt on server. Repeated key returns the same attempt.
        Task<Attempt> StartAsync(string shotId, string idempotencyKey, CancellationToken cancellation);
        // MUST use identity from verified auth, not supplied client user ID.
        Task<VerifiedResult> SubmitAsync(Attempt attempt, IReadOnlyList<RoundEvidence> evidence, CancellationToken cancellation);
    }
    public sealed class PendingAttempt {
        public Attempt Attempt { get; }
        public IReadOnlyList<RoundEvidence> Evidence { get; }
        public PendingAttempt(Attempt attempt,IReadOnlyList<RoundEvidence> evidence) {
            if(attempt==null||evidence==null||evidence.Count!=3)throw new ArgumentException("Complete attempt required");
            var snapshot=new List<RoundEvidence>();foreach(var item in evidence){if(item==null)throw new ArgumentException("Missing evidence");snapshot.Add(item);}Attempt=attempt;Evidence=snapshot.AsReadOnly();
        }
    }
    public interface IAttemptJournal {
        void Save(PendingAttempt pending);
        void Clear(string attemptId);
    }
    public enum RunnerState { Idle, Starting, Playing, PendingSubmission, Submitting, Completed, Interrupted }
    public sealed class ShotRunner
    {
        private readonly ICompetitionGateway gateway;
        private readonly Func<string, IMicrogame> factory;
        private readonly List<RoundEvidence> evidence=new List<RoundEvidence>();
        private Attempt attempt;
        private readonly IAttemptJournal journal;
        public bool JournalCleanupFailed { get; private set; }
        public RunnerState State { get; private set; }=RunnerState.Idle;
        public ShotRunner(ICompetitionGateway gateway, Func<string, IMicrogame> factory,IAttemptJournal journal=null)
        { this.gateway=gateway ?? throw new ArgumentNullException(nameof(gateway)); this.factory=factory ?? throw new ArgumentNullException(nameof(factory));this.journal=journal; }
        public static ShotRunner Restore(ICompetitionGateway gateway,Func<string,IMicrogame> factory,PendingAttempt pending,IAttemptJournal journal=null) {
            if(pending==null)throw new ArgumentNullException(nameof(pending));var runner=new ShotRunner(gateway,factory,journal);runner.attempt=pending.Attempt;runner.evidence.AddRange(pending.Evidence);runner.State=RunnerState.PendingSubmission;return runner;
        }
        public async Task<VerifiedResult> RunAsync(string shotId, string startKey, CancellationToken cancellation)
        {
            if(State!=RunnerState.Idle) throw new InvalidOperationException("Attempt already running or consumed");
            if(string.IsNullOrWhiteSpace(shotId)||string.IsNullOrWhiteSpace(startKey)) throw new ArgumentException("Missing identity");
            State=RunnerState.Starting;
            try {
                attempt=await gateway.StartAsync(shotId,startKey,cancellation);
                if(attempt==null) throw new InvalidOperationException("Missing server attempt");
                State=RunnerState.Playing;
                foreach(var config in attempt.Rounds) {
                    cancellation.ThrowIfCancellationRequested();
                    using(var game=factory(config.Game)) {
                        if(game==null) throw new InvalidOperationException("Game unavailable");
                        var result=await game.PlayAsync(config,cancellation);
                        evidence.Add(result ?? throw new InvalidOperationException("Missing evidence"));
                    }
                }
            } catch { State=RunnerState.Interrupted; throw; }
            State=RunnerState.PendingSubmission;
            return await SubmitAsync(cancellation);
        }
        public async Task<VerifiedResult> SubmitAsync(CancellationToken cancellation)
        {
            if(State!=RunnerState.PendingSubmission) throw new InvalidOperationException("No complete pending attempt");
            // Persist complete evidence BEFORE sending. A failed write leaves the runner retryable.
            journal?.Save(new PendingAttempt(attempt,evidence.AsReadOnly()));
            State=RunnerState.Submitting;
            try {
                var result=await gateway.SubmitAsync(attempt,evidence.AsReadOnly(),cancellation);
                if(result==null) throw new InvalidOperationException("Missing verified result");
                State=RunnerState.Completed;
                try{journal?.Clear(attempt.Id);}catch{JournalCleanupFailed=true;}
                return result;
            } catch { State=RunnerState.PendingSubmission; throw; }
        }
        // No Reset: a new runner cannot grant a new server attempt.
        // Restored evidence can only be submitted; it cannot replay consumed rounds.
    }
}
