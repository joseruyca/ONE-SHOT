using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
var directory=Path.Combine(Directory.GetCurrentDirectory(),"client","Assets");
if(!Directory.Exists(directory))throw new Exception("Run from repository root");
var failures=0;var files=Directory.GetFiles(directory,"*.cs",SearchOption.AllDirectories);
foreach(var file in files){
 var tree=CSharpSyntaxTree.ParseText(File.ReadAllText(file),new CSharpParseOptions(LanguageVersion.CSharp9),file);
 foreach(var diagnostic in tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error)){Console.WriteLine(diagnostic);failures++;}
}
Console.WriteLine($"Parsed {files.Length} Unity C# files; syntax errors: {failures}. This does not typecheck Unity APIs or build the app.");
Environment.ExitCode=failures==0?0:1;
