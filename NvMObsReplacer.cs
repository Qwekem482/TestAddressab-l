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
        
        // Find all NavMeshObstacle components in the scene
        NavMeshObstacle[] obstacles = FindObjectsOfType<NavMeshObstacle>();
        
        LogEntry($"Found {obstacles.Length} NavMeshObstacle components in the scene");
        
        if (obstacles.Length == 0)
        {
            LogEntry("No NavMeshObstacle components found in the scene");
            SaveLogFile();
            EditorUtility.DisplayDialog("NavMeshObstacle Replacer", "No NavMeshObstacle components found in the scene.", "OK");
            return;
        }
        
        int processedCount = 0;
        int removedCount = 0;
        int replacedCount = 0;
        
        foreach (NavMeshObstacle obstacle in obstacles)
        {
            if (obstacle == null || obstacle.gameObject == null)
            {
                LogEntry("Skipping null obstacle or gameObject");
                continue;
            }
            
            GameObject targetObject = obstacle.gameObject;
            string objectName = targetObject.name;
            
            LogEntry($"Processing object: {objectName}");
            
            // Record the component for undo
            Undo.RecordObject(targetObject, "NavMeshObstacle Processing");
            
            if (shouldRemove)
            {
                LogEntry($"Removing NavMeshObstacle from {objectName}");
                Undo.DestroyObjectImmediate(obstacle);
                removedCount++;
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
                obstacle.carving = !obstacle.carving; // Toggle carving as an example modification
                LogEntry($"Modified NavMeshObstacle carving property on {objectName} to {obstacle.carving}");
                replacedCount++;
            }
            else if (!shouldReplace)
            {
                LogEntry($"Skipping replacement for {objectName} (shouldReplace=false)");
            }
            
            processedCount++;
        }
        
        LogEntry($"Processing completed. Processed: {processedCount}, Removed: {removedCount}, Replaced: {replacedCount}");
        
        // Mark scene as dirty to save changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        SaveLogFile();
        
        string message = $"Processing completed!\n\n" +
                        $"Objects processed: {processedCount}\n" +
                        $"Components removed: {removedCount}\n" +
                        $"Components replaced/modified: {replacedCount}\n\n" +
                        $"Log saved to: {logFilePath}";
        
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