using System;
using UnityEngine;
namespace OneShot.Client {
 [CreateAssetMenu(menuName="ONE SHOT/Backend Configuration")]
 public sealed class BackendConfiguration : ScriptableObject {
  [Tooltip("HTTPS project origin. Publishable key only. NEVER service_role.")]
  public string supabaseUrl="",publishableKey="";
  public bool IsConfigured => Uri.TryCreate(supabaseUrl,UriKind.Absolute,out var uri) && uri.Scheme=="https" && uri.AbsolutePath=="/" && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment) && string.IsNullOrEmpty(uri.UserInfo) && !string.IsNullOrEmpty(publishableKey);
 }
}
