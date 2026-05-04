using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;   // Cần thiết để kiểm tra/tạo thư mục
#endif

public class GameEditorUI : MonoBehaviour
{
    public static GameEditorUI Instance { get; private set; }

    [SerializeField] private LevelManagerSO levelManagerSO;

    [Header("Button")]
    [SerializeField] private Button addNewLevelButton;

    private void Awake() {

        Instance = this;

        // Button
        addNewLevelButton.onClick.AddListener(() => {

            CreateNewLevelAsset();
        });
    }

    private void CreateNewLevelAsset() {
#if UNITY_EDITOR
        // 1. Định nghĩa đường dẫn thư mục
        string folderPath = "Assets/ScriptableObject/Level";

        // 2. Kiểm tra nếu thư mục chưa tồn tại thì tạo mới
        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // 3. Tạo instance mới của LevelSO trong bộ nhớ
        LevelSO newLevel = ScriptableObject.CreateInstance<LevelSO>();

        // 4. Tạo tên file không trùng lặp (ví dụ: Level_1, Level_2...)
        int levelCount = levelManagerSO.levelSOList.Count + 1;
        string assetPath = $"{folderPath}/Level_{levelCount}.asset";

        // Đảm bảo không ghi đè nếu file đã tồn tại tên đó
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        // 5. Lưu xuống ổ cứng thành file .asset
        AssetDatabase.CreateAsset(newLevel, assetPath);
        AssetDatabase.SaveAssets();

        // 6. Đưa vào danh sách quản lý của LevelManagerSO
        levelManagerSO.levelSOList.Add(newLevel);
        EditorUtility.SetDirty(levelManagerSO); // Đánh dấu để Unity lưu lại thay đổi trong Manager

        // 7. Refresh lại Editor để file hiện lên ngay lập tức
        AssetDatabase.Refresh();

#endif
    }
}
