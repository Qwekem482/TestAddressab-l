using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Tools
{
    public class TestSetupHelper : EditorWindow
    {
        [MenuItem("Tools/Test Setup/Create Test Objects with NavMesh Obstacles")]
        public static void CreateTestObjects()
        {
            // Create a test GameObject in the scene with NavMesh Obstacle
            GameObject testSceneObject = new GameObject("TestSceneObject");
            testSceneObject.AddComponent<NavMeshObstacle>();
            testSceneObject.AddComponent<TestData>();
            testSceneObject.GetComponent<TestData>().testName = "Scene Test Object";
            
            // Create a test prefab with NavMesh Obstacle
            GameObject testPrefabObject = new GameObject("TestPrefabObject");
            testPrefabObject.AddComponent<NavMeshObstacle>();
            testPrefabObject.AddComponent<TestData>();
            testPrefabObject.GetComponent<TestData>().testName = "Prefab Test Object";
            
            // Ensure Assets/Prefabs directory exists
            string prefabDir = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            // Save as prefab
            string prefabPath = "Assets/Prefabs/TestPrefabWithNavMeshObstacle.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(testPrefabObject, prefabPath, InteractionMode.UserAction);
            
            // Create another nested test prefab
            GameObject parentObject = new GameObject("ParentTestObject");
            GameObject childObject = new GameObject("ChildWithNavMeshObstacle");
            childObject.transform.SetParent(parentObject.transform);
            childObject.AddComponent<NavMeshObstacle>();
            childObject.AddComponent<TestData>();
            childObject.GetComponent<TestData>().testName = "Child Test Object";
            
            string nestedPrefabPath = "Assets/Prefabs/NestedTestPrefabWithNavMeshObstacle.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(parentObject, nestedPrefabPath, InteractionMode.UserAction);
            
            Debug.Log("Created test objects with NavMesh Obstacles:");
            Debug.Log("- 1 scene object: TestSceneObject");
            Debug.Log("- 2 prefabs: TestPrefabWithNavMeshObstacle.prefab, NestedTestPrefabWithNavMeshObstacle.prefab");
            
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Test Setup/Clean Test Objects")]
        public static void CleanTestObjects()
        {
            // Remove scene objects
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in sceneObjects)
            {
                if (obj.name.Contains("Test") && obj.GetComponent<TestData>() != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            // Remove test prefabs
            string[] prefabPaths = {
                "Assets/Prefabs/TestPrefabWithNavMeshObstacle.prefab",
                "Assets/Prefabs/NestedTestPrefabWithNavMeshObstacle.prefab"
            };
            
            foreach (string path in prefabPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Cleaned up test objects");
        }
    }
}