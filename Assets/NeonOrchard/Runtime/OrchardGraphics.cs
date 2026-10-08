using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NeonOrchard
{
    public static class Palette
    {
        public static Color Ink = Hex("0A2229"), Panel = Hex("13333A"), Card = Hex("1C4045"), Mint = Hex("C7F36B"), White = Hex("F5F5E9"), Muted = Hex("97B6B2"), Coral = Hex("FF8D77"), Gold = Hex("FFCE76");
        public static readonly Color[] Skins = { Mint, Hex("71DFCB"), Hex("EBA8D6"), Gold };
        public static Color Hex(string s) { ColorUtility.TryParseHtmlString("#" + s, out var c); return c; }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public class SoftShape : UnityEngine.UI.Graphic
    {
        public float radius = 24;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); Round(vh, rectTransform.rect, Mathf.Min(radius, Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2), color);
        }
        public static void Round(VertexHelper vh, Rect r, float rad, Color c)
        {
            int start = vh.currentVertCount;
            vh.AddVert(r.center, c, Vector2.zero);
            const int n = 6;
            for (int corner = 0; corner < 4; corner++)
            {
                Vector2 center = new Vector2(corner < 2 ? r.xMax - rad : r.xMin + rad, corner == 0 || corner == 3 ? r.yMax - rad : r.yMin + rad);
                float angle = 90 - corner * 90;
                for (int k = 0; k <= n; k++)
                {
                    float a = (angle - k * 90f / n) * Mathf.Deg2Rad;
                    vh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rad, c, Vector2.zero);
                }
            }
            int count = 4 * (n + 1);
            for (int i = 0; i < count; i++) vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % count);
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public class OrchardBoard : UnityEngine.UI.Graphic
    {
        public SnakeModel model;
        public bool illustration;
        public Color snakeColor = Palette.Mint;
        public float pulse, hit;
        Vector2 Offset;
        float cell;
        static readonly Cell[] demo = { new Cell(12,15), new Cell(11,15), new Cell(10,15), new Cell(9,15), new Cell(8,15), new Cell(7,15), new Cell(6,15), new Cell(6,14), new Cell(6,13), new Cell(6,12), new Cell(6,11), new Cell(7,11), new Cell(8,11), new Cell(9,11), new Cell(10,11), new Cell(10,10), new Cell(10,9), new Cell(10,8), new Cell(9,8), new Cell(8,8), new Cell(7,8), new Cell(6,8), new Cell(5,8) };
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); Rect r = rectTransform.rect;
            SoftShape.Round(vh, r, 30, Palette.Panel);
            cell = Mathf.Min((r.width - 28) / SnakeModel.Width, (r.height - 28) / SnakeModel.Height);
            Offset = r.center - new Vector2(SnakeModel.Width, SnakeModel.Height) * cell / 2;
            if(illustration) { cell=(r.height-115)/9;Offset=r.center-new Vector2(9,11.5f)*cell; }
            for (int y = 0; y < SnakeModel.Height; y++) for (int x = 0; x < SnakeModel.Width; x++)
            {
                Vector2 p = At(new Cell(x,y));
                if(p.x<r.xMin+22||p.x>r.xMax-22||p.y<r.yMin+22||p.y>r.yMax-22)continue;
                SoftShape.Round(vh, new Rect(p.x-1.1f,p.y-1.1f,2.2f,2.2f),1.1f,new Color(.5f,.7f,.65f,.13f));
            }
            if (!illustration && model != null)
            {
                foreach (var rock in model.Rocks)
                {
                    Tile(vh, rock, Palette.Hex("36585B"), .85f, .20f);
                    Vector2 p = At(rock);
                    SoftShape.Round(vh, new Rect(p.x-cell*.17f,p.y+cell*.12f,cell*.34f,cell*.07f),cell*.03f,Palette.Hex("64807C"));
                }
            }
            var body = illustration || model == null ? (System.Collections.Generic.IList<Cell>)demo : model.Body;
            for (int i = body.Count - 1; i >= 0; i--)
            {
                Color c = Color.Lerp(snakeColor, Palette.Hex("66B878"), .38f * i / body.Count);
                if (hit > 0) c = Color.Lerp(c, Palette.Coral, hit);
                Tile(vh, body[i], c, .91f, .31f);
                if (i > 0)
                {
                    Vector2 a = At(body[i]), b = At(body[i-1]);
                    if ((a-b).sqrMagnitude < cell*cell*1.1f)
                        SoftShape.Round(vh,new Rect(Mathf.Min(a.x,b.x)-cell*.43f,Mathf.Min(a.y,b.y)-cell*.43f,Mathf.Abs(a.x-b.x)+cell*.86f,Mathf.Abs(a.y-b.y)+cell*.86f),cell*.25f,c);
                }
            }
            Cell head = body[0], dir = illustration || model == null ? new Cell(1,0) : model.Direction;
            Vector2 h = At(head), d = new Vector2(dir.x,dir.y), side = new Vector2(-d.y,d.x);
            for (int i = -1; i <= 1; i+=2)
            {
                Vector2 e = h + d*cell*.19f + side*cell*.22f*i;
                SoftShape.Round(vh,new Rect(e.x-cell*.10f,e.y-cell*.10f,cell*.20f,cell*.20f),cell*.1f,Palette.Ink);
                e += d*cell*.02f + Vector2.up*cell*.025f;
                SoftShape.Round(vh,new Rect(e.x-cell*.026f,e.y-cell*.026f,cell*.052f,cell*.052f),cell*.026f,Palette.White);
            }
            Cell food = illustration || model == null ? new Cell(13,11) : model.Food;
            Vector2 f = At(food); float size = cell * (.66f + .04f*Mathf.Sin(pulse*4));
            SoftShape.Round(vh,new Rect(f.x-size*.75f,f.y-size*.75f,size*1.5f,size*1.5f),size*.75f,new Color(1,.55f,.47f,.10f));
            SoftShape.Round(vh,new Rect(f.x-size/2,f.y-size/2,size,size),size*.45f,Palette.Coral);
            SoftShape.Round(vh,new Rect(f.x,f.y+size*.36f,size*.38f,size*.20f),size*.1f,Palette.Mint);
            SoftShape.Round(vh,new Rect(f.x-size*.2f,f.y+size*.04f,size*.12f,size*.17f),size*.06f,Palette.White);
        }
        Vector2 At(Cell c) => Offset + new Vector2(c.x+.5f,c.y+.5f)*cell;
        void Tile(VertexHelper vh, Cell c, Color color, float scale, float radius)
        {
            Vector2 p=At(c); float size=cell*scale;
            SoftShape.Round(vh,new Rect(p.x-size/2,p.y-size/2,size,size),cell*radius,color);
        }
    }

    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        bool pressed;
        public void OnPointerDown(PointerEventData e) { pressed=true; }
        public void OnPointerUp(PointerEventData e) { pressed=false; }
        public void OnPointerExit(PointerEventData e) { pressed=false; }
        void Update() { transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*(pressed?.965f:1),Time.unscaledDeltaTime*22); }
        void OnDisable() { pressed=false; transform.localScale=Vector3.one; }
    }
}
