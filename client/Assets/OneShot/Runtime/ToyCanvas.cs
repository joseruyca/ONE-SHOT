using UnityEngine;
using UnityEngine.UIElements;
namespace OneShot.Client {
 public sealed class ToyCanvas:VisualElement {
  public string Kind;public ArtTheme Theme;public GameConfig Config;public int Elapsed,Position,Cut=-1,FeedbackAt=-1000;public bool Stopped,Hit;
  public ToyCanvas(){generateVisualContent+=Draw;style.backgroundColor=Color.white;style.borderTopLeftRadius=32;style.borderTopRightRadius=32;style.borderBottomLeftRadius=32;style.borderBottomRightRadius=32;style.marginTop=12;style.marginBottom=12;}
  static void TextureQuad(MeshGenerationContext context,Rect r,Texture2D texture){
   var mesh=context.Allocate(4,6,texture);var uv=mesh.uvRegion;
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMin,r.yMin,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMin,uv.yMax)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMax,r.yMin,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMax,uv.yMax)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMax,r.yMax,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMax,uv.yMin)});
   mesh.SetNextVertex(new Vertex{position=new Vector3(r.xMin,r.yMax,Vertex.nearZ),tint=Color.white,uv=new Vector2(uv.xMin,uv.yMin)});
   foreach(ushort index in new ushort[]{0,1,2,2,3,0})mesh.SetNextIndex(index);
  }
  void Draw(MeshGenerationContext context){
   var p=context.painter2D;float w=contentRect.width,h=contentRect.height;if(w<1||h<1||Theme==null)return;
   void Box(float x,float y,float ww,float hh,Color c){p.fillColor=c;p.BeginPath();p.MoveTo(new Vector2(x,y));p.LineTo(new Vector2(x+ww,y));p.LineTo(new Vector2(x+ww,y+hh));p.LineTo(new Vector2(x,y+hh));p.ClosePath();p.Fill();}
   void Circle(float x,float y,float radius,Color c){p.fillColor=c;p.BeginPath();p.Arc(new Vector2(x,y),radius,new Angle(0),new Angle(360),ArcDirection.Clockwise);p.ClosePath();p.Fill();}
   void Line(float x,float y,float xx,float yy,Color c,float thickness){p.strokeColor=c;p.lineWidth=thickness;p.BeginPath();p.MoveTo(new Vector2(x,y));p.LineTo(new Vector2(xx,yy));p.Stroke();}
   void Rounded(float x,float y,float ww,float hh,float radius,Color c){float r=Mathf.Min(radius,Mathf.Min(ww,hh)/2);Box(x+r,y,ww-2*r,hh,c);Box(x,y+r,ww,hh-2*r,c);Circle(x+r,y+r,r,c);Circle(x+ww-r,y+r,r,c);Circle(x+r,y+hh-r,r,c);Circle(x+ww-r,y+hh-r,r,c);}
   void Car(float x,float y,float size){
    Rounded(x-size*.5f,y-size*.29f,size,size*.64f,size*.12f,new Color(0,0,0,.08f));
    Rounded(x-size*.34f,y-size*.36f,size*.68f,size*.12f,4,Theme.ink);Rounded(x-size*.34f,y+size*.25f,size*.68f,size*.12f,4,Theme.ink);
    Rounded(x-size*.5f,y-size*.28f,size,size*.56f,size*.13f,Theme.ink);Rounded(x-size*.20f,y-size*.22f,size*.51f,size*.44f,size*.08f,Theme.park);
    Rounded(x+size*.33f,y-size*.18f,size*.09f,size*.11f,2,Theme.accent);Rounded(x+size*.33f,y+size*.07f,size*.09f,size*.11f,2,Theme.accent);
    if(Theme.car!=null)TextureQuad(context,new Rect(x-size*.5f,y-size*.3f,size,size*.6f),Theme.car);
   }
   void Bread(float x,float y,float ww,float hh){Rounded(x,y+8,ww,hh,24,new Color(0,0,0,.07f));Rounded(x,y,ww,hh,24,Theme.slice);Rounded(x+10,y+10,ww-20,hh-20,18,new Color32(255,226,171,255));for(int i=0;i<9;i++)Circle(x+ww*(.14f+i*.09f),y+hh*(i%2==0?.40f:.60f),ww*.013f,Theme.slice);if(Theme.bread!=null)TextureQuad(context,new Rect(x,y,ww,hh),Theme.bread);}
   void Bug(float x,float y,float radius){Circle(x-radius*.7f,y-radius*.3f,radius*.75f,Theme.mosquito);Circle(x+radius*.7f,y-radius*.3f,radius*.75f,Theme.mosquito);Line(x,y,x,y+radius,Theme.ink,5);Circle(x,y-radius*.1f,radius*.23f,Theme.ink);Line(x,y+radius*.45f,x-radius*.6f,y+radius,Theme.ink,3);Line(x,y+radius*.45f,x+radius*.6f,y+radius,Theme.ink,3);if(Theme.mosquitoArtwork!=null)TextureQuad(context,new Rect(x-radius,y-radius,radius*2,radius*2),Theme.mosquitoArtwork);}
   if(Kind=="collection"){
    Circle(w*.25f,h*.34f,w*.18f,new Color32(254,229,211,255));Bread(w*.08f,h*.23f,w*.36f,h*.22f);
    Circle(w*.70f,h*.35f,w*.19f,new Color32(223,236,230,255));Car(w*.70f,h*.35f,w*.27f);
    Circle(w*.48f,h*.74f,w*.15f,new Color32(231,224,250,255));Bug(w*.48f,h*.74f,w*.07f);return;
   }
   if(Kind=="slice"){
    Bread(w*.15f,h*.32f,w*.7f,h*.36f);
    if(Cut>=0){float x=w*(.15f+.7f*Cut/10000f);Line(x,h*.30f,x,h*.70f,Color.white,9);Line(x+3,h*.30f,x+3,h*.70f,Theme.ink,2);}
   }else if(Kind=="park"){
    float lane=w*.8f,left=w*.1f,target=left+lane*Config.target/10000f;
    Rounded(left-12,h*.30f,lane+24,h*.40f,20,new Color32(217,230,223,255));
    for(int i=0;i<5;i++)Line(left+lane*i/5,h*.33f,left+lane*i/5+lane*.05f,h*.33f,Color.white,3);
    Rounded(target-26,h*.39f,52,h*.22f,8,new Color(1,1,1,.65f));Line(target,h*.38f,target,h*.62f,Theme.ink,3);
    Car(left+lane*Mathf.Min(Position,11000)/10000f,h*.5f,64);
   }else if(Config?.windows!=null){
    Rounded(w*.08f,h*.12f,w*.84f,h*.76f,24,new Color(1,1,1,.38f));
    foreach(var window in Config.windows)if(Elapsed>=window.start&&Elapsed<=window.end)Bug(w*window.x/10000f,h*window.y/10000f,Mathf.Min(w,h)*.08f);
   }
   float feedback=1-Mathf.Clamp01((Elapsed-FeedbackAt)/500f);
   if(feedback>0){p.strokeColor=Hit?Theme.ink:Theme.slice;p.lineWidth=2+feedback*3;p.BeginPath();p.Arc(new Vector2(w*.5f,h*.5f),Mathf.Min(w,h)*(.18f+(1-feedback)*.08f),new Angle(0),new Angle(360),ArcDirection.Clockwise);p.Stroke();}
  }
 }
}
