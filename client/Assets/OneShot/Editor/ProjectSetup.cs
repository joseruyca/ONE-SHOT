using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using OneShot.Client;
namespace OneShot.Editor {
 [InitializeOnLoad] public static class ProjectSetup {
  static ProjectSetup(){EditorApplication.delayCall+=Ensure;}
  public static void Ensure(){
   Directory.CreateDirectory("Assets/Resources");Directory.CreateDirectory("Assets/Scenes");
   if(!File.Exists("Assets/Resources/OneShotArt.asset"))AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ArtTheme>(),"Assets/Resources/OneShotArt.asset");
   if(!File.Exists("Assets/Resources/OneShotBackend.asset"))AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BackendConfiguration>(),"Assets/Resources/OneShotBackend.asset");
   if(!File.Exists("Assets/Scenes/Main.unity")){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");}
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
   PlayerSettings.companyName="ONE SHOT";PlayerSettings.productName="ONE SHOT";PlayerSettings.bundleVersion="0.6.0";
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.oneshot.daily");PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.oneshot.daily");
   AssetDatabase.SaveAssets();
  }
  public static void EnsureStudioArt(){
   foreach(var name in new[]{"Hero","HeroWide","Toys"}){
    string path="Assets/Resources/Studio/"+name+".png";
    if(!File.Exists(path))throw new System.Exception("Artwork file missing: "+path);
    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
    var importer=AssetImporter.GetAtPath(path) as TextureImporter;
    if(importer==null)throw new System.Exception("TextureImporter missing: "+path);
    bool changed=importer.textureType!=TextureImporterType.Default||importer.textureShape!=TextureImporterShape.Texture2D||importer.mipmapEnabled||importer.maxTextureSize!=2048||importer.npotScale!=TextureImporterNPOTScale.None||!importer.sRGBTexture||importer.wrapMode!=TextureWrapMode.Clamp||importer.filterMode!=FilterMode.Bilinear||!importer.alphaIsTransparency;
    if(changed){importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;importer.sRGBTexture=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.alphaIsTransparency=true;importer.SaveAndReimport();}
   }
  }

 }
 public static class Build {
  public static void Android(){ProjectSetup.Ensure();PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;EditorUserBuildSettings.buildAppBundle=false;Directory.CreateDirectory("Builds");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Main.unity"},"Builds/ONE_SHOT.apk",BuildTarget.Android,BuildOptions.Development);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Android build failed");}
  public static void IOS(){ProjectSetup.Ensure();PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);Directory.CreateDirectory("Builds");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Main.unity"},"Builds/iOS",BuildTarget.iOS,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("iOS export failed");}
 }
}
