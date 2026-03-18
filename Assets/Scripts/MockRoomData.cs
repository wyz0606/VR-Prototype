using System.Collections.Generic;
using UnityEngine;

// Using structs for the data payload keeps memory overhead lightweight 
// and avoids unnecessary heap allocations during the serialization process.
[System.Serializable]
public struct MockTransformData {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[System.Serializable]
public struct MockSceneEntity {
    public string entityName;
    public string semanticClassification; // e.g., "WALL_FACE", "FLOOR", "DESK"
    public MockTransformData transform;
}

[System.Serializable]
public class MockRoomLayout {
    public string layoutName;
    public List<MockSceneEntity> entities = new List<MockSceneEntity>();
}