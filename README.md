# Flower Sort 3D

Game mobile puzzle 3D offline thể loại Sorting, phát triển bằng Unity (C#) cho nền tảng Android.

## Giới thiệu

Người chơi chạm vào các hộp rỗng để thu thập hoa cùng màu từ lưới hoa phía trên. Hộp đầy được đưa lên xe đẩy; xe đủ 3 hộp sẽ rời đi và xe mới xuất hiện. Game gồm **50 level** với độ khó tăng dần.

## Cách chơi

1. Quan sát lưới hoa và xe đẩy hiện tại.
2. Chạm vào một hộp: nếu cùng màu với xe đang chờ, hộp lên xe; nếu không, hộp vào khay chờ (5 ô).
3. Hoa ở hàng dưới cùng, cùng màu với hộp, tự động bay vào hộp. Hộp đầy sẽ đóng nắp.
4. Xe đủ 3 hộp thì rời đi.

**Thắng:** thu hết hoa trên lưới.
**Thua:** khay chờ đầy và không còn hộp nào có thể di chuyển.

## Hộp đặc biệt

| Loại | Mô tả |
|------|-------|
| Hộp lớn (Large) | Chứa 20 hoa, gấp đôi hộp thường |
| Hộp bí ẩn (Secret) | Ẩn màu cho đến khi ra hàng đầu |
| Hộp sao (Star) | Chỉ lên được ô có biểu tượng sao trên xe |
| Hộp nối dây (Connect) | Hai hộp liên kết với nhau |

## Công nghệ sử dụng

- **Unity 2022.3.62f2**, Universal Render Pipeline (URP)
- **C#**
- [DOTween](http://dotween.demigiant.com/) - animation
- [UniTask](https://github.com/Cysharp/UniTask) - xử lý bất đồng bộ
- [Odin Inspector](https://odininspector.com/) - công cụ chỉnh sửa level trong Editor
- Cinemachine, TextMeshPro, MK Toon, QuickOutline
- ScriptableObject (dữ liệu level), PlayerPrefs (lưu cài đặt, tiến trình)

## Kiến trúc

- **State machine:** `LoadGame` → `MainMenu` → `InGame` (`GameStateManager`)
- **Cấu trúc dữ liệu:** `Stack<Food>` cho cột hoa, `Queue<Box>` cho cột hộp
- **Design pattern:** Singleton, Object Pool, State, Observer
- **Level:** lưu bằng `LevelData` (ScriptableObject) trong `Assets/Resources/Level`

```
Assets/
├── Scripts/
│   ├── CoreGame/      # GameController, LevelManager, Box, Car, Food, Slot
│   ├── GameState/     # State machine
│   ├── GameView/      # UI các màn hình
│   ├── Audio/         # Quản lý âm thanh
│   └── Ulti/          # Tiện ích
├── Resources/Level/   # Dữ liệu 50 level
├── 2D/                # Hình ảnh UI
└── 3D/                # Model, material
```

## Cài đặt và chạy

1. Cài **Unity Hub** và Unity **2022.3.62f2** (kèm module *Android Build Support*).
2. Clone repo:
   ```bash
   git clone https://github.com/dungvd399/doancoso.git
   ```
3. Mở thư mục project bằng Unity Hub (Add → chọn thư mục vừa clone).
4. Mở scene `Assets/Scenes/Gameplay.unity` và bấm **Play**.
5. Để build Android: **File → Build Settings → Android → Build** (Minimum API Level: 26).

## Lưu ý về plugin

Project sử dụng các plugin trả phí **Odin Inspector** và **DOTween Pro**. Nếu repo không kèm các plugin này, hãy tự import từ Unity Asset Store vào `Assets/Plugins/Sirenix` và `Assets/Storepackages/Demigiant`.

## Hạn chế

- Chỉ hỗ trợ chơi offline, chưa có đăng nhập, bảng xếp hạng hay lưu dữ liệu đám mây.
- Tối ưu chủ yếu cho các thiết bị Android phổ biến.

## Hướng phát triển

- Hệ thống mạng chơi (Lives) và mua hàng trong game (IAP)
- Thêm level và vật phẩm hỗ trợ (Boosters)
- Bảng xếp hạng, lưu dữ liệu đám mây
- Hoàn thiện hiệu ứng, âm thanh và tối ưu hiệu năng
