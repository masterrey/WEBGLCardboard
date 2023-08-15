Cardboard.NET
=============

This asset is the port of the Cardboard SDK in C # for use in Unity Engine.  
It does not depend on third-party libraries, including the Google SDK.  
This asset doesn't use deprecated UIWebView, which helps you publish your apps in AppStore without restrictions.

To implement lens rendering, the built-in capabilities for displaying polygonal grids are used.

This asset support recognize QR codes from Google Cardboard and use his to generate correct lens meshes.

You can interact with a controll (like a button) througt a visual cursor.
Support for various control devices and manipulators is not provided.

If you have problem with adopting google cardboard SDK, this is pretty good solution for you, because:

- Implemented in pure C#
- Has no dependencies
- Works on iOS, Android and any other platform
- Actual preview in editor with lens distortion
- Scaning Google QR-codes without cardboard application
- Works without cardboard application
- Open-source implementation
- Fully customizable and hackable
- Compatible with il2cpp

If you have issues, please don't hesitate to email contact@tiltshift.xyz

Quick Guide
=============

  1) Open the Demo example scene
  2) Press play and see how it work. 
  3) Select the CardboardController object in the Hierarchy to change smoothing.
  
  To move the camera in the editor, hold ALT and move the mouse. 
  You can change the type of click on interactive elements in the CardboardInputProvider settings (bool GazeClicking).
  
  4) To use in your own project, use CardboardController prefab.