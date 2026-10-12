using System;
namespace OneShot.Core {
 public static class SliceGeometry {
  static bool Valid(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
  public static bool TryCut(double width,double height,double startX,double startY,double endX,double endY,out int ratio){
   ratio=0;
   if(!Valid(width)||!Valid(height)||!Valid(startX)||!Valid(startY)||!Valid(endX)||!Valid(endY)||width<=0||height<=0)return false;
   if(Math.Abs(endY-startY)<40 || Math.Min(startY,endY)>height*.32 || Math.Max(startY,endY)<height*.68)return false;
   double crossing=startX+(endX-startX)*(height*.5-startY)/(endY-startY);
   double normalized=(crossing/width-.15)/.7;
   if(normalized<0||normalized>1)return false;
   ratio=Math.Min(10000,Math.Max(0,(int)Math.Floor(normalized*10000+.5)));return true;
  }
 }
}
