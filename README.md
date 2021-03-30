# VRInvite
The VR Bystander Integration Library

Built on Unity version 2019.3.7f1

Client Requirements: 
- Multiplayer HLAPI, XR Legacy Input Helpers Packages

Smartphone Build Settings:
- Build-Plattform: Android
- Player Settings:
	- Default Orientation: Landscape right / left
	- Graphics APIs: remove Vulkan
	- Multithreaded Rendering: disabled
	- Minimum API Level: Android 7.0 Nougat API Level 24
	- ARCore Supported: enabled

- Enter the Server’s IP address in the NetworkControl script, attached to the NetworkManager object in the scene. 
- Both Smartphone and Server have to be in the same LAN network


Server Build Settings:
- Player Settings: Color Space set to Gamma


Notes:
- We used the Example Database for QR code reading. This can be exchanged in the settings of the AugmentedImagesSessionConfig. 
- The Smartphone 3D model was made by Vertex Studio: https://assetstore.unity.com/packages/3d/props/electronics/free-smartphone-90324 












