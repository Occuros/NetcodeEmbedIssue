using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEditor.Compilation;
using UnityEngine;

namespace Unity.NetCode.Editor
{
    /// <summary>
    /// Simple utility class that cache and restore netcode generated file in the temp folder.
    /// This help in general when a project is re-open and the previously generated files has been removed.
    /// </summary>

    //There are a couple of problem by storing the files in the Temp folder: all generated files are lost when the editor quit.
    //A solution would be to store them in another persistent folder, like Library or Assets/NetCodeGenerated~.
    //In both cases there is no easy way from the source generator to cleanup any deleted or removed assemblies or
    //assemblies who don't have any ghost / rpc to generate anymore.
    // Our current proposed solution:
    // 1- We store generated files for each assembly in seperate folders. The folder is deleted and repopulated any time that assembly is rebuit.
    //    This way we are guarantee that the generated files are always in sync with the assembly contents after any update.
    // 2- When the editor is going to quit, we make a copy the Temp/NetCodeGenerated folder in the Library/NetCodeGenerated_Backup.
    // 3- When the editor is reopend, if the Temp/NetCodeGenerated directory does not exists, all the files are copied back. Otherwise,
    //    only the subset of directory that are not present in Temp but in the backup are restored (we can eventually check the date)
    //
    // An extra pass that check for all the assembly in the compilation pipeline and remove any not present anymore
    // is done by the CodeGenService in the Unity.NetCode.Editor, that will take care of that.
    //
    // USE CASES TAKEN INTO EXAMS:
    //- Editor close/re-open, no changes. It work as expected.
    //- Editor crash/re-open. Temp is never deleted in that case so should be ok
    //- When checkout/update to a newer version
    //   a) With editor closed: When the editor is opened again, if something in code changed, a first compilation pass is always done before
    //                          the domain reload. That means is safe to assume that whatever has been regenerated by the deps tree
    //                          is the most up-to-date version of the assembly generated files. Only the folders that are not present in the Temp
    //                          but that exists in the project and in the backup are copied back.
    //   b) With editor opened: The files were already copied so is something is updated, the changes are reflected.

    internal class BackupAndRestoreGenFiles : ScriptableSingleton<BackupAndRestoreGenFiles>
    {
        [SerializeField] private bool restoredBackup;
        public bool HasRestoredBackup => restoredBackup;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.quitting += () =>
            {
                var tempDir = Path.GetFullPath("Temp/NetCodeGenerated");
                var backupDir = Path.GetFullPath("Library/NetCodeGenerated_Backup");
                //Move the temp generator directory to a backup location so if we reload the editor
                //we still have something to look at
                if (Directory.Exists(tempDir))
                {
                    FileUtil.ReplaceDirectory(tempDir, backupDir);
                }
            };
            if (!instance.HasRestoredBackup)
                instance.BackupGeneratedFiles();
        }

        public void BackupGeneratedFiles()
        {
            var tempDir = Path.GetFullPath("Temp/NetCodeGenerated");
            var backupDir = Path.GetFullPath("Library/NetCodeGenerated_Backup");
            //This is usually happen if the editor is re-opened
            if (Directory.Exists(backupDir) && !Directory.Exists(tempDir))
            {
                FileUtil.CopyFileOrDirectory(backupDir, tempDir);
            }
            //Do this only once, not every domain reload
            else if (Directory.Exists(backupDir) && Directory.Exists(tempDir))
            {
                // And that is when we have partial update to do:
                // a) If an assembly has been removed. The older folder is not deleted neither in the backup and temp.
                // b) An assembly does not have any more ghosts/rpc/commands. if the regeneration happens before the backup is restored
                //    the folder folders in not updated correctly.
                var assemblySet =
                    new HashSet<string>(CompilationPipeline.GetAssemblies().Select(a => a.name));
                foreach (var d in Directory.GetDirectories(tempDir))
                {
                    //If that assembly does not exist anymore -> delete the generated folder
                    var relativePath = Path.GetFileName(d);
                    if (!assemblySet.Contains(relativePath))
                    {
                        FileUtil.DeleteFileOrDirectory(d);
                        FileUtil.DeleteFileOrDirectory(Path.Combine(backupDir, relativePath));
                    }
                }

                foreach (var d in assemblySet)
                {
                    var infoTemp = new DirectoryInfo(Path.Combine(tempDir, d));
                    var infoBackup = new DirectoryInfo(Path.Combine(backupDir, d));
                    if (!infoBackup.Exists)
                        continue;
                    //Just copy non existing dir or more recent ones
                    if (!infoTemp.Exists || infoBackup.LastWriteTimeUtc > infoTemp.LastWriteTimeUtc)
                        FileUtil.ReplaceDirectory(infoBackup.FullName, infoTemp.FullName);
                }
            }
            restoredBackup = true;
        }
    }
}
