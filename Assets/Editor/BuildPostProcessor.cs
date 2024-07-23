using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.Assertions;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

public class BuildPostProcessor
{
    private const string FRAMEWORK_ORIGIN_PATH = "Assets/Plugins/iOS"; // relative to project folder
    private const string FRAMEWORK_TARGET_PATH = "Frameworks/io.start.xcframework"; // relative to build folder

    private const string START_IO_FRAMEWORK_NAME = "StartApp.xcframework";

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            string sourcePath = Path.Combine(FRAMEWORK_ORIGIN_PATH, START_IO_FRAMEWORK_NAME);
            string destPath = Path.Combine(FRAMEWORK_TARGET_PATH, START_IO_FRAMEWORK_NAME);

            CopyAndReplaceDirectory(sourcePath, Path.Combine(path, destPath), true);

            PBXProject project = new PBXProject();
            string sPath = PBXProject.GetPBXProjectPath(path);
            project.ReadFromFile(sPath);

            string targetGuid = project.GetUnityFrameworkTargetGuid();
            project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

            string startioXCFrameworkGuid = project.AddFile(destPath, destPath);
            var frameworkLinkPhaseGuid = project.GetFrameworksBuildPhaseByTarget(targetGuid);
            project.AddFileToBuildSection(targetGuid, frameworkLinkPhaseGuid, startioXCFrameworkGuid);

            File.WriteAllText(sPath, project.WriteToString());
        }
    }

    private static void CopyAndReplaceDirectory(string sourcePath, string destPath, bool ignoreMetaFiles)
    {
        if (Directory.Exists(destPath))
            Directory.Delete(destPath, true);

        Directory.CreateDirectory(destPath);

        foreach (var file in Directory.GetFiles(sourcePath))
        {
            if (ignoreMetaFiles && Path.GetExtension(file).Equals(".meta"))
                continue;
            File.Copy(file, Path.Combine(destPath, Path.GetFileName(file)));
        }

        foreach (var dir in Directory.GetDirectories(sourcePath))
            CopyAndReplaceDirectory(dir, Path.Combine(destPath, Path.GetFileName(dir)), ignoreMetaFiles);
    }
}
#endif