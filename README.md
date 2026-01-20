# Mission Planner Water Sampler Plugin

A custom **Mission Planner plugin** that adds a dedicated **Water Sampler Dashboard** with **integrated live video streaming** powered by **Skydroid** and **LibVLCSharp**. This plugin is designed for real-time monitoring and control during water sampling missions.

---

## ✨ Features

* **Custom Water Sampler Dashboard**
  Real-time UI tailored for water sampling operations.

* **Live Video Integration**
  High-quality video feed using **Skydroid hardware** and **LibVLCSharp**.

* **Custom Control Panel**
  Dedicated controls for sampler triggering and status monitoring.

* **Seamless Mission Planner Integration**
  Works as a standard Mission Planner plugin.

---

## 🧩 Installation

### 1️⃣ Add the Plugin DLL

Copy the plugin DLL from the build output to the Mission Planner plugins directory:

```
C:\Program Files (x86)\Mission Planner\plugins\
```

**File to copy:**

* `WaterSamplerDashboardPlugin.dll`

---

### 2️⃣ Add Video Dependencies (LibVLCSharp)

To enable the video feed, copy the following files from the `/lib` folder into the **Mission Planner root directory** (where `MissionPlanner.exe` is located):

* `LibVLCSharp.dll`
* `LibVLCSharp.WinForms.dll`

⚠️ **Important:**

* Ensure **VLC Media Player** is installed on your system.
* LibVLCSharp depends on the VLC core libraries to function correctly.

---

## 🛠 Development Setup

If you want to modify or extend the plugin:

1. Open `WaterSamplerDashboardPlugin.sln` in **Visual Studio**.
2. Update **Mission Planner references** to point to your local Mission Planner installation directory.
3. Build the solution to generate a new `.dll` file.
4. Copy the generated DLL to the Mission Planner `plugins` folder.

---

## 📦 Project Structure

```
├── WaterSamplerDashboardPlugin.sln
├── src/
│   └── Plugin source code
├── bin/
│   └── WaterSamplerDashboardPlugin.dll
├── lib/
│   ├── LibVLCSharp.dll
│   └── LibVLCSharp.WinForms.dll
├── LICENSE
└── README.md
```

---

## 📜 License

This project is licensed under the **MIT License**.
See the `LICENSE` file for more details.

---

## 🤝 Contributions

Contributions, issues, and feature requests are welcome!
Feel free to fork this repository and submit a pull request.

---

## 📬 Support

If you face issues with Mission Planner integration or video streaming, please:

* Verify DLL placement
* Confirm VLC installation
* Check Mission Planner plugin logs

Happy flying 🚁💧
