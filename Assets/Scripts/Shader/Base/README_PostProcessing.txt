= GENERAL SYSTEM/WORKFLOW =

- The Scriptable Render Feature (SRF) and Scriptable Render Pass (SRP) have been modified to use the CustomPostProcessing class as input
- You do not need to modify the SRF or SRP

- The CustomPostProcessing abstract class has a Shader and an abstract method for sending data to the shader

- To add a new effect, simply create a new ScriptableObject class deriving from CustomPostProcessing (ensure you use CreateAssetMenu())

- Create a new URP shader, ensure it includes "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl" to blit to the camera (you may have some difficulty with the URP syntax)

- Ensure that URP shader has a texture with the name "_CameraTexture", that will be the camera's color texture

- Add that shader as a parameter in your derived Post Processing class

- Modify the abstract SendDataToShader(Material mat) method to send any necessary data to the shader

- Pass that shader into the list of Post Processing effects in the SRF

- The SRP will automatically be generated in the CustomPostProcessing class, you do not need to worry about that

- If your code is correct, it should work