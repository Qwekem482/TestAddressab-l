using UnityEngine;
using UnityEngine.AI;

public class TestData : MonoBehaviour
{
    public string testName = "Test Data Object";
    
    void Start()
    {
        Debug.Log($"TestData object '{testName}' started");
    }
    
    // This script is referenced in the Addressables linker.xml
    // It's a simple test script that can have NavMesh obstacles added to it
}