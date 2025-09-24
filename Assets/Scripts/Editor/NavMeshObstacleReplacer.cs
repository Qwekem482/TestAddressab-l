using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Tools
{
    public class NavMeshObstacleReplacer : EditorWindow
    {
        private bool _isPreview = true;
        private bool _includeScene = true;
        private string _logPath = "Logs";
        private List<GameObject> _foundGameObjects = new List<GameObject>();
        private List<string> _foundPrefabPaths = new List<string>();
        private Vector2 _scrollPosition;

        [MenuItem("Tools/NavMesh Obstacle Replacer")]
        public static void ShowWindow()
        {
            GetWindow<NavMeshObstacleReplacer>("NavMesh Obstacle Replacer");
        }

        private void OnGUI()
        {
            GUILayout.Label("NavMesh Obstacle Replacer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _isPreview = EditorGUILayout.Toggle("Preview Mode", _isPreview);
            _includeScene = EditorGUILayout.Toggle("Include Scene Objects", _includeScene);
            
            GUILayout.Space(10);

            if (GUILayout.Button("Scan for NavMesh Obstacles"))
            {
                ScanForNavMeshObstacles();
            }

            if (GUILayout.Button("Process"))
            {
                Process(_isPreview, _includeScene);
            }

            GUILayout.Space(10);

            // Display found objects
            if (_foundGameObjects.Count > 0 || _foundPrefabPaths.Count > 0)
            {
                GUILayout.Label($"Found {_foundGameObjects.Count} scene objects and {_foundPrefabPaths.Count} prefabs with NavMesh Obstacles:", EditorStyles.boldLabel);
                
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
                
                foreach (var obj in _foundGameObjects)
                {
                    EditorGUILayout.ObjectField("Scene Object", obj, typeof(GameObject), true);
                }
                
                foreach (var prefabPath in _foundPrefabPaths)
                {
                    EditorGUILayout.LabelField("Prefab", prefabPath);
                }
                
                EditorGUILayout.EndScrollView();
            }
        }

        public void Process(bool isPreview, bool includeScene)
        {
            _isPreview = isPreview;
            _includeScene = includeScene;
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFileName = $"NvMObsReplacer_ReplaceLog_{timestamp}.txt";
            string logFilePath = Path.Combine(_logPath, logFileName);
            
            // Ensure log directory exists
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            List<string> logEntries = new List<string>();
            logEntries.Add($"NavMesh Obstacle Replacer Log - {DateTime.Now}");
            logEntries.Add($"Preview Mode: {isPreview}");
            logEntries.Add($"Include Scene: {includeScene}");
            logEntries.Add("=".PadRight(50, '='));

            try
            {
                // First scan for all NavMesh Obstacles
                ScanForNavMeshObstacles();
                
                logEntries.Add($"Found {_foundGameObjects.Count} scene objects with NavMesh Obstacles");
                logEntries.Add($"Found {_foundPrefabPaths.Count} prefabs with NavMesh Obstacles");
                logEntries.Add("");

                int processedCount = 0;
                int errorCount = 0;

                // Process scene objects if includeScene is true
                if (includeScene)
                {
                    foreach (var gameObject in _foundGameObjects)
                    {
                        try
                        {
                            bool success = ProcessGameObject(gameObject, isPreview);
                            if (success)
                            {
                                processedCount++;
                                logEntries.Add($"SUCCESS: Processed scene object '{gameObject.name}'");
                            }
                            else
                            {
                                errorCount++;
                                logEntries.Add($"ERROR: Failed to process scene object '{gameObject.name}'");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            logEntries.Add($"EXCEPTION: Error processing scene object '{gameObject.name}': {ex.Message}");
                        }
                    }
                }

                // Process prefabs
                foreach (var prefabPath in _foundPrefabPaths)
                {
                    try
                    {
                        bool success = ProcessPrefab(prefabPath, isPreview);
                        if (success)
                        {
                            processedCount++;
                            logEntries.Add($"SUCCESS: Processed prefab '{prefabPath}'");
                        }
                        else
                        {
                            errorCount++;
                            logEntries.Add($"ERROR: Failed to process prefab '{prefabPath}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        logEntries.Add($"EXCEPTION: Error processing prefab '{prefabPath}': {ex.Message}");
                    }
                }

                logEntries.Add("");
                logEntries.Add("=".PadRight(50, '='));
                logEntries.Add($"Summary:");
                logEntries.Add($"Total objects processed: {processedCount}");
                logEntries.Add($"Total errors: {errorCount}");
                logEntries.Add($"Preview mode: {(isPreview ? "No changes made" : "Changes applied")}");

                // Write log to file
                File.WriteAllLines(logFilePath, logEntries);
                
                string message = $"Process completed! Processed: {processedCount}, Errors: {errorCount}. Log saved to: {logFilePath}";
                if (isPreview)
                {
                    message += " (Preview mode - no changes made)";
                }
                
                EditorUtility.DisplayDialog("NavMesh Obstacle Replacer", message, "OK");
                
                Debug.Log($"NavMesh Obstacle Replacer: {message}");
            }
            catch (Exception ex)
            {
                logEntries.Add($"CRITICAL ERROR: {ex.Message}");
                logEntries.Add($"Stack trace: {ex.StackTrace}");
                File.WriteAllLines(logFilePath, logEntries);
                
                EditorUtility.DisplayDialog("Error", $"Critical error occurred: {ex.Message}", "OK");
                Debug.LogError($"NavMesh Obstacle Replacer critical error: {ex.Message}");
            }
        }

        private void ScanForNavMeshObstacles()
        {
            _foundGameObjects.Clear();
            _foundPrefabPaths.Clear();

            try
            {
                // Scan scene objects if includeScene is true
                if (_includeScene)
                {
                    NavMeshObstacle[] sceneObstacles = FindObjectsOfType<NavMeshObstacle>();
                    foreach (var obstacle in sceneObstacles)
                    {
                        if (obstacle != null && obstacle.gameObject != null && !_foundGameObjects.Contains(obstacle.gameObject))
                        {
                            _foundGameObjects.Add(obstacle.gameObject);
                        }
                    }
                    Debug.Log($"Scan completed: Found {_foundGameObjects.Count} scene objects with NavMesh Obstacles");
                }

                // Scan prefabs
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
                foreach (string guid in prefabGuids)
                {
                    try
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        
                        if (prefab != null)
                        {
                            NavMeshObstacle[] obstacles = prefab.GetComponentsInChildren<NavMeshObstacle>(true);
                            if (obstacles.Length > 0)
                            {
                                _foundPrefabPaths.Add(path);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error scanning prefab with GUID {guid}: {ex.Message}");
                    }
                }
                Debug.Log($"Scan completed: Found {_foundPrefabPaths.Count} prefabs with NavMesh Obstacles");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during NavMesh Obstacle scan: {ex.Message}");
                EditorUtility.DisplayDialog("Scan Error", $"Error occurred during scan: {ex.Message}", "OK");
            }
        }

        private bool ProcessGameObject(GameObject gameObject, bool isPreview)
        {
            if (gameObject == null) 
            {
                Debug.LogError("ProcessGameObject: gameObject is null");
                return false;
            }

            NavMeshObstacle[] obstacles = gameObject.GetComponents<NavMeshObstacle>();
            if (obstacles.Length == 0) 
            {
                Debug.LogWarning($"ProcessGameObject: No NavMeshObstacle components found on {gameObject.name}");
                return false;
            }

            if (!isPreview)
            {
                try
                {
                    // Record the object for undo
                    Undo.RecordObject(gameObject, "Remove NavMesh Obstacles");
                    
                    // Remove all NavMesh Obstacle components (iterate backwards to avoid index issues)
                    for (int i = obstacles.Length - 1; i >= 0; i--)
                    {
                        var obstacle = obstacles[i];
                        if (obstacle != null)
                        {
                            Undo.DestroyObjectImmediate(obstacle);
                        }
                    }

                    // Mark the scene as dirty
                    EditorUtility.SetDirty(gameObject);
                    
                    // Force the scene to refresh
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception while processing GameObject {gameObject.name}: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        private bool ProcessPrefab(string prefabPath, bool isPreview)
        {
            if (string.IsNullOrEmpty(prefabPath)) return false;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return false;

            NavMeshObstacle[] obstacles = prefab.GetComponentsInChildren<NavMeshObstacle>(true);
            if (obstacles.Length == 0) return false;

            if (!isPreview)
            {
                try
                {
                    // Load the prefab for editing
                    string assetPath = AssetDatabase.GetAssetPath(prefab);
                    GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);
                    
                    if (prefabInstance == null)
                    {
                        Debug.LogError($"Failed to load prefab contents for: {assetPath}");
                        return false;
                    }

                    // Find all NavMesh Obstacle components in the prefab instance
                    NavMeshObstacle[] prefabObstacles = prefabInstance.GetComponentsInChildren<NavMeshObstacle>(true);
                    
                    // Remove all NavMesh Obstacle components from the prefab
                    for (int i = prefabObstacles.Length - 1; i >= 0; i--)
                    {
                        var obstacle = prefabObstacles[i];
                        if (obstacle != null)
                        {
                            DestroyImmediate(obstacle, true);
                        }
                    }

                    // Save the prefab
                    bool saveSuccess = PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                    if (!saveSuccess)
                    {
                        Debug.LogError($"Failed to save prefab: {assetPath}");
                        PrefabUtility.UnloadPrefabContents(prefabInstance);
                        return false;
                    }

                    // Unload the prefab contents
                    PrefabUtility.UnloadPrefabContents(prefabInstance);

                    // Refresh the asset database
                    AssetDatabase.Refresh();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception while processing prefab {prefabPath}: {ex.Message}");
                    return false;
                }
            }

            return true;
        }
    }
}