using UnityEditor;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// Static methods to programmatically test the NavMesh Obstacle Replacer
    /// This simulates the Process(false, false) call mentioned in the issue
    /// </summary>
    public static class NavMeshObstacleReplacerCLI
    {
        [MenuItem("Tools/NavMesh Obstacle Replacer/Process (Preview=false, IncludeScene=false)")]
        public static void ProcessNonPreviewNoScene()
        {
            var window = EditorWindow.GetWindow<NavMeshObstacleReplacer>();
            window.Process(false, false);
        }

        [MenuItem("Tools/NavMesh Obstacle Replacer/Process (Preview=false, IncludeScene=true)")]
        public static void ProcessNonPreviewWithScene()
        {
            var window = EditorWindow.GetWindow<NavMeshObstacleReplacer>();
            window.Process(false, true);
        }

        [MenuItem("Tools/NavMesh Obstacle Replacer/Process (Preview=true, IncludeScene=false)")]
        public static void ProcessPreviewNoScene()
        {
            var window = EditorWindow.GetWindow<NavMeshObstacleReplacer>();
            window.Process(true, false);
        }

        [MenuItem("Tools/NavMesh Obstacle Replacer/Process (Preview=true, IncludeScene=true)")]
        public static void ProcessPreviewWithScene()
        {
            var window = EditorWindow.GetWindow<NavMeshObstacleReplacer>();
            window.Process(true, true);
        }

        /// <summary>
        /// This is the exact method call mentioned in the issue: Process(false, false)
        /// </summary>
        public static void RunProblemScenario()
        {
            Debug.Log("Running the exact scenario from the issue: Process(false, false)");
            var window = EditorWindow.GetWindow<NavMeshObstacleReplacer>();
            window.Process(false, false);
        }
    }
}