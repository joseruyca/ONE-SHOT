using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Editor {
 public static class Validation {
  [MenuItem("ONE SHOT/Validate project")]
  public static void Check(){
   ProjectSetup.Ensure();AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
   if(EditorUtility.scriptCompilationFailed)throw new Exception("Unity script compilation failed.");
   if(Resources.Load<StyleSheet>("Studio/Studio")==null)throw new Exception("Studio stylesheet missing.");
   RequireTexture("HeroWide");RequireTexture("Toys");
   if(Resources.Load<TextAsset>("Copy")==null)throw new Exception("Bilingual copy missing.");
   if(!File.Exists("Assets/Scenes/Main.unity"))throw new Exception("Main scene missing.");
   var backend=Resources.Load<OneShot.Client.BackendConfiguration>("OneShotBackend");if(backend==null||!backend.IsConfigured)throw new Exception("Backend configuration missing.");
   Directory.CreateDirectory("Builds");File.WriteAllText("Builds/IMPORT_QA.txt","Unity import and required resources PASS\n"+Application.unityVersion+"\n"+DateTime.UtcNow.ToString("O")+"\nThis check does not verify gameplay, device performance or authenticated server flows.\n");Debug.Log("ONE SHOT import QA PASS");
  }
  static void RequireTexture(string name){
   string path="Assets/Resources/Studio/"+name+".png";
   var asset=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
   if(asset==null)throw new Exception("Artwork did not import as Texture2D: "+path+"; imported type="+AssetDatabase.GetMainAssetTypeAtPath(path)+"; graphics="+SystemInfo.graphicsDeviceType);
   if(asset.width<1||asset.height<1)throw new Exception("Artwork has invalid dimensions: "+path);
   var resource=Resources.Load<Texture2D>("Studio/"+name);
   if(resource==null)throw new Exception("Artwork cannot load through Resources: Studio/"+name+"; imported="+asset.width+"x"+asset.height+"; graphics="+SystemInfo.graphicsDeviceType);
   Debug.Log("ONE SHOT artwork PASS: "+name+" "+asset.width+"x"+asset.height);
  }
 }
}
