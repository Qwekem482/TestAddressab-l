using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;

public class NvMObsReplacer : EditorWindow
{
    private static string logFilePath;
    private static List<string> logEntries = new List<string>();

    [MenuItem("Tools/NavMeshObstacle Replacer")]
    public static void ShowWindow()
    {
        GetWindow<NvMObsReplacer>("NvMObs Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("NavMeshObstacle Replacer Tool", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Process (Remove: true, Replace: true)"))
        {
            Process(true, true);
        }
        
        if (GUILayout.Button("Process (Remove: true, Replace: false)"))
        {
            Process(true, false);
        }
        
        if (GUILayout.Button("Process (Remove: false, Replace: true)"))
        {
            Process(false, true);
        }
        
        if (GUILayout.Button("Process (Remove: false, Replace: false)"))
        {
            Process(false, false);
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("This tool processes all NavMeshObstacle components in the scene.", EditorStyles.helpBox);
    }

    public static void Process(bool shouldRemove, bool shouldReplace)
    {
        InitializeLogging();
        
        LogEntry($"Starting Process with parameters: shouldRemove={shouldRemove}, shouldReplace={shouldReplace}");
        
        // FIXED: Previous version was only processing one object
        // Now using FindObjectsOfType to get ALL NavMeshObstacle components in the scene
        NavMeshObstacle[] obstacles = FindObjectsOfType<NavMeshObstacle>();
        
        LogEntry($"Found {obstacles.Length} NavMeshObstacle components in the scene");
        
        if (obstacles.Length == 0)
        {
            LogEntry("No NavMeshObstacle components found in the scene");
            SaveLogFile();
            EditorUtility.DisplayDialog("NavMeshObstacle Replacer", "No NavMeshObstacle components found in the scene.", "OK");
            return;
        }
        
        // FIXED: Handle the case where Process(false, false) should only log without making changes
        if (!shouldRemove && !shouldReplace)
        {
            LogEntry("Process called with (false, false) - performing information scan only");
            foreach (NavMeshObstacle obstacle in obstacles)
            {
                if (obstacle != null && obstacle.gameObject != null)
                {
                    LogEntry($"Found NavMeshObstacle on object: {obstacle.gameObject.name} - Carving: {obstacle.carving}, Size: {obstacle.size}");
                }
            }
            LogEntry("Information scan completed. No modifications made.");
            SaveLogFile();
            
            string infoMessage = $"Information scan completed!\n\n" +
                            $"Found {obstacles.Length} NavMeshObstacle components.\n" +
                            $"No modifications made (both parameters were false).\n\n" +
                            $"Log saved to: {logFilePath}";
            EditorUtility.DisplayDialog("NavMeshObstacle Replacer - Info Scan", infoMessage, "OK");
            return;
        }
        
        int processedCount = 0;
        int removedCount = 0;
        int replacedCount = 0;
        int skippedCount = 0;
        
        // FIXED: Now properly iterates through ALL obstacles, not just one
        foreach (NavMeshObstacle obstacle in obstacles)
        {
            if (obstacle == null || obstacle.gameObject == null)
            {
                LogEntry("Skipping null obstacle or gameObject");
                skippedCount++;
                continue;
            }
            
            GameObject targetObject = obstacle.gameObject;
            string objectName = targetObject.name;
            
            LogEntry($"Processing object: {objectName}");
            
            // Record the component for undo
            Undo.RecordObject(targetObject, "NavMeshObstacle Processing");
            
            bool objectModified = false;
            
            if (shouldRemove)
            {
                LogEntry($"Removing NavMeshObstacle from {objectName}");
                Undo.DestroyObjectImmediate(obstacle);
                removedCount++;
                objectModified = true;
                LogEntry($"Successfully removed NavMeshObstacle from {objectName}");
            }
            else
            {
                LogEntry($"Skipping removal of NavMeshObstacle from {objectName} (shouldRemove=false)");
            }
            
            if (shouldReplace && shouldRemove)
            {
                // Add a new component as replacement (example: Collider)
                LogEntry($"Adding replacement Collider to {objectName}");
                if (targetObject.GetComponent<Collider>() == null)
                {
                    BoxCollider newCollider = Undo.AddComponent<BoxCollider>(targetObject);
                    LogEntry($"Added BoxCollider as replacement to {objectName}");
                    replacedCount++;
                    objectModified = true;
                }
                else
                {
                    LogEntry($"Object {objectName} already has a Collider, skipping replacement");
                }
            }
            else if (shouldReplace && !shouldRemove)
            {
                // Modify existing obstacle properties
                LogEntry($"Modifying existing NavMeshObstacle properties on {objectName}");
                bool originalCarving = obstacle.carving;
                obstacle.carving = !obstacle.carving; // Toggle carving as an example modification
                LogEntry($"Modified NavMeshObstacle carving property on {objectName} from {originalCarving} to {obstacle.carving}");
                replacedCount++;
                objectModified = true;
                EditorUtility.SetDirty(obstacle); // Mark as dirty for saving
            }
            else if (!shouldReplace)
            {
                LogEntry($"Skipping replacement for {objectName} (shouldReplace=false)");
            }
            
            if (objectModified)
            {
                processedCount++;
            }
        }
        
        LogEntry($"Processing completed. Total objects found: {obstacles.Length}, Processed/Modified: {processedCount}, Removed: {removedCount}, Replaced/Modified: {replacedCount}, Skipped: {skippedCount}");
        
        // Mark scene as dirty to save changes
        if (processedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        SaveLogFile();
        
        string message = $"Processing completed!\n\n" +
                        $"Objects found: {obstacles.Length}\n" +
                        $"Objects modified: {processedCount}\n" +
                        $"Components removed: {removedCount}\n" +
                        $"Components replaced/modified: {replacedCount}\n" +
                        (skippedCount > 0 ? $"Objects skipped: {skippedCount}\n" : "") +
                        $"\nLog saved to: {logFilePath}";
        
        EditorUtility.DisplayDialog("NavMeshObstacle Replacer", message, "OK");
    }
    
    private static void InitializeLogging()
    {
        logEntries.Clear();
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"NvMObsReplacer_ReplaceLog_{timestamp}.txt";
        logFilePath = Path.Combine(Application.dataPath, "..", fileName);
        
        LogEntry("=== NavMeshObstacle Replacer Log ===");
        LogEntry($"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        LogEntry($"Unity Version: {Application.unityVersion}");
        LogEntry($"Scene: {UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name}");
        LogEntry("=====================================");
    }
    
    private static void LogEntry(string message)
    {
        string logLine = $"[{DateTime.Now:HH:mm:ss}] {message}";
        logEntries.Add(logLine);
        Debug.Log($"NvMObsReplacer: {message}");
    }
    
    private static void SaveLogFile()
    {
        try
        {
            logEntries.Add("=====================================");
            logEntries.Add($"Log ended at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            File.WriteAllLines(logFilePath, logEntries);
            LogEntry($"Log file saved successfully to: {logFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save log file: {ex.Message}");
        }
    }
}
#endif