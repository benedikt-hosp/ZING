# ZING

> ZING was formerly called "VR Player". Older material, file names and the original repository name (`VR-Player-SMI_V1`) still use that name.

Unity projects for presenting 360-degree video and image stimuli in a VR headset while recording eye-tracking data. Experiments (stimulus order, 2D/3D, resolution) are described in `*.experiment` files that the player reads. This is the research code behind **ZING** (ICMI 2022).

- **Paper:** ZING, International Conference on Multimodal Interaction (ICMI) 2022. No DOI is given in this repository.
- **Status:** research code, not maintained. Issues and pull requests may not be answered.
- **Unity versions:** `VR-ET-Player/` 2018.2.10f1, `VR_Player_ET_V3/VR-Player_ET_V3/` 2018.3.9f1. Both are old projects (2018/2019) and may need migration in a current Unity.
- **License:** MIT, see [LICENSE](LICENSE). The license covers the code in this repository only, **not** the third-party SDKs and plugins listed below.

## What is in this repository

- `VR-ET-Player/`: first player version with SMI eye tracking (`Assets/Code/`, `Load3DVideos.cs`, `InputServiceScript.cs`, scenes).
- `VR_Player_ET_V3/VR-Player_ET_V3/`: second version (`EyeTracker.cs`, `Load3DVideos.cs`, 360 player scene, render textures and skyboxes for 2D/3D 4K playback).

No stimuli and no recorded data are included.

## Third-party SDKs

The following components are **not** included. Download them yourself and copy them into the `Assets/` folder of each Unity project:

| Component | What to download | Where to put it |
|---|---|---|
| SMI Eye Tracking Unity SDK (`SMI.SMIEyeTrackingUnity`, HTC Vive integration) | From SMI (SensoMotoric Instruments). The company no longer exists, so use your own licensed copy of the SDK. | `Assets/Plugins/SMIEyeTrackingUnity/` and the SDK's `SMI_CameraWithEyeTracking` prefab (`Assets/Resources/`) |
| SimpleFileBrowser | SimpleFileBrowser Unity plugin | `Assets/Plugins/SimpleFileBrowser/` |
| HTC Vive Input Utility (VIU) | HTC Vive Input Utility for Unity | `Assets/HTC.UnityPlugin/` |
| SteamVR Unity Plugin | <https://github.com/ValveSoftware/steamvr_unity_plugin> | `Assets/SteamVR/` |
| TextMesh Pro essentials | In Unity: Window > TextMeshPro > Import TMP Essential Resources | `Assets/TextMesh Pro/` |
| Stimuli | Your own 360 videos / images. Do not use material you are not allowed to redistribute. | `Assets/stimuli/` (as referenced in your `*.experiment` file) |

The folders above are listed in `.gitignore`.

## Experiment files

The player reads `*.experiment` files from an `Experiment/` folder next to the application. Each line describes one stimulus (type, 2D/3D, resolution, file path, ...). The files were created with a separate Qt tool (VR Experiment Designer). Paths inside experiment files are absolute and machine specific. Adapt them to your machine.
