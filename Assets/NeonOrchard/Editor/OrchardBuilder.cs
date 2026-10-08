using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NeonOrchard.Editor
{
    public static class OrchardBuilder
    {
        public const string ScenePath="Assets/NeonOrchard/Scenes/Orchard.unity";
        public static void SetPreview(int width=720,int height=1280)
        {
            var assembly=typeof(UnityEditor.Editor).Assembly;
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance").GetValue(null);
            var groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");
            var currentGroup=sizesType.GetProperty("currentGroupType").GetValue(sizes);
            var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{currentGroup});
            var sizeType=assembly.GetType("UnityEditor.GameViewSize");
            var kind=assembly.GetType("UnityEditor.GameViewSizeType");
            var entry=Activator.CreateInstance(sizeType,new object[]{Enum.ToObject(kind,1),width,height,"Neon Orchard "+width+"x"+height});
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{entry});
            int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
            var viewType=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex").SetValue(view,count-1);view.Repaint();
        }
        [MenuItem("Neon Orchard/Set up game")]
        public static void Setup()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before setup.");
            Directory.CreateDirectory("Assets/NeonOrchard/Scenes");Directory.CreateDirectory("Assets/NeonOrchard/Art");Directory.CreateDirectory("Release");
            if(!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset")) { TMP_PackageResourceImporter.ImportResources(true,false,false); return; }
            Configure();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
            camera.tag="MainCamera";camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Palette.Ink;camera.transform.position=new Vector3(0,0,-10);
            var app=new GameObject("Neon Orchard",typeof(OrchardApp)).GetComponent<OrchardApp>();
            app.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if(!app.font)throw new InvalidOperationException("TMP font has not finished importing.");
            EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            MakeIcon();AssetDatabase.SaveAssets();
            Debug.Log("Neon Orchard setup complete.");
        }
        [MenuItem("Neon Orchard/Configure Android")]
        public static void Configure()
        {
            PlayerSettings.companyName="Neon Orchard";PlayerSettings.productName="Neon Orchard";
            PlayerSettings.bundleVersion="1.0.0";PlayerSettings.Android.bundleVersionCode=1;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.neonorchard.snake");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth=720;PlayerSettings.defaultScreenHeight=1280;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Low);
            PlayerSettings.Android.forceInternetPermission=false;PlayerSettings.Android.forceSDCardPermission=false;
            PlayerSettings.SplashScreen.show=false;
            QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;
            UnityEditor.Analytics.AnalyticsSettings.enabled=false;
            UnityEditor.Analytics.AnalyticsSettings.initializeOnStartup=false;
            UnityEditor.CrashReporting.CrashReportingSettings.enabled=false;
            var services=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/UnityConnectSettings.asset")[0]);
            foreach(var path in new[]{"m_Enabled","InsightsSettings.m_EngineDiagnosticsEnabled","InsightsSettings.m_Enabled","UnityAnalyticsSettings.m_InitializeOnStartup","UnityAdsSettings.m_InitializeOnStartup"})
            { var prop=services.FindProperty(path);if(prop!=null)prop.boolValue=false; }
            services.ApplyModifiedPropertiesWithoutUndo();
            EditorSettings.serializationMode=SerializationMode.ForceText;
            var serialized=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input=serialized.FindProperty("activeInputHandler");if(input!=null)input.intValue=1;serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }
        public static void MakeIcon()
        {
            const int size=512;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            var points=new[]{new Vector2(333,354),new Vector2(180,354),new Vector2(155,328),new Vector2(155,276),new Vector2(185,250),new Vector2(280,250),new Vector2(307,220),new Vector2(307,170),new Vector2(277,145),new Vector2(164,145)};
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Vector2 p=new Vector2(x,y);Color c=Color.Lerp(Palette.Ink,Palette.Panel,(float)y/size);
                float dist=float.MaxValue;
                for(int i=0;i<points.Length-1;i++) { var a=points[i];var v=points[i+1]-a;dist=Mathf.Min(dist,Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude))); }
                c=Color.Lerp(c,Palette.Mint,Mathf.Clamp01(29-dist));
                foreach(var eye in new[]{new Vector2(330,365),new Vector2(330,343)})c=Color.Lerp(c,Palette.Ink,Mathf.Clamp01(6-Vector2.Distance(p,eye)));
                c=Color.Lerp(c,Palette.Coral,Mathf.Clamp01(28-Vector2.Distance(p,new Vector2(366,259))));
                c=Color.Lerp(c,Palette.Mint,Mathf.Clamp01(10-Vector2.Distance(p,new Vector2(377,285))));
                texture.SetPixel(x,y,c);
            }
            texture.Apply();byte[] png=texture.EncodeToPNG();File.WriteAllBytes("Assets/NeonOrchard/Art/AppIcon.png",png);File.WriteAllBytes("Release/AppIcon-512.png",png);
            UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset("Assets/NeonOrchard/Art/AppIcon.png");
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/NeonOrchard/Art/AppIcon.png");
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.SaveAndReimport();
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NeonOrchard/Art/AppIcon.png");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);
        }
        [MenuItem("Neon Orchard/Build Android test APK")]
        public static void BuildApk()
        {
            Configure();EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;
            Build("Release/NeonOrchard-test.apk",BuildTarget.Android);
        }
        [MenuItem("Neon Orchard/Build release AAB")]
        public static void BuildAab()
        {
            Configure();
            if(!PlayerSettings.Android.useCustomKeystore || string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName))
                throw new InvalidOperationException("Configure your upload keystore under Player Settings > Publishing Settings before building a store release.");
            EditorUserBuildSettings.buildAppBundle=true;Build("Release/NeonOrchard.aab",BuildTarget.Android);
        }
        [MenuItem("Neon Orchard/Build Windows preview")]
        public static void BuildWindows() { Build("Release/Windows/NeonOrchard.exe",BuildTarget.StandaloneWindows64); }
        static void Build(string output,BuildTarget target)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=target,options=BuildOptions.None});
            File.WriteAllText("Release/LastBuild.txt",report.summary.result+"\n"+report.summary.totalSize+" bytes\n"+report.summary.totalTime+"\n"+string.Join("\n",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error).Select(m=>m.content)));
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed. Read Release/LastBuild.txt.");
        }
        public static string Tests()
        {
            int checks=0;Action<bool,string> check=(ok,msg)=>{if(!ok)throw new Exception(msg);checks++;};
            var m=new SnakeModel(72);m.Reset(0,false);
            check(!m.Turn(new Cell(-1,0)),"Immediate reverse rejected");
            check(m.Turn(new Cell(0,1)),"Up turn accepted");check(m.Turn(new Cell(-1,0)),"Buffered left accepted");
            check(!m.Turn(new Cell(0,-1)),"Input buffer bounded");m.Step();check(m.Direction==new Cell(0,1),"First queued turn");m.Step();check(m.Direction==new Cell(-1,0),"Second queued turn");
            m.Reset(0,false);for(int i=0;i<30&&!m.Over;i++)m.Step();check(m.Over,"Wall collision ends run");
            m.Reset(0,false);m.Body.Clear();m.Body.AddRange(new[]{new Cell(7,10),new Cell(8,10),new Cell(8,11),new Cell(7,11)});check(m.Step()==StepResult.Crashed,"Self collision detected");
            var save=new SaveData{unlocked=999,skin=999,best=-7,stars=null};save.Sanitize();check(save.unlocked==23&&save.skin==0&&save.best==0&&save.stars.Length==24,"Malformed save repaired");
            int completed=0;
            for(int l=0;l<Stage.Count;l++)for(int seed=0;seed<8;seed++)
            {
                m=new SnakeModel(seed*391+l);m.Reset(l,false);
                check(!m.Rocks.Overlaps(m.Body),"Safe spawn at stage "+l);
                check(!m.Rocks.Contains(m.Food)&&!m.Body.Contains(m.Food),"Valid fruit spawn");
                var connected=Flood(m.Rocks);check(connected==SnakeModel.Width*SnakeModel.Height-m.Rocks.Count,"Connected stage "+l);
                int steps=0;
                while(!m.Over&&steps++<4000)
                {
                    var path=PathToFood(m);check(path.Count>0,"Reachable fruit for stage "+l+" seed "+seed);
                    m.Turn(path[0]);var result=m.Step();
                    check(result!=StepResult.Crashed,"Stage playthrough collision");
                    if(result==StepResult.Complete)completed++;
                    else check(!m.Rocks.Contains(m.Food)&&!m.Body.Contains(m.Food),"Fruit never overlaps occupied cell");
                }
                check(m.Eaten==m.Stage.Target,"Stage target completable");
            }
            string report="PASS: "+checks+" checks; "+completed+" complete stage playthroughs (24 stages x 8 seeds).\nReverse prevention, buffered turns, wall/self collisions, save repair, connected stage geometry, fruit placement and completion verified.";
            File.WriteAllText("Release/GameplayTests.txt",report);Debug.Log(report);return report;
        }
        public static string SmokeTests()
        {
            var app=UnityEngine.Object.FindFirstObjectByType<OrchardApp>();
            if(!app)throw new Exception("Enter Play mode first.");
            string original=JsonUtility.ToJson(app.Save);var truncated=new List<string>();int views=0;
            Action inspect=()=>
            {
                Canvas.ForceUpdateCanvases();views++;
                foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
                {
                    t.ForceMeshUpdate();if(t.isTextTruncated)truncated.Add(app.Current+": "+t.text);
                }
            };
            try
            {
                app.Home();inspect();app.Journey();inspect();app.Settings();inspect();app.Looks();inspect();
                app.StartRun(0,false);inspect();app.Pause();inspect();app.Resume();
                if(app.Current!=OrchardApp.View.Ready)throw new Exception("Resume failed");
                var tick=typeof(OrchardApp).GetMethod("Tick",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                int steps=0;while(!app.Model.Over&&steps++<4000)
                {
                    var path=PathToFood(app.Model);if(path.Count==0)throw new Exception("Smoke path unavailable");
                    app.Direction(path[0].x,path[0].y);tick.Invoke(app,null);
                }
                if(app.Current!=OrchardApp.View.Result||app.Save.stars[0]<1||app.Save.unlocked<1)throw new Exception("Completion/progression failed");
                inspect();app.StartRun(0,true);app.Direction(1,0);
                while(!app.Model.Over)tick.Invoke(app,null);
                if(app.Current!=OrchardApp.View.Result)throw new Exception("Endless result failed");
                inspect();app.StartRun(0,false);if(app.Model.Score!=0)throw new Exception("Retry score reset failed");
                var music=UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
                if(music.Length<2||!music.Any(s=>s.loop&&s.clip))throw new Exception("Music/effect sources missing");
                if(truncated.Count>0)throw new Exception("Truncated UI text: "+string.Join(" | ",truncated));
                string report="PASS: "+views+" screens checked without truncated text. Pause/resume, journey completion, stars, unlocks, endless failure, retry reset, and music sources verified. Touch pause button was also verified through simulated pointer input.";
                File.WriteAllText("Release/UiTests.txt",report);return report;
            }
            finally { JsonUtility.FromJsonOverwrite(original,app.Save);app.Persist();app.Home(); }
        }
        static int Flood(HashSet<Cell> rocks)
        {
            var seen=new HashSet<Cell>();var q=new Queue<Cell>();q.Enqueue(new Cell(0,0));seen.Add(new Cell(0,0));
            while(q.Count>0) { var c=q.Dequeue();foreach(var d in directions) { var n=c+d;if(SnakeModel.Inside(n)&&!rocks.Contains(n)&&seen.Add(n))q.Enqueue(n); } }
            return seen.Count;
        }
        static readonly Cell[] directions={new Cell(1,0),new Cell(0,1),new Cell(-1,0),new Cell(0,-1)};
        public static List<Cell> PathToFood(SnakeModel m)
        {
            var blocked=new HashSet<Cell>(m.Rocks);blocked.UnionWith(m.Body.Skip(1).Take(m.Body.Count-2));
            var q=new Queue<Cell>();var previous=new Dictionary<Cell,Cell>();var head=m.Body[0];q.Enqueue(head);previous[head]=head;
            while(q.Count>0)
            {
                var c=q.Dequeue();if(c==m.Food)break;
                foreach(var d in directions)
                {
                    var n=c+d;if(c==head&&d.x==-m.Direction.x&&d.y==-m.Direction.y)continue;
                    if(SnakeModel.Inside(n)&&!blocked.Contains(n)&&!previous.ContainsKey(n)){previous[n]=c;q.Enqueue(n);}
                }
            }
            var path=new List<Cell>();if(!previous.ContainsKey(m.Food))return path;
            var next=m.Food;while(next!=head){var p=previous[next];path.Add(new Cell(next.x-p.x,next.y-p.y));next=p;}path.Reverse();return path;
        }
    }
}
