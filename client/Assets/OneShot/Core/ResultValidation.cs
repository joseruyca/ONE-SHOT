using System;
namespace OneShot.Core {
 public static class ResultValidation {
  public static void Validate(int total,int[] scores){
   if(scores==null||scores.Length!=3)throw new ArgumentException("Three server scores required");
   int sum=0;foreach(var score in scores){if(score<0||score>10000)throw new ArgumentOutOfRangeException(nameof(scores));sum+=score;}
   if(total!=sum)throw new ArgumentException("Total does not match round scores");
  }
 }
}
