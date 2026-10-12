using System;
using System.Collections.Generic;
using UnityEngine;
namespace OneShot.Client {
 public sealed class Localizer {
  [Serializable] sealed class Entry {public string key,es,en;}
  [Serializable] sealed class Catalog {public Entry[] entries;}
  readonly Dictionary<string,string> values=new Dictionary<string,string>();
  public Localizer(bool spanish){var asset=Resources.Load<TextAsset>("Copy");if(asset==null)return;var catalog=JsonUtility.FromJson<Catalog>(asset.text);foreach(var e in catalog.entries)values[e.key]=spanish?e.es:e.en;}
  public string Get(string key)=>values.TryGetValue(key,out var value)?value:key;
 }
}
