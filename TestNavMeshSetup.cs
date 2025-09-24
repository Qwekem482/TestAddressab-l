using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;

public class TestNavMeshSetup : EditorWindow
{
    [MenuItem("Tools/Create Test NavMeshObstacles")]
    public static void CreateTestObjects()
    {
        // Create a few test objects with NavMeshObstacle components
        for (int i = 0; i < 5; i++)
        {
            GameObject testObj = new GameObject($"TestObstacle_{i}");
            testObj.transform.position = new Vector3(i * 2f, 0, 0);
            
            // Add NavMeshObstacle component
            NavMeshObstacle obstacle = testObj.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.size = new Vector3(1f, 1f, 1f);
            
            // Register for undo
            Undo.RegisterCreatedObjectUndo(testObj, "Create Test NavMesh Obstacle");
        }
        
        Debug.Log("Created 5 test objects with NavMeshObstacle components");
        EditorUtility.DisplayDialog("Test Setup", "Created 5 test objects with NavMeshObstacle components", "OK");
    }
    
    [MenuItem("Tools/Clean Test NavMeshObstacles")]
    public static void CleanTestObjects()
    {
        GameObject[] testObjects = GameObject.FindGameObjectsWithTag("Untagged");
        int cleanedCount = 0;
        
        foreach (GameObject obj in testObjects)
        {
            if (obj.name.StartsWith("TestObstacle_"))
            {
                Undo.DestroyObjectImmediate(obj);
                cleanedCount++;
            }
        }
        
        Debug.Log($"Cleaned up {cleanedCount} test objects");
        EditorUtility.DisplayDialog("Test Cleanup", $"Cleaned up {cleanedCount} test objects", "OK");
    }
}
#endif