using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Editor {
 public static class Validation {
  [MenuItem("ONE SHOT/Validate project")]
  public static void Check(){
   ProjectSetup.Ensure();AssetDatabase.Refresh();
   if(EditorUtility.scriptCompilationFailed)throw new Exception("Unity script compilation failed.");
   if(Resources.Load<StyleSheet>("Studio/Studio")==null)throw new Exception("Studio stylesheet missing.");
   if(Resources.Load<Texture2D>("Studio/Hero")==null||Resources.Load<Texture2D>("Studio/Toys")==null)throw new Exception("Studio artwork missing.");
   if(Resources.Load<TextAsset>("Copy")==null)throw new Exception("Bilingual copy missing.");
   if(!File.Exists("Assets/Scenes/Main.unity"))throw new Exception("Main scene missing.");
   var backend=Resources.Load<OneShot.Client.BackendConfiguration>("OneShotBackend");if(backend==null||!backend.IsConfigured)throw new Exception("Backend configuration missing.");
   Directory.CreateDirectory("Builds");File.WriteAllText("Builds/IMPORT_QA.txt","Unity import and required resources PASS\n"+Application.unityVersion+"\n"+DateTime.UtcNow.ToString("O")+"\nThis check does not verify gameplay, device performance or authenticated server flows.\n");Debug.Log("ONE SHOT import QA PASS");
  }
 }
}
