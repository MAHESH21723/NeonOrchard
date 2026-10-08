using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace NeonOrchard
{
    public sealed class OrchardApp : MonoBehaviour
    {
        public TMP_FontAsset font;
        public enum View { Home, Journey, Settings, Looks, Ready, Playing, Paused, Result, Privacy }
        public View Current { get; private set; }
        public SnakeModel Model { get; private set; }
        public SaveData Save { get; private set; }
        public RectTransform Root { get; private set; }
        RectTransform safe, screen, modal;
        OrchardBoard board;
        OrchardAudio sound;
        TMP_Text scoreText, progressText, hint, bestText;
        RectTransform progressFill;
        float accumulator, runTime, paintTime;
        int level, page;
        bool endless, runCounted, touching;
        Vector2 touchStart;
        const string SaveKey="neon-orchard.v1";

        void Awake()
        {
            Application.targetFrameRate=60; Screen.sleepTimeout=SleepTimeout.SystemSetting;
            try { Save=JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey,""))??new SaveData(); } catch { Save=new SaveData(); }
            Save.Sanitize();
            if(!font) font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            sound=gameObject.AddComponent<OrchardAudio>();
            sound.Configure(Save.music,Save.sound);
            if(!FindFirstObjectByType<EventSystem>()) new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            var canvasObject=new GameObject("Neon Orchard UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=.5f;
            var bg=canvasObject.AddComponent<SoftShape>();bg.color=Palette.Ink;bg.radius=0;bg.raycastTarget=false;
            safe=Rect("Safe Area",canvasObject.transform,0,0,720,1280);
            Root=Rect("Design Frame",safe,0,0,720,1280); Root.anchorMin=Root.anchorMax=Root.pivot=new Vector2(.5f,.5f);Root.anchoredPosition=Vector2.zero;
            Home();
        }
        void Update()
        {
            Rect area=Screen.safeArea;
            area=UnityEngine.Rect.MinMaxRect(Mathf.Clamp(area.xMin,0,Screen.width),Mathf.Clamp(area.yMin,0,Screen.height),Mathf.Clamp(area.xMax,0,Screen.width),Mathf.Clamp(area.yMax,0,Screen.height));
            if(area.width<1||area.height<1)area=new Rect(0,0,Screen.width,Screen.height);
            safe.anchorMin=new Vector2(area.x/Screen.width,area.y/Screen.height);safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;
            float scale=Mathf.Min(safe.rect.width/720,safe.rect.height/1280);Root.localScale=Vector3.one*scale;
            ReadInput();
            if(Current==View.Playing)
            {
                runTime+=Time.unscaledDeltaTime;accumulator+=Mathf.Min(Time.unscaledDeltaTime,.1f);
                if(accumulator>=Model.Interval) { accumulator-=Model.Interval; Tick(); }
            }
            if(board && Time.unscaledTime>paintTime) { paintTime=Time.unscaledTime+.04f;board.pulse=Time.unscaledTime;board.SetVerticesDirty(); }
        }
        void ReadInput()
        {
            var k=Keyboard.current;
            if(k!=null)
            {
                if(k.escapeKey.wasPressedThisFrame) Back();
                if(k.upArrowKey.wasPressedThisFrame||k.wKey.wasPressedThisFrame) Direction(0,1);
                if(k.downArrowKey.wasPressedThisFrame||k.sKey.wasPressedThisFrame) Direction(0,-1);
                if(k.leftArrowKey.wasPressedThisFrame||k.aKey.wasPressedThisFrame) Direction(-1,0);
                if(k.rightArrowKey.wasPressedThisFrame||k.dKey.wasPressedThisFrame) Direction(1,0);
                if(k.spaceKey.wasPressedThisFrame && Current==View.Playing) Pause();
            }
            if(Current!=View.Playing && Current!=View.Ready) { touching=false;return; }
            var t=Touchscreen.current;
            if(t!=null && t.primaryTouch.press.isPressed)
            {
                Vector2 p=t.primaryTouch.position.ReadValue();
                if(!touching) { touching=true;touchStart=p; }
                else Swipe(p);
            }
            else if(Mouse.current!=null && Mouse.current.leftButton.isPressed)
            {
                Vector2 p=Mouse.current.position.ReadValue();
                if(!touching) { touching=true;touchStart=p; } else Swipe(p);
            }
            else touching=false;
        }
        void Swipe(Vector2 p)
        {
            Vector2 d=p-touchStart;float threshold=Mathf.Max(20,Screen.width*.035f);
            if(d.sqrMagnitude<threshold*threshold)return;
            if(Mathf.Abs(d.x)>Mathf.Abs(d.y))Direction(d.x>0?1:-1,0);else Direction(0,d.y>0?1:-1);
            touchStart=p;
        }
        public void Direction(int x,int y)
        {
            if(Current!=View.Playing && Current!=View.Ready)return;
            Model.Turn(new Cell(x,y));
            if(Current==View.Ready) { Current=View.Playing;hint.text="COLLECT FRUIT. FIND YOUR FLOW.";accumulator=0; }
        }
        public void StartRun(int index,bool infinite)
        {
            if(Model!=null&&(Current==View.Playing||Current==View.Paused||Current==View.Ready))CompleteRun();
            level=Mathf.Clamp(index,0,Stage.Count-1);endless=infinite;Model=new SnakeModel(Environment.TickCount);Model.Reset(level,endless);
            accumulator=0;runTime=0;runCounted=false;BuildGame();Current=View.Ready;
        }
        void Tick()
        {
            var result=Model.Step();
            if(result==StepResult.Ate||result==StepResult.Complete) { Save.fruit++;sound.Eat();if(Save.haptics) Handheld.Vibrate(); }
            UpdateHud();
            if(result==StepResult.Crashed) { sound.Crash();Finish(false); }
            else if(result==StepResult.Complete) { sound.Win();Finish(true); }
        }
        void UpdateHud()
        {
            if(scoreText)scoreText.text=Model.Score.ToString("0000");
            if(progressText)progressText.text=endless ? Model.Eaten+" FRUIT" : Model.Eaten+" / "+Model.Stage.Target+" FRUIT";
            if(progressFill)progressFill.sizeDelta=new Vector2(endless?620:620*Mathf.Clamp01((float)Model.Eaten/Model.Stage.Target),6);
            if(bestText)bestText.text="BEST "+Mathf.Max(Save.best,Model.Score).ToString("0000");
        }
        void CompleteRun()
        {
            if(runCounted)return;runCounted=true;Save.runs++;Save.best=Mathf.Max(Save.best,Model.Score);Persist();
        }
        void Finish(bool won)
        {
            if(won&&!endless)
            {
                int stars=runTime<Model.Stage.Target*7?3:runTime<Model.Stage.Target*12?2:1;
                Save.stars[level]=Mathf.Max(Save.stars[level],stars);Save.unlocked=Mathf.Max(Save.unlocked,Mathf.Min(Stage.Count-1,level+1));
            }
            CompleteRun();Current=View.Result;
            var p=Modal(won?"BEAUTIFULLY GROWN":"ONE MORE BITE?",won?"Harvest complete":"A fresh start awaits",won?Palette.Mint:Palette.Coral);
            Label(p,won&&!endless?new string('*',Save.stars[level]):"FRUIT COLLECTED",40,155,550,54,36,Palette.Gold,TextAlignmentOptions.Center);
            Label(p,Model.Score.ToString("0000"),40,222,550,94,78,Palette.White,TextAlignmentOptions.Center);
            Label(p,won? (level==23?"You mastered the orchard. Keep growing in Endless.":"A new path is ready to explore."):"Small turns. Big possibilities. Try again.",40,326,550,65,23,Palette.Muted,TextAlignmentOptions.Center);
            Button(p,won&&!endless&&level<23?"NEXT LEVEL  >":"PLAY AGAIN",40,430,550,92,()=>StartRun(won&&!endless&&level<23?level+1:level,endless),Palette.Mint,Palette.Ink);
            Button(p,"BACK TO ORCHARD",40,540,550,80,Home,Palette.Card,Palette.White);
        }
        void BuildGame()
        {
            Clear(View.Ready);
            Label(screen,endless?"ENDLESS / PERSONAL BEST":("JOURNEY / "+Model.Stage.World),44,38,540,32,19,Palette.Muted);
            Label(screen,endless?"Keep growing":"Level "+(level+1).ToString("00"),42,77,470,58,44,Palette.White);
            Button(screen,"II",584,40,92,92,Pause,Palette.Card,Palette.White);
            scoreText=Label(screen,"0000",44,147,340,70,56,Palette.White);
            bestText=Label(screen,"BEST "+Save.best.ToString("0000"),386,167,288,34,21,Palette.Muted,TextAlignmentOptions.Right);
            board=Rect("Orchard Board",screen,42,236,636,774).gameObject.AddComponent<OrchardBoard>();board.raycastTarget=false;board.model=Model;board.snakeColor=Palette.Skins[Save.skin];
            progressText=Label(screen,"",48,1028,370,32,21,Palette.White);
            Label(screen,endless?"NO LIMITS":Model.Stage.Name.ToUpperInvariant(),385,1028,289,32,17,Palette.Muted,TextAlignmentOptions.Right);
            Panel(screen,50,1077,620,6,Palette.Card,3);
            progressFill=Panel(screen,50,1077,0,6,Palette.Mint,3).rectTransform;
            hint=Label(screen,"SWIPE OR TAP AN ARROW TO START",35,1102,650,33,19,Palette.Muted,TextAlignmentOptions.Center);
            if(Save.buttons)
            {
                string[] labels={"<","^","v",">"};int[] dx={-1,0,0,1},dy={0,1,-1,0};
                for(int i=0;i<4;i++) { int j=i;Button(screen,labels[i],48+i*160,1154,144,96,()=>Direction(dx[j],dy[j]),Palette.Card,Palette.White,37); }
            }
            else Label(screen,"Swipe anywhere to turn  /  Avoid walls and yourself",40,1175,640,65,21,Palette.Muted,TextAlignmentOptions.Center);
            UpdateHud();
        }
        public void Pause()
        {
            if(Current!=View.Playing&&Current!=View.Ready)return;
            Current=View.Paused;Persist();
            var p=Modal("TAKE A BREATHER","The orchard can wait",Palette.Mint);
            Label(p,"Your run is paused.\nCome back when you are ready.",42,177,546,100,29,Palette.Muted,TextAlignmentOptions.Center);
            Button(p,"KEEP GROWING",40,327,550,96,Resume,Palette.Mint,Palette.Ink);
            Button(p,"RESTART LEVEL",40,446,550,86,()=>StartRun(level,endless),Palette.Card,Palette.White);
            Button(p,"BACK TO ORCHARD",40,552,550,80,()=>{CompleteRun();Home();},Palette.Panel,Palette.Muted);
        }
        public void Resume()
        {
            if(Current!=View.Paused)return;if(modal)Destroy(modal.gameObject);modal=null;Current=View.Ready;accumulator=0;hint.text="SWIPE OR TAP TO RESUME";
        }
        public void Home()
        {
            Clear(View.Home);
            Label(screen,"N / O",44,43,180,44,29,Palette.Mint);
            Label(screen,"A LITTLE MOMENT OF PLAY",245,49,430,30,17,Palette.Muted,TextAlignmentOptions.Right);
            Label(screen,"neon\norchard.",40,113,640,244,92,Palette.White);
            Label(screen,"A classic, freshly grown.",44,357,600,47,29,Palette.Muted);
            board=Rect("Living Orchard",screen,42,437,636,389).gameObject.AddComponent<OrchardBoard>();board.illustration=true;board.raycastTarget=false;board.snakeColor=Palette.Skins[Save.skin];
            Panel(screen,64,457,148,40,Palette.Mint,20);Label(screen,"SNAKE, REIMAGINED",73,465,133,25,12,Palette.Ink,TextAlignmentOptions.Center);
            Label(screen,"GROW AT YOUR OWN PACE",67,778,576,30,17,Palette.Muted);
            Button(screen,"CONTINUE JOURNEY    >",42,860,636,105,()=>StartRun(Save.unlocked,false),Palette.Mint,Palette.Ink,29);
            Button(screen,"ENDLESS",42,982,308,91,()=>StartRun(0,true),Palette.Card,Palette.White,25);
            Button(screen,"LEVELS  /  "+(Save.unlocked+1).ToString("00"),370,982,308,91,Journey,Palette.Card,Palette.White,25);
            Button(screen,"YOUR COLORS",42,1093,308,90,Looks,Palette.Panel,Palette.Muted,21);
            Button(screen,"SETTINGS",370,1093,308,90,Settings,Palette.Panel,Palette.Muted,21);
            Label(screen,"24 LEVELS  /  OFFLINE  /  ALL YOURS",40,1223,640,26,17,Palette.Muted,TextAlignmentOptions.Center);
        }
        public void Journey()
        {
            page=Save.unlocked/6;DrawJourney();
        }
        void DrawJourney()
        {
            Clear(View.Journey);Header("Your journey","ONE GOOD TURN AT A TIME");
            var stage=Stage.Create(page*6);
            Label(screen,(page+1).ToString("00")+" / "+stage.World,44,220,630,57,39,Palette.Mint);
            Label(screen,"Collect the fruit. Explore a little further.",44,290,630,42,23,Palette.Muted);
            for(int i=0;i<6;i++)
            {
                int index=page*6+i;bool unlocked=index<=Save.unlocked;float x=44+(i%2)*328,y=371+(i/2)*227;
                var b=Button(screen,"",x,y,306,204,()=>StartRun(index,false),unlocked?Palette.Card:Palette.Panel,Palette.White);
                b.interactable=unlocked;
                Label(b.transform,(index+1).ToString("00"),23,19,260,70,54,unlocked?Palette.White:Palette.Muted);
                Label(b.transform,Stage.Create(index).Name,23,102,260,43,23,unlocked?Palette.White:Palette.Muted);
                Label(b.transform,unlocked?(Save.stars[index]>0?new string('*',Save.stars[index])+"  COMPLETE":"READY TO GROW"):"LOCKED",23,162,260,25,15,unlocked?Palette.Mint:Palette.Muted);
            }
            var prev=Button(screen,"<",44,1095,100,92,()=>{page--;DrawJourney();},Palette.Card,Palette.White);prev.interactable=page>0;
            Label(screen,"CHAPTER "+(page+1)+" OF 4",159,1125,402,30,21,Palette.Muted,TextAlignmentOptions.Center);
            var next=Button(screen,">",576,1095,100,92,()=>{page++;DrawJourney();},Palette.Card,Palette.White);next.interactable=page<3;
            Label(screen,"Earn stars by harvesting efficiently.",40,1230,640,25,18,Palette.Muted,TextAlignmentOptions.Center);
        }
        public void Looks()
        {
            Clear(View.Looks);Header("Your colors","A FRESH LOOK, EARNED BY PLAYING");
            Label(screen,Save.fruit+" FRUIT COLLECTED",44,220,630,50,29,Palette.Mint);
            string[] names={"Original lime","Lagoon","Blossom","Golden"};
            for(int i=0;i<4;i++)
            {
                int index=i;bool unlocked=i==0||Save.fruit>=i*40;float y=330+i*185;
                var b=Button(screen,"",44,y,632,160,()=>{Save.skin=index;Persist();Looks();},Palette.Panel,Palette.White);b.interactable=unlocked;
                Panel(b.transform,24,38,80,80,Palette.Skins[i],32);
                Label(b.transform,names[i],130,29,467,53,35,Palette.White);
                Label(b.transform,unlocked?(Save.skin==i?"SELECTED":"TAP TO WEAR"):(i*40)+" TOTAL FRUIT TO UNLOCK",130,99,467,30,19,Save.skin==i?Palette.Mint:Palette.Muted);
            }
            Label(screen,"Every fruit counts. No shop. Just play.",44,1138,632,67,24,Palette.Muted,TextAlignmentOptions.Center);
        }
        public void Settings()
        {
            Clear(View.Settings);Header("Make it yours","SETTLE INTO YOUR OWN RHYTHM");
            Setting("Music","Original mellow orchard loop",285,Save.music,()=>Save.music=!Save.music);
            Setting("Sound effects","Little sounds for little victories",452,Save.sound,()=>Save.sound=!Save.sound);
            Setting("Vibration","A small tap when you collect fruit",619,Save.haptics,()=>Save.haptics=!Save.haptics);
            Setting("Direction buttons","Swipes are always available",786,Save.buttons,()=>Save.buttons=!Save.buttons);
            Button(screen,"HOW TO PLAY",44,981,632,86,Help,Palette.Card,Palette.White,23);
            Button(screen,"PRIVACY & CREDITS",44,1088,632,86,Privacy,Palette.Panel,Palette.Muted,23);
            Label(screen,"NEON ORCHARD  /  VERSION 1.0",44,1227,632,26,17,Palette.Muted,TextAlignmentOptions.Center);
        }
        void Setting(string title,string description,float y,bool enabled,Action toggle)
        {
            Panel(screen,44,y,632,141,Palette.Panel,24);Label(screen,title,68,y+23,430,45,33,Palette.White);Label(screen,description,68,y+82,430,32,19,Palette.Muted);
            Button(screen,enabled?"ON":"OFF",526,y+30,126,80,()=>{toggle();Persist();Settings();},enabled?Palette.Mint:Palette.Card,enabled?Palette.Ink:Palette.Muted,23);
        }
        void Help()
        {
            var p=Modal("A FEW SIMPLE THINGS","Small turns. Big growth.",Palette.Mint);
            Label(p,"01   Swipe or tap an arrow to turn.\n\n02   Collect coral fruit to grow.\n\n03   Avoid the edges, rocks and your tail.\n\n04   Harvest the target to clear a level.",45,188,540,298,27,Palette.White);
            Button(p,"GOT IT",40,533,550,95,()=>{Destroy(modal.gameObject);modal=null;},Palette.Mint,Palette.Ink);
        }
        void Privacy()
        {
            Clear(View.Privacy);Header("Simply yours","PRIVACY & CREDITS");
            Label(screen,"Made for a quiet moment.",44,252,632,82,42,Palette.Mint);
            Label(screen,"Neon Orchard works offline. It has no ads, purchases, accounts or social features.\n\nYour scores, preferences and unlocked levels stay on this device. The game does not send gameplay data to a server. Clearing app data removes your progress.\n\nVisuals and music were created for this game. Built with Unity and TextMeshPro.\n\nSwipe controls, direction buttons, arrow keys and WASD are supported.",44,379,632,670,29,Palette.White);
            Button(screen,"BACK TO SETTINGS",44,1128,632,96,Settings,Palette.Card,Palette.White,24);
        }
        void Header(string title,string eyebrow)
        {
            Button(screen,"<",44,35,86,86,Home,Palette.Card,Palette.White,31);
            Label(screen,eyebrow,154,59,530,36,17,Palette.Muted);
            Label(screen,title,42,142,635,73,55,Palette.White);
        }
        void Back()
        {
            if(Current==View.Playing||Current==View.Ready)Pause();
            else if(Current==View.Paused)Resume();
            else if(modal) { Destroy(modal.gameObject);modal=null; }
            else if(Current!=View.Home) Home();
            else
            {
                var p=Modal("SEE YOU SOON","Leave the orchard?",Palette.Mint);
                Button(p,"KEEP PLAYING",40,286,550,99,()=>{Destroy(modal.gameObject);modal=null;},Palette.Mint,Palette.Ink);
                Button(p,"EXIT GAME",40,421,550,92,()=>{Persist();Application.Quit();},Palette.Card,Palette.White);
            }
        }
        RectTransform Modal(string eyebrow,string title,Color accent)
        {
            if(modal) { modal.gameObject.SetActive(false);Destroy(modal.gameObject); }
            modal=Rect("Overlay",Root,0,0,720,1280);var shade=modal.gameObject.AddComponent<SoftShape>();shade.color=new Color(.02f,.08f,.10f,.92f);shade.radius=0;shade.raycastTarget=true;
            var p=Panel(modal,45,298,630,683,Palette.Panel,35).rectTransform;
            Label(p,eyebrow,35,39,560,40,20,accent,TextAlignmentOptions.Center);
            Label(p,title,35,90,560,65,36,Palette.White,TextAlignmentOptions.Center);
            return p;
        }
        void Clear(View view)
        {
            if(screen) { screen.gameObject.SetActive(false);Destroy(screen.gameObject); }
            if(modal) { modal.gameObject.SetActive(false);Destroy(modal.gameObject); }
            board=null;modal=null;Current=view;screen=Rect(view.ToString(),Root,0,0,720,1280);
            scoreText=progressText=hint=bestText=null;progressFill=null;touching=false;
        }
        public void Persist() { PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(Save));PlayerPrefs.Save();if(sound)sound.Configure(Save.music,Save.sound); }
        void OnApplicationPause(bool paused) { if(paused) { Pause();Persist();AudioListener.pause=true; } else AudioListener.pause=false; }
        void OnApplicationFocus(bool focused) { if(!focused) { Pause();Persist(); } }
        void OnApplicationQuit() { Persist(); }
        static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        static SoftShape Panel(Transform parent,float x,float y,float w,float h,Color color,float radius=24)
        {
            var p=Rect("Panel",parent,x,y,w,h).gameObject.AddComponent<SoftShape>();p.color=color;p.radius=radius;p.raycastTarget=false;return p;
        }
        TMP_Text Label(Transform parent,string text,float x,float y,float w,float h,float size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var t=Rect("Text - "+text.Replace('\n',' '),parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;t.fontStyle=FontStyles.Normal;return t;
        }
        UnityEngine.UI.Button Button(Transform parent,string text,float x,float y,float w,float h,Action action,Color color,Color textColor,float size=27)
        {
            var p=Panel(parent,x,y,w,h,color,23);p.name="Button - "+text;p.raycastTarget=true;
            var b=p.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=p;var colors=b.colors;colors.highlightedColor=new Color(.93f,.97f,.91f);colors.pressedColor=new Color(.76f,.84f,.78f);colors.disabledColor=new Color(.62f,.68f,.65f,.65f);b.colors=colors;
            b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(()=>{sound.Tap();action();});p.gameObject.AddComponent<PressFeedback>();
            Label(p.transform,text,14,8,w-28,h-16,size,textColor,TextAlignmentOptions.Center);return b;
        }
    }
}
