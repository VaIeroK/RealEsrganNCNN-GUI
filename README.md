# RealEsrgan NCNN GUI

A simple, user-friendly Windows GUI for batch upscaling images and videos using [realesrgan-ncnn-vulkan](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan) with [ffmpeg](https://www.ffmpeg.org/download.html) for video unpack/pack.

<img width="859" height="931" alt="image" src="https://github.com/user-attachments/assets/452472ea-4240-4db6-b56d-850cb5798c30" />

## What it does
- Upscales single images or entire folders of images using Real-ESRGAN (NCNN Vulkan).
- Automatically unpacks videos into frames, runs ESRGAN on frames, then repacks frames back into a video.
- Resume support: if processing is interrupted you can continue where it stopped — already processed frames are skipped.
- Disk‑space checks before heavy operations to avoid running out of disk.
- Manual and automatic cleanup of temporary folders (`<video>_input` and `<video>_output`).
- Shows process logs in the app window.
- Adds optional context-menu entries in Explorer for quick file sending.
- Single-instance: subsequent launches forward files to the running instance.

## Quick start
1. Place `realesrgan-ncnn-vulkan.exe` and `ffmpeg.exe` next to the application executable — or create `.lnk` shortcuts to them in the application folder.
2. Put your model files in a `models` folder next to the app.
3. Run the application.

## Basic usage (GUI)
1. Choose `File` or `Folder`.
2. Click `Browse` and add files or a directory.
3. Select model, scale and optionally `TTA` or tile size.
4. Click `Run`. Logs and progress appear in the console area.

## Video processing workflow (user view)
- Selecting a video creates two temporary folders next to the source:
  - `<name>_input` — unpacked frames (PNG).
  - `<name>_output` — upscaled frames from ESRGAN.
- After ESRGAN finishes, frames in `<name>_output` are packed into `<name>_upscaled.<ext>` using `ffmpeg`.
- On successful packing, temporary folders are removed automatically.
- If you press `Stop` during processing, the final video is not created; temporary folders remain for resume or manual cleanup.

## Context menu integration (Explorer)
- You can add or remove a "RealEsrgan Upscale" entry in Windows Explorer directly from the application UI: open the app menu Tools → Context menu and choose "Add to context menu" or "Remove from context menu".  
- The action registers the app for supported file types (images and videos) so you can send files to the app from Explorer. Administrative privileges may be required to modify the system context menu.

## Resume behavior
- If `<name>_input` already exists, the app skips unpacking.
- If `<name>_output` contains frames, those frame names are removed from `<name>_input` before continuing — only remaining frames are processed.
- This allows safe interruption and continuation of long jobs.

## Cleaning temporary folders
- Use `Clear Video Folders` button to remove `_input` and `_output` folders for selected videos.
- The app will also try to remove temporary folders automatically after a successful packing; if deletion fails you'll be notified and can clean manually.

## Tips
- If ESRGAN or ffmpeg are not found, place their executables next to the app or create shortcuts (`.lnk`) in the same folder — the app resolves those.
- For very large videos, ensure significantly more free disk space than the video size (unpacking frames can require multiple times the source file size).
- Reduce tile size if GPU memory is insufficient.

## Useful notes
- Temporary folders use the naming convention `<original-name>_input` and `<original-name>_output` and sit in the same folder as the source video.
- Output images and videos is named `<original-name>_upscaled.<ext>` and saved next to the original.

## Troubleshooting
- "Not enough disk space" — free space on the drive containing the temp folders or change output location.
- ESRGAN process doesn’t start — check `realesrgan-ncnn-vulkan.exe` presence and `models` folder.
- ffmpeg errors — check logs displayed in application for details.

Enjoy upscaling — efficient, resumable, and safe for large batches and videos.
