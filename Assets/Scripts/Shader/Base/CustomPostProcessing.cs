using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Abstract method for defining post processing shaders, such that we can use a template SRP/Feature, and define the behaviour in the abstract method SendDataToShader()
/// </summary>
public abstract class CustomPostProcessing : ScriptableObject
{
    [Header("Shader Configuration")]
    [SerializeField] private Shader shader;
    [SerializeField] ScriptableRenderPassInput input = ScriptableRenderPassInput.Color; // determines the target for our shader (depth, color, etc)

    [Header("Debugging")]
    [SerializeField] private bool toProfile = false;    // creates a profiling scope for the shader if true
    private ProfilingSampler profiler = null;           // if ToProfile is true, will create profiler

    private CustomSRP customPass;                       // reference to the scriptable render pass

    public Shader GetShader() { return shader; }
    public CustomSRP GetPass() { return this.customPass; }
    public ProfilingSampler GetProfiler() { return this.profiler; }
    public bool ToProfile() { return this.toProfile; }
    public ScriptableRenderPassInput GetRenderInput() { return this.input; }

    /// <summary>
    /// Initializes the variables needed for code to actually run properly
    /// </summary>
    public void Init()
    {
        if (shader == null) { Debug.LogError("Shader was null!"); return; }
        if (toProfile) { profiler = new ProfilingSampler(this.name); }
        customPass = new CustomSRP(this);
    }

    /// <summary>
    /// Abstract method for setting relevant shader data
    /// </summary>
    public abstract void SendDataToShader(Material mat);
}
