
# Mission Planner Water Sampler Plugin
===

# 

# This plugin adds a custom Water Sampler Dashboard to Mission Planner, including integrated video coverage powered by Skydroid and LibVLCSharp.

# 

# \## Installation

# 

# To use this plugin in your existing Mission Planner installation, follow these steps:

# 

# \### 1. Add the Plugin

# Copy `WaterSamplerDashboardPlugin.dll` from the `/bin` folder of this repository into your Mission Planner plugins directory:

# `C:\\Program Files (x86)\\Mission Planner\\plugins\\`

# 

# \### 2. Add Video Dependencies (LibVLCSharp)

# For the video feed to work, you must copy the following files from the `/lib` folder into the \*\*root\*\* Mission Planner directory (where `MissionPlanner.exe` is located):

# \* `LibVLCSharp.dll`

# \* `LibVLCSharp.WinForms.dll`

# \* \*Note: Ensure you have VLC player installed on your system as these libraries depend on the VLC cores.\*

# 

# \## 🛠 Features

# \* \*\*Custom Dashboard:\*\* Real-time interface for water sampling operations.

# \* \*\*Video Integration:\*\* High-quality video feed integration for Skydroid hardware.

# \* \*\*Custom UI:\*\* Specialized control panel for sampler triggering and status monitoring.

# 

# \## Development

# If you want to modify the code:

# 1\. Open `WaterSamplerDashboardPlugin.sln` in Visual Studio.

# 2\. Ensure you have the Mission Planner references updated to point to your local Mission Planner installation folder.

# 3\. Build the solution to generate a new `.dll`.

# 

# \## License

# This project is licensed under the MIT License - see the LICENSE file for details.

