using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;


public class BuildPostProcessor
{

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            Debug.Log("PASHA PostProcess");
            PBXProject project = new PBXProject();
            string sPath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(sPath);

            string targetGuid = project.GetUnityFrameworkTargetGuid();
            //string targetGuid = project.TargetGuidByName(targetName);

            project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

            File.WriteAllText(sPath, project.WriteToString());
        }
    }
}