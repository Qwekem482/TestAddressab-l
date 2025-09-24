using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// This script demonstrates the fix for the NvMObsReplacer tool issue
/// </summary>
public class FixDemonstration : MonoBehaviour
{
    [Header("Before Fix - Issues")]
    [TextArea(5, 10)]
    public string beforeFixIssues = 
        "ISSUES TRƯỚC KHI SỬA:\n" +
        "1. Process(false, false) không hoạt động đúng\n" +
        "2. Tool chỉ xử lý 1 object thay vì tất cả objects\n" +
        "3. Logic remove/replace không đúng với tham số\n" +
        "4. Thiếu logging chi tiết";

    [Header("After Fix - Solutions")]
    [TextArea(5, 10)]
    public string afterFixSolutions =
        "GIẢI PHÁP ĐÃ ÁP DỤNG:\n" +
        "1. Process(false, false) giờ chỉ scan thông tin, không thay đổi\n" +
        "2. Sử dụng FindObjectsOfType() để xử lý TẤT CẢ objects\n" +
        "3. Logic rõ ràng cho từng combination tham số\n" +
        "4. Logging chi tiết với timestamp vào file .txt\n" +
        "5. Hỗ trợ Undo cho tất cả operations";

    [Header("Test Instructions")]
    [TextArea(3, 10)]
    public string testInstructions =
        "CÁCH TEST:\n" +
        "1. Tools > Create Test NavMeshObstacles (tạo 5 test objects)\n" +
        "2. Tools > NavMeshObstacle Replacer > Process (false, false)\n" +
        "3. Kiểm tra log file được tạo\n" +
        "4. Thử các combinations khác để test remove/replace\n" +
        "5. Tools > Clean Test NavMeshObstacles (dọn dẹp)";

    // Example of the old problematic behavior (commented out)
    /*
    // OLD BROKEN CODE EXAMPLE:
    public static void OldBrokenProcess(bool shouldRemove, bool shouldReplace)
    {
        // BUG 1: Only gets first NavMeshObstacle, not all
        NavMeshObstacle obstacle = FindObjectOfType<NavMeshObstacle>();
        
        if (obstacle != null)
        {
            // BUG 2: Always tries to do something even when both params are false
            if (shouldRemove)
            {
                DestroyImmediate(obstacle);
            }
            // BUG 3: Replace logic doesn't consider shouldRemove parameter properly
            if (shouldReplace)
            {
                // Would try to replace on already destroyed object
                obstacle.gameObject.AddComponent<Collider>();
            }
        }
        
        // BUG 4: No logging, no feedback to user
    }
    */

    // Show the fixed approach
    private void ShowFixExplanation()
    {
        Debug.Log("=== NvMObsReplacer Fix Explanation ===");
        Debug.Log("OLD: FindObjectOfType<NavMeshObstacle>() - only gets first match");
        Debug.Log("NEW: FindObjectsOfType<NavMeshObstacle>() - gets ALL matches");
        Debug.Log("");
        Debug.Log("OLD: No special handling for Process(false, false)");
        Debug.Log("NEW: Process(false, false) = info scan only, no modifications");
        Debug.Log("");
        Debug.Log("OLD: Poor parameter logic and error handling");
        Debug.Log("NEW: Clear logic for each parameter combination");
        Debug.Log("");
        Debug.Log("OLD: No logging or user feedback");
        Debug.Log("NEW: Comprehensive logging to timestamped files");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FixDemonstration))]
    public class FixDemonstrationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            FixDemonstration demo = (FixDemonstration)target;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Show Fix Explanation in Console"))
            {
                demo.ShowFixExplanation();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Open NavMeshObstacle Replacer Tool"))
            {
                NvMObsReplacer.ShowWindow();
            }
        }
    }
#endif
}
#endif