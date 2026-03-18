using System;
using UnityEngine;
using UnityEngine.UI;
using Meta.XR.MRUtilityKit;
using System.IO;

public class RoomDataManager : MonoBehaviour {
  const string FOLDER_NAME = "RoomData";
  const string DATA_NAME_BASE = "QuestCapturedRoom";
  [SerializeField] Button saveRoomDataBtn;

  private void Awake() {
    saveRoomDataBtn.onClick.AddListener(OnRoomDataBtnClicked);
  }

  private void OnRoomDataBtnClicked() {
#if UNITY_EDITOR
    SaveData();
#endif
  }

  private void SaveData() {
    MRUKRoom room = MRUK.Instance.GetCurrentRoom();
    if (room == null) {
      Debug.LogError("No room found! Make sure MRUK has successfully loaded the physical space.");
      return;
    }

    MockRoomLayout layout = new MockRoomLayout { layoutName = DATA_NAME_BASE };

    // Loop through all physical elements (walls, floors, couches, etc.)
    foreach (MRUKAnchor anchor in room.Anchors) {
      MockSceneEntity entity = new MockSceneEntity();

      // Convert the MRUK semantic label enum (e.g., WALL_FACE) to a string
      entity.entityName = anchor.Label.ToString();
      entity.semanticClassification = anchor.Label.ToString();

      // Store the exact world position and rotation
      entity.transform.position = anchor.transform.position;
      entity.transform.rotation = anchor.transform.rotation;

      // Differentiate between 3D furniture and 2D surfaces
      if (anchor.VolumeBounds.HasValue) {
        // It's a 3D object like a desk. Get the full bounds.
        entity.transform.scale = anchor.VolumeBounds.Value.size;
      }
      else if (anchor.PlaneRect.HasValue) {
        // It's a 2D surface like a wall. 
        // We fake a 0.01f Z-depth so Unity can generate a BoxCollider for it in the Editor.
        entity.transform.scale = new Vector3(anchor.PlaneRect.Value.width, anchor.PlaneRect.Value.height, 0.01f);
      }

      layout.entities.Add(entity);
    }

    // Serialize the struct to JSON
    string json = JsonUtility.ToJson(layout, true);

    // Save to the target directory
    string folderPath = Path.Combine(Application.dataPath, FOLDER_NAME);
    if (!Directory.Exists(folderPath))
      Directory.CreateDirectory(folderPath);
    string path = Path.Combine(folderPath, $"{layout.layoutName}.json");
    File.WriteAllText(path, json);
    UnityEditor.AssetDatabase.Refresh();

    Debug.Log($"[SUCCESS] Saved MRUK Room Data to: {path}");

  }

  public void LoadData() {
    string path = Path.Combine(Application.dataPath, $"{FOLDER_NAME}/{DATA_NAME_BASE}.json");
    if (!File.Exists(path)) {
      Debug.LogError("JSON file not found!");
      return;
    }

    string json = File.ReadAllText(path);
    MockRoomLayout layout = JsonUtility.FromJson<MockRoomLayout>(json);

    // Create a new root for the loaded room
    GameObject newRoomRoot = new GameObject($"LoadedRoom_{layout.layoutName}");

    foreach (var entity in layout.entities) {
      // Here you would instantiate your specific prefabs based on the semantic classification
      GameObject mockObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      mockObj.name = entity.entityName;
      mockObj.transform.position = entity.transform.position;
      mockObj.transform.rotation = entity.transform.rotation;
      mockObj.transform.localScale = entity.transform.scale;
      mockObj.transform.SetParent(newRoomRoot.transform);
    }

    Debug.Log("Successfully loaded MR Layout!");
  }
}
