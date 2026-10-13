using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Client {
 // Original 2D characters. Their motion changes expression, not the authoritative hit areas.
 public sealed class ToyCanvas:VisualElement {
  public string Kind;public ArtTheme Theme;public GameConfig Config;
  public int Elapsed,Position,Cut=-1,FeedbackAt=-1000;
  public bool Stopped,Hit,Dragging,Preview,ReducedMotion;
  public Vector2 StrokeStart,StrokeEnd,ImpactPoint;
  public readonly HashSet<int> Caught=new HashSet<int>();
  public float ImpactTime=-100,Clock;
  public ToyCanvas(){
   generateVisualContent+=Draw;style.overflow=Overflow.Hidden;style.backgroundColor=Color.white;
   style.borderTopLeftRadius=30;style.borderTopRightRadius=30;style.borderBottomLeftRadius=30;style.borderBottomRightRadius=30;
   style.marginTop=12;style.marginBottom=12;
   schedule.Execute(()=>{Clock=Time.realtimeSinceStartup;MarkDirtyRepaint();}).Every(16);
   RegisterCallback<PointerDownEvent>(e=>{if(Kind=="collection"){ImpactPoint=e.localPosition;ImpactTime=Time.realtimeSinceStartup;Hit=true;}});
  }
  public void ResetTrial(GameConfig config){Config=config;Elapsed=0;Position=0;Cut=-1;Stopped=false;Dragging=false;FeedbackAt=-1000;Caught.Clear();}
  public void React(Vector2 point,bool hit){ImpactPoint=point;ImpactTime=Time.realtimeSinceStartup;Hit=hit;MarkDirtyRepaint();}
  static void TextureQuad(MeshGenerationContext c,Rect r,Texture2D texture){
   var mesh=c.Allocate(4,6,texture);
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMin,r.yMin,Vertex.nearZ),tint=Color.white,uv=new Vector2(0,1)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMax,r.yMin,Vertex.nearZ),tint=Color.white,uv=new Vector2(1,1)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMax,r.yMax,Vertex.nearZ),tint=Color.white,uv=new Vector2(1,0)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMin,r.yMax,Vertex.nearZ),tint=Color.white,uv=new Vector2(0,0)});
   foreach(ushort index in new ushort[]{0,1,2,2,3,0})mesh.SetNextIndex(index);
  }
  void Draw(MeshGenerationContext context){
   float w=contentRect.width,h=contentRect.height;if(w<1||h<1||Theme==null)return;var p=context.painter2D;
   Color ink=Theme.ink,coral=Theme.accent;float time=ReducedMotion?0:Clock;
   float age=Clock-ImpactTime,kick=ReducedMotion?0:Mathf.Sin(Mathf.Clamp01(age/.4f)*Mathf.PI)*Mathf.Clamp01(1-age/.4f);
   void Box(float x,float y,float ww,float hh,Color c){p.fillColor=c;p.BeginPath();p.MoveTo(new Vector2(x,y));p.LineTo(new Vector2(x+ww,y));p.LineTo(new Vector2(x+ww,y+hh));p.LineTo(new Vector2(x,y+hh));p.ClosePath();p.Fill();}
   void Circle(float x,float y,float radius,Color c){if(radius<=0)return;p.fillColor=c;p.BeginPath();p.Arc(new Vector2(x,y),radius,new Angle(0),new Angle(360),ArcDirection.Clockwise);p.ClosePath();p.Fill();}
   void Line(float x,float y,float xx,float yy,Color c,float thickness){p.strokeColor=c;p.lineWidth=thickness;p.BeginPath();p.MoveTo(new Vector2(x,y));p.LineTo(new Vector2(xx,yy));p.Stroke();}
   void Round(float x,float y,float ww,float hh,float radius,Color c){if(ww<=0||hh<=0)return;float r=Mathf.Min(radius,Mathf.Min(ww,hh)/2);Box(x+r,y,ww-2*r,hh,c);Box(x,y+r,ww,hh-2*r,c);Circle(x+r,y+r,r,c);Circle(x+ww-r,y+r,r,c);Circle(x+r,y+hh-r,r,c);Circle(x+ww-r,y+hh-r,r,c);}
   void Spark(float x,float y,float size,Color c){Line(x-size,y,x+size,y,c,3);Line(x,y-size,x,y+size,c,3);}
   void Face(float x,float y,float size,bool smile){
    float blink=(time%4.6f>4.45f)?0.2f:1;
    for(int i=-1;i<=1;i+=2){float xx=x+i*size*.22f;Round(xx-size*.09f,y-size*.10f,size*.18f,size*.22f*blink,size*.06f,ink);}
    if(smile){Line(x-size*.12f,y+size*.19f,x,y+size*.26f,ink,size*.04f);Line(x,y+size*.26f,x+size*.12f,y+size*.19f,ink,size*.04f);}else Circle(x,y+size*.23f,size*.07f,ink);
   }
   void Bread(float x,float y,float ww,float hh){
    if(Theme.useCustomArtwork&&Theme.bread!=null){TextureQuad(context,new Rect(x,y,ww,hh),Theme.bread);return;}
    float gap=Cut>=0?8+kick*13:0,split=Mathf.Clamp(Cut/10000f,.001f,.999f);
    void Piece(float xx,float width,float faceX){
     Round(xx,y+8,width,hh,22,new Color(0.09f,0.15f,0.28f,.09f));Round(xx,y,width,hh,22,ink);Round(xx+5,y+5,width-10,hh-10,18,Theme.slice);Round(xx+12,y+12,width-24,hh-24,13,new Color32(255,238,189,255));
     if(width>ww*.22f)Face(faceX,y+hh*.48f,Mathf.Min(hh,width)*.63f,Cut<0||Hit);
    }
    if(Cut<0)Piece(x,ww,x+ww*.5f);else{Piece(x-gap,ww*split,x+ww*split*.5f-gap);Piece(x+ww*split+gap,ww*(1-split),x+ww*(split+(1-split)*.5f)+gap);}
    for(int i=0;i<4;i++){float xx=x+ww*(.16f+.21f*i);if(Cut>=0)xx+=i*.21f+.16f<split?-gap:gap;Line(xx,y+hh*.17f,xx+ww*.055f,y+hh*.24f,Theme.slice,5);}
   }
   void Wheel(float x,float y,float r){Circle(x,y,r,ink);Circle(x,y,r*.47f,Color.white);float a=Stopped?0:time*8;Line(x-Mathf.Cos(a)*r*.32f,y-Mathf.Sin(a)*r*.32f,x+Mathf.Cos(a)*r*.32f,y+Mathf.Sin(a)*r*.32f,Theme.park,3);}
   void Car(float x,float y,float size){
    if(Theme.useCustomArtwork&&Theme.car!=null){TextureQuad(context,new Rect(x-size*.5f,y-size*.4f,size,size*.8f),Theme.car);return;}
    float bounce=Stopped?kick*4:Mathf.Sin(time*16)*size*.015f;y-=bounce;
    Round(x-size*.5f,y+size*.18f,size,size*.10f,8,new Color(.09f,.15f,.28f,.09f));
    Round(x-size*.29f,y-size*.31f,size*.57f,size*.39f,size*.12f,ink);Round(x-size*.23f,y-size*.25f,size*.45f,size*.31f,size*.08f,Color.white);
    Line(x,y-size*.24f,x,y+size*.02f,ink,4);
    Round(x-size*.48f,y-size*.03f,size*.96f,size*.31f,size*.11f,ink);Round(x-size*.43f,y+size*.01f,size*.86f,size*.22f,size*.08f,Theme.park);
    Circle(x+size*.36f,y+size*.10f,size*.055f,new Color32(255,227,125,255));Wheel(x-size*.28f,y+size*.24f,size*.12f);Wheel(x+size*.28f,y+size*.24f,size*.12f);
    if(!Stopped&&!ReducedMotion)for(int i=0;i<3;i++)Line(x-size*(.65f+i*.12f),y-size*.05f+i*size*.09f,x-size*.53f,y-size*.05f+i*size*.09f,Theme.park,3);
   }
   void Bug(float x,float y,float radius){
    if(Theme.useCustomArtwork&&Theme.mosquitoArtwork!=null){TextureQuad(context,new Rect(x-radius,y-radius,radius*2,radius*2),Theme.mosquitoArtwork);return;}
    // Wings flutter around a fixed centre; the visible body stays in the server hit circle.
    float flutter=1+Mathf.Sin(time*35)*.14f;
    Circle(x-radius*.50f,y-radius*.18f,radius*.45f*flutter,new Color32(214,228,255,255));Circle(x+radius*.50f,y-radius*.18f,radius*.45f*flutter,new Color32(214,228,255,255));
    Line(x-radius*.22f,y+radius*.25f,x-radius*.49f,y+radius*.61f,ink,3);Line(x+radius*.22f,y+radius*.25f,x+radius*.49f,y+radius*.61f,ink,3);
    Circle(x,y+radius*.11f,radius*.40f,ink);Circle(x,y+radius*.08f,radius*.31f,Theme.mosquito);
    for(int i=-1;i<=1;i+=2){Circle(x+i*radius*.20f,y-radius*.10f,radius*.19f,Color.white);Circle(x+i*radius*.20f+radius*.04f,y-radius*.08f,radius*.075f,ink);}
    Line(x,y+radius*.10f,x,y+radius*.47f,ink,3);
   }
   if(Kind=="collection"){
    if(Theme.useCustomArtwork&&Theme.hero!=null){TextureQuad(context,new Rect(0,0,w,h),Theme.hero);return;}
    Box(0,0,w,h,new Color32(238,242,255,255));
    Circle(w*.28f,h*.45f,w*.22f,new Color32(255,222,199,255));Circle(w*.73f,h*.62f,w*.21f,new Color32(210,231,255,255));
    float floatY=Mathf.Sin(time*2)*h*.015f;
    Bread(w*.08f,h*.30f+floatY,w*.41f,h*.36f);Car(w*.72f,h*.66f-floatY,w*.30f);Bug(w*.72f,h*.25f+floatY,Mathf.Min(w,h)*.14f);
    Spark(w*.14f,h*.18f,8,coral);Spark(w*.49f,h*.78f,10,Theme.park);Circle(w*.47f,h*.16f,5,Theme.mosquito);
   }else if(Kind=="slice"){
    Box(0,0,w,h,new Color32(255,249,236,255));
    for(int i=0;i<5;i++)Line(w*.1f+i*w*.18f,h*.83f,w*.16f+i*w*.18f,h*.83f,Theme.slice,3);
    Bread(w*.15f,h*.32f,w*.7f,h*.36f);
    if(Dragging){Line(StrokeStart.x,StrokeStart.y,StrokeEnd.x,StrokeEnd.y,coral,5);Circle(StrokeEnd.x,StrokeEnd.y,9,Color.white);Circle(StrokeEnd.x,StrokeEnd.y,5,coral);}
   }else if(Kind=="park"){
    Box(0,0,w,h,new Color32(236,245,255,255));float left=w*.1f,lane=w*.8f,target=left+lane*(Config?.target??6500)/10000f;
    float LaneY(float normalized)=>h*(.54f+.045f*Mathf.Sin(normalized*Mathf.PI));
    float pos=Preview?4750+Mathf.Sin(time)*1750:Mathf.Min(Position,11500);float ty=LaneY((Config?.target??6500)/10000f);
    for(int i=0;i<24;i++){float a=i/24f,b=(i+1)/24f;Line(left+lane*a,LaneY(a),left+lane*b,LaneY(b),Color.white,h*.24f);}
    for(int i=0;i<8;i++){float a=i/8f;Line(left+lane*a,LaneY(a)-h*.08f,left+lane*(a+.05f),LaneY(a+.05f)-h*.08f,Theme.park,3);}
    Round(target-31,ty-h*.09f,62,h*.20f,12,new Color32(196,229,222,255));Line(target,ty-h*.15f,target,ty+h*.15f,Theme.accent,3);
    Car(left+lane*pos/10000f,LaneY(pos/10000f)-12,Preview?Mathf.Min(w*.60f,h*.75f):Mathf.Min(w*.22f,100));
    Spark(w*.16f,h*.18f,8,Theme.park);Circle(w*.86f,h*.22f,6,Theme.accent);
   }else{
    Box(0,0,w,h,new Color32(245,240,255,255));for(int i=0;i<6;i++)Circle(w*(.12f+i*.15f),h*(i%2==0?.16f:.84f),4,Theme.mosquito);
    if(Preview)Bug(w*.5f+Mathf.Sin(time*1.5f)*w*.10f,h*.48f+Mathf.Cos(time*2)*h*.06f,Mathf.Min(w,h)*.21f);
    else if(Config?.windows!=null)for(int i=0;i<Config.windows.Length;i++){var window=Config.windows[i];if(Elapsed>=window.start&&Elapsed<=window.end&&!Caught.Contains(i))Bug(w*window.x/10000f,h*window.y/10000f,Mathf.Min(w,h)*window.radius/10000f*.90f);}
   }
   if(age>=0&&age<.55f){
    float t=age/.55f;Color c=Hit?Theme.park:coral;c.a=1-t;
    if(!ReducedMotion)for(int i=0;i<8;i++){float a=i*Mathf.PI/4;float d=12+60*t;Circle(ImpactPoint.x+Mathf.Cos(a)*d,ImpactPoint.y+Mathf.Sin(a)*d,3.5f*(1-t),c);}
    p.strokeColor=c;p.lineWidth=3;p.BeginPath();p.Arc(ImpactPoint,12+36*t,new Angle(0),new Angle(360),ArcDirection.Clockwise);p.Stroke();
   }
  }
 }
}
