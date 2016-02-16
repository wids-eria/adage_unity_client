using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.FacebookEditor;
using UnityEditor.XCodeEditor;

namespace UnityEditor.FacebookEditor
{
    public static class XCodePostProcess
    {
        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            // If integrating with facebook on any platform, throw a warning if the app id is invalid
            if (!FBSettings.IsValidAppId)
            {
                Debug.LogWarning("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
				//return;
            }


#if UNITY_5
            if (target == BuildTarget.iOS)
#else
            if (target == BuildTarget.iPhone)
#endif
            {
                UnityEditor.XCodeEditor.XCProject project = new UnityEditor.XCodeEditor.XCProject(path);

                // Find and run through all projmods files to patch the project

                string projModPath = System.IO.Path.Combine(Application.dataPath, FBSettings.PluginLocation + "Facebook/Editor/iOS");
				Debug.Log(projModPath);
                var files = System.IO.Directory.GetFiles(projModPath, "*.projmods", System.IO.SearchOption.AllDirectories);
                foreach (var file in files)
                {
					Debug.Log(System.IO.Path.Combine(Application.dataPath, FBSettings.PluginLocation));
					project.ApplyMod(System.IO.Path.Combine(Application.dataPath, FBSettings.PluginLocation), file);
                }
                project.Save();

                PlistMod.UpdatePlist(path, FBSettings.AppId);
                FixupFiles.FixSimulator(path);

                FixupFiles.AddVersionDefine(path);
            }

            if (target == BuildTarget.Android)
            {
                // The default Bundle Identifier for Unity does magical things that causes bad stuff to happen
                if (PlayerSettings.bundleIdentifier == "com.Company.ProductName")
                {
                    Debug.LogError("The default Unity Bundle Identifier (com.Company.ProductName) will not work correctly.");
                }
                if (!FacebookAndroidUtil.IsSetupProperly())
                {
                    Debug.LogError("Your Android setup is not correct. See Settings in Facebook menu.");
                }
            }
        }
    }
}
