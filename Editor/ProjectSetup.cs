using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using static System.Environment;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class ProjectSetup
{
    [MenuItem("Tools/Open Asset Folder")]
    public static void OpenAssetFolder()
    {
        string path = "C:/Users/82688/AppData/Roaming/Unity/Asset Store-5.x";
        if (Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            Debug.LogError("The folder '_Project' does not exist.");
        }
    }
    
    [MenuItem("Tools/Setup/Assets/Import Odin")]
    public static void ImportOdin()
    {
        Assets.ImportAsset("Odin Inspector and Serializer.unitypackage", "Sirenix/Editor ExtensionsSystem");
    }
    
    [MenuItem("Tools/Setup/Assets/Import Editor Extensions")]
    public static void ImportEditorExtensions()
    {
        Assets.ImportAsset("vFolders 2", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("vInspector 2", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("vHierarchy 2", "NoLicense");
    }
    
    [MenuItem("Tools/Setup/Assets/Import Console Pro")]
    public static void ImportConsolePro()
    {
        Assets.ImportAsset("Editor Console Pro v3.975", "NoLicense");
    }

    [MenuItem("Tools/Setup/Packages/Install Essential Packages")]
    public static void InstallPackages()
    {
        // Packages.InstallPackages(new[]
        // {
        //     "com.unity.2d.animation",
        //     "git+https://github.com/adammyhre/Unity-Utils.git",
        //     "git+https://github.com/adammyhre/Unity-Improved-Timers.git",
        //     "git+https://github.com/KyleBanks/scene-ref-attribute.git"
        //     // If necessary, import new Input System last as it requires a Unity Editor restart
        //     // "com.unity.inputsystem"
        // });
    }

    [MenuItem("Tools/Setup/Create Folders", priority = 0)]
    public static void CreateFolders()
    {
        Folders.Create("_Project", "Animation", "Materials", "Prefabs");
        Refresh();
        Folders.Move("Scenes", "_Project");
        Refresh();
        // Optional: Disable Domain Reload
        // EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
    }

    static class Assets
    {
        public static void ImportAsset(string asset, string folder)
        {
            string basePath;
            if (OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
            {
                string homeDirectory = GetFolderPath(Environment.SpecialFolder.Personal);
                basePath = Combine(homeDirectory, "Library/Unity/Asset Store-5.x");
            }
            else
            {
                string defaultPath = Combine(GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                basePath = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
            }

            asset = asset.EndsWith(".unitypackage") ? asset : asset + ".unitypackage";

            string fullPath = Combine(basePath, folder, asset);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
            }

            ImportPackage(fullPath, false);
        }
    }

    static class Packages {
        static AddRequest request;
        static Queue<string> packagesToInstall = new Queue<string>();

        public static void InstallPackages(string[] packages) {
            foreach (var package in packages) {
                packagesToInstall.Enqueue(package);
            }

            if (packagesToInstall.Count > 0) {
                StartNextPackageInstallation();
            }
        }

        static async void StartNextPackageInstallation() {
            request = Client.Add(packagesToInstall.Dequeue());
            
            while (!request.IsCompleted) await Task.Delay(10);
            
            if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
            else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);

            if (packagesToInstall.Count > 0) {
                await Task.Delay(1000);
                StartNextPackageInstallation();
            }
        }
    }
    
    static class Folders {
        public static void Create(string root, params string[] folders) {
            var fullpath = Combine(Application.dataPath, root);
            if (!Directory.Exists(fullpath)) {
                Directory.CreateDirectory(fullpath);
            }

            foreach (var folder in folders) {
                CreateSubFolders(fullpath, folder);
            }
        }
        
        static void CreateSubFolders(string rootPath, string folderHierarchy) {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders) {
                currentPath = Combine(currentPath, folder);
                if (!Directory.Exists(currentPath)) {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
        
        public static void Move(string folderName, string newParent) {
            var sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath)) {
                var destinationPath = $"Assets/{newParent}/{folderName}";
                var error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogError($"Failed to move {folderName}: {error}");
                }
            }
            else
            {
                Debug.LogError($"Folder {folderName} does not exist.");
            }
        }
        
        public static void Delete(string folderName) {
            var pathToDelete = $"Assets/{folderName}";

            if (IsValidFolder(pathToDelete)) {
                DeleteAsset(pathToDelete);
            }
        }
    }
}