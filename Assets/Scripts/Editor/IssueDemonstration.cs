using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// Demonstration script that shows the fix for the specific issue:
    /// Process(false, false) now correctly removes NavMesh Obstacles from prefabs
    /// </summary>
    public class IssueDemonstration
    {
        [MenuItem("Tools/Demonstrate Issue Fix")]
        public static void DemonstrateIssueFix()
        {
            Debug.Log("=== DEMONSTRATING THE NAVMESH OBSTACLE REPLACER FIX ===");
            Debug.Log("");
            Debug.Log("Issue: When running Process(false, false), the tool would scan and find");
            Debug.Log("NavMesh Obstacles but fail to remove them from prefabs.");
            Debug.Log("");
            
            // Clean up first
            TestSetupHelper.CleanTestObjects();
            
            // Create test objects
            Debug.Log("1. Creating test objects with NavMesh Obstacles...");
            TestSetupHelper.CreateTestObjects();
            
            // Count before
            var sceneObstacles = Object.FindObjectsOfType<UnityEngine.AI.NavMeshObstacle>();
            int prefabObstacles = CountPrefabNavMeshObstacles();
            
            Debug.Log($"   Created: {sceneObstacles.Length} scene obstacles, {prefabObstacles} prefab obstacles");
            Debug.Log("");
            
            // Now run the exact problematic scenario
            Debug.Log("2. Running the EXACT issue scenario: Process(false, false)");
            Debug.Log("   - isPreview = false (should make actual changes)");
            Debug.Log("   - includeScene = false (should only process prefabs)");
            Debug.Log("");
            
            var window = EditorWindow.GetWindow<NavMeshObstacleReplacer>();
            window.Process(false, false);
            
            // Count after
            var sceneObstaclesAfter = Object.FindObjectsOfType<UnityEngine.AI.NavMeshObstacle>();
            int prefabObstaclesAfter = CountPrefabNavMeshObstacles();
            
            Debug.Log("");
            Debug.Log("3. RESULTS:");
            Debug.Log($"   Scene obstacles: {sceneObstacles.Length} → {sceneObstaclesAfter.Length} (should be unchanged)");
            Debug.Log($"   Prefab obstacles: {prefabObstacles} → {prefabObstaclesAfter} (should be 0)");
            Debug.Log("");
            
            bool sceneCorrect = sceneObstaclesAfter.Length == sceneObstacles.Length;
            bool prefabCorrect = prefabObstaclesAfter == 0;
            
            if (sceneCorrect && prefabCorrect)
            {
                Debug.Log("✅ SUCCESS: The issue has been FIXED!");
                Debug.Log("   - Scene objects preserved (includeScene=false worked)");
                Debug.Log("   - Prefab obstacles removed (processing worked)");
                Debug.Log("");
                Debug.Log("The tool now correctly:");
                Debug.Log("• Scans for NavMesh Obstacles in prefabs");
                Debug.Log("• Successfully removes them when isPreview=false");
                Debug.Log("• Respects the includeScene parameter");
                Debug.Log("• Generates proper logs with detailed results");
            }
            else
            {
                Debug.LogError("❌ FAILURE: The issue persists!");
                if (!sceneCorrect)
                    Debug.LogError("   Scene objects were modified when they shouldn't be");
                if (!prefabCorrect)
                    Debug.LogError("   Prefab obstacles were not removed");
            }
            
            // Check for log file
            string logDir = "Logs";
            if (Directory.Exists(logDir))
            {
                string[] logFiles = Directory.GetFiles(logDir, "NvMObsReplacer_ReplaceLog_*.txt");
                if (logFiles.Length > 0)
                {
                    Debug.Log("");
                    Debug.Log($"📄 Log file created: {Path.GetFileName(logFiles[logFiles.Length - 1])}");
                    Debug.Log("   Check the log file for detailed operation results");
                }
            }
            
            // Clean up
            TestSetupHelper.CleanTestObjects();
            
            Debug.Log("");
            Debug.Log("=== DEMONSTRATION COMPLETE ===");
        }
        
        private static int CountPrefabNavMeshObstacles()
        {
            int count = 0;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    var obstacles = prefab.GetComponentsInChildren<UnityEngine.AI.NavMeshObstacle>(true);
                    count += obstacles.Length;
                }
            }
            return count;
        }
    }
}