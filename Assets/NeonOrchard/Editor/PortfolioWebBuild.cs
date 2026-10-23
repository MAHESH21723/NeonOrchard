using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NeonOrchard.Editor
{
    public static class PortfolioWebBuild
    {
        public static void Build()
        {
            OrchardBuilder.Configure();
            OrchardBuilder.Tests();
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
            Directory.CreateDirectory("Builds/WebGL");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { OrchardBuilder.ScenePath },
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            File.WriteAllText("Release/WebBuild.txt", report.summary.result + "\n" + report.summary.totalSize + " bytes\n" + report.summary.totalTime);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Neon Orchard Web build failed.");
        }
    }
}
