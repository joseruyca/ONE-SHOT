using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace OneShot.Client {
 [Serializable] public sealed class PracticeSession {public string utc;public string[] games;public int[] scores;public int total;}
 [Serializable] public sealed class PracticeLog {public List<PracticeSession> sessions=new List<PracticeSession>();}
 public static class PracticeJournal {
  static string PathName=>Path.Combine(Application.persistentDataPath,"practice-history.json");
  public static PracticeLog Load(){
   try{if(!File.Exists(PathName))return new PracticeLog();var log=JsonUtility.FromJson<PracticeLog>(File.ReadAllText(PathName));if(log?.sessions==null)return new PracticeLog();log.sessions.RemoveAll(s=>s==null||s.games==null||s.scores==null||s.games.Length!=s.scores.Length||s.games.Length<1||s.games.Length>3||Array.Exists(s.scores,v=>v<0||v>10000)||Array.Exists(s.games,g=>g!="slice"&&g!="park"&&g!="mosquito"));if(log.sessions.Count>50)log.sessions.RemoveRange(0,log.sessions.Count-50);return log;
   }catch(IOException){return new PracticeLog();}catch(ArgumentException){return new PracticeLog();}
  }
  public static bool Record(List<string> games,List<int> scores,int total){
   var log=Load();log.sessions.Add(new PracticeSession{utc=DateTime.UtcNow.ToString("O"),games=games.ToArray(),scores=scores.ToArray(),total=total});if(log.sessions.Count>50)log.sessions.RemoveAt(0);
   try{Directory.CreateDirectory(Application.persistentDataPath);var temporary=PathName+".tmp";File.WriteAllText(temporary,JsonUtility.ToJson(log));if(File.Exists(PathName))File.Replace(temporary,PathName,null);else File.Move(temporary,PathName);return true;}catch(IOException){return false;}catch(UnauthorizedAccessException){return false;}
  }
  public static bool Clear(){try{if(File.Exists(PathName))File.Delete(PathName);return true;}catch(IOException){return false;}catch(UnauthorizedAccessException){return false;}}
 }
}
