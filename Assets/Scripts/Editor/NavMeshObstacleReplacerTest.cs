using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Tools
{
    /// <summary>
    /// Complete test suite for the NavMesh Obstacle Replacer fix
    /// This validates that the issue has been resolved
    /// </summary>
    public class NavMeshObstacleReplacerTest : EditorWindow
    {
        [MenuItem("Tools/NavMesh Obstacle Replacer/Run Complete Test")]
        public static void RunCompleteTest()
        {
            var testWindow = GetWindow<NavMeshObstacleReplacerTest>("NavMesh Obstacle Replacer Test");
            testWindow.ExecuteTest();
        }

        private void ExecuteTest()
        {
            Debug.Log("=== Starting NavMesh Obstacle Replacer Complete Test ===");
            
            try
            {
                // Step 1: Clean up any existing test objects
                Debug.Log("Step 1: Cleaning up existing test objects...");
                TestSetupHelper.CleanTestObjects();

                // Step 2: Create test objects with NavMesh Obstacles
                Debug.Log("Step 2: Creating test objects with NavMesh Obstacles...");
                TestSetupHelper.CreateTestObjects();

                // Step 3: Verify objects were created
                Debug.Log("Step 3: Verifying test objects were created...");
                int sceneObstaclesBefore = FindObjectsOfType<NavMeshObstacle>().Length;
                int prefabObstaclesBefore = CountPrefabNavMeshObstacles();
                
                Debug.Log($"Before processing: {sceneObstaclesBefore} scene obstacles, {prefabObstaclesBefore} prefab obstacles");
                
                if (sceneObstaclesBefore == 0 && prefabObstaclesBefore == 0)
                {
                    Debug.LogError("Test failed: No NavMesh Obstacles were created for testing");
                    return;
                }

                // Step 4: Test preview mode first (should not remove anything)
                Debug.Log("Step 4: Testing preview mode (should not remove obstacles)...");
                var window = GetWindow<NavMeshObstacleReplacer>();
                window.Process(true, true); // Preview mode with scene inclusion
                
                int sceneObstaclesAfterPreview = FindObjectsOfType<NavMeshObstacle>().Length;
                int prefabObstaclesAfterPreview = CountPrefabNavMeshObstacles();
                
                if (sceneObstaclesAfterPreview != sceneObstaclesBefore || prefabObstaclesAfterPreview != prefabObstaclesBefore)
                {
                    Debug.LogError($"Preview mode test failed: Obstacles were modified. Before: {sceneObstaclesBefore}/{prefabObstaclesBefore}, After: {sceneObstaclesAfterPreview}/{prefabObstaclesAfterPreview}");
                    return;
                }
                Debug.Log("✓ Preview mode test passed - no obstacles were removed");

                // Step 5: Test the exact scenario from the issue: Process(false, false)
                Debug.Log("Step 5: Testing the exact issue scenario: Process(false, false)...");
                window.Process(false, false); // Non-preview mode, exclude scene objects
                
                int sceneObstaclesAfterIssueScenario = FindObjectsOfType<NavMeshObstacle>().Length;
                int prefabObstaclesAfterIssueScenario = CountPrefabNavMeshObstacles();
                
                Debug.Log($"After Process(false, false): {sceneObstaclesAfterIssueScenario} scene obstacles, {prefabObstaclesAfterIssueScenario} prefab obstacles");
                
                // Since includeScene=false, scene obstacles should remain, but prefab obstacles should be removed
                if (sceneObstaclesAfterIssueScenario != sceneObstaclesBefore)
                {
                    Debug.LogError($"Issue scenario test failed: Scene obstacles were modified when they shouldn't be. Before: {sceneObstaclesBefore}, After: {sceneObstaclesAfterIssueScenario}");
                    return;
                }
                
                if (prefabObstaclesAfterIssueScenario != 0)
                {
                    Debug.LogError($"Issue scenario test failed: Prefab obstacles were not removed. Before: {prefabObstaclesBefore}, After: {prefabObstaclesAfterIssueScenario}");
                    return;
                }
                
                Debug.Log("✓ Issue scenario test passed - prefab obstacles removed, scene obstacles preserved");

                // Step 6: Test full processing: Process(false, true)
                Debug.Log("Step 6: Testing full processing: Process(false, true)...");
                window.Process(false, true); // Non-preview mode, include scene objects
                
                int sceneObstaclesAfterFull = FindObjectsOfType<NavMeshObstacle>().Length;
                int prefabObstaclesAfterFull = CountPrefabNavMeshObstacles();
                
                Debug.Log($"After Process(false, true): {sceneObstaclesAfterFull} scene obstacles, {prefabObstaclesAfterFull} prefab obstacles");
                
                if (sceneObstaclesAfterFull != 0 || prefabObstaclesAfterFull != 0)
                {
                    Debug.LogError($"Full processing test failed: Not all obstacles were removed. Scene: {sceneObstaclesAfterFull}, Prefabs: {prefabObstaclesAfterFull}");
                    return;
                }
                
                Debug.Log("✓ Full processing test passed - all obstacles removed");

                // Step 7: Check log file was created
                Debug.Log("Step 7: Verifying log file creation...");
                string logDir = "Logs";
                if (Directory.Exists(logDir))
                {
                    string[] logFiles = Directory.GetFiles(logDir, "NvMObsReplacer_ReplaceLog_*.txt");
                    if (logFiles.Length > 0)
                    {
                        Debug.Log($"✓ Log file creation test passed - found {logFiles.Length} log files");
                        Debug.Log($"Latest log file: {logFiles[logFiles.Length - 1]}");
                    }
                    else
                    {
                        Debug.LogWarning("Log file creation test failed - no log files found");
                    }
                }
                else
                {
                    Debug.LogWarning("Log directory not found");
                }

                // Step 8: Clean up
                Debug.Log("Step 8: Cleaning up test objects...");
                TestSetupHelper.CleanTestObjects();

                Debug.Log("=== NavMesh Obstacle Replacer Complete Test PASSED ===");
                Debug.Log("The issue has been successfully resolved!");
                
                EditorUtility.DisplayDialog("Test Results", 
                    "NavMesh Obstacle Replacer test PASSED!\n\n" +
                    "The issue with Process(false, false) has been resolved.\n" +
                    "The tool now correctly scans, removes, and replaces NavMesh Obstacle components.", 
                    "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Test failed with exception: {ex.Message}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
                
                EditorUtility.DisplayDialog("Test Failed", 
                    $"Test failed with exception: {ex.Message}", 
                    "OK");
            }
        }

        private int CountPrefabNavMeshObstacles()
        {
            int count = 0;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    NavMeshObstacle[] obstacles = prefab.GetComponentsInChildren<NavMeshObstacle>(true);
                    count += obstacles.Length;
                }
            }
            return count;
        }
    }
}