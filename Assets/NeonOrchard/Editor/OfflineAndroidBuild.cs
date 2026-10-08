#if UNITY_ANDROID
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;

namespace NeonOrchard.Editor
{
    // The game is intentionally offline. Unity can infer INTERNET permission from
    // assemblies present for editor tooling; remove it at the manifest merge layer.
    public sealed class OfflineAndroidBuild : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 10000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string file=Path.Combine(path,"src/main/AndroidManifest.xml");
            var doc=XDocument.Load(file);var root=doc.Root;
            XNamespace android="http://schemas.android.com/apk/res/android";
            XNamespace tools="http://schemas.android.com/tools";
            root.SetAttributeValue(XNamespace.Xmlns+"tools",tools.NamespaceName);
            foreach(string permission in new[]{"android.permission.INTERNET","android.permission.ACCESS_NETWORK_STATE"})
            {
                foreach(var existing in root.Elements("uses-permission").Where(e=>(string)e.Attribute(android+"name")==permission).ToList())existing.Remove();
                root.Add(new XElement("uses-permission",new XAttribute(android+"name",permission),new XAttribute(tools+"node","remove")));
            }
            doc.Save(file);
        }
    }
}
#endif
