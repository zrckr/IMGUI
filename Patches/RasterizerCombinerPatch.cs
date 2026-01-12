using System.Runtime.CompilerServices;
using FezEngine.Tools;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;

namespace IMGUI.Patches;

/// <summary>
/// Runtime patch for FezEngine's RasterizerCombiner that enables scissor test control for ImGui clipping rectangles.
/// </summary>
public static class RasterizerCombinerPatch
{
    private static readonly ConditionalWeakTable<object, ScissorTestData> _scissorTestData = new();

    private static Hook _applyHook;
    
    private delegate void orig_Apply(RasterizerCombiner self, GraphicsDevice device);
    
    /// <summary>
    /// Applies the runtime hook to RasterizerCombiner.Apply method to synchronize scissor test state with the graphics device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when RasterizerCombiner type or Apply method cannot be found via reflection.</exception>
    public static void Apply()
    {
        var rasterizerCombinerType = Type.GetType("FezEngine.Tools.RasterizerCombiner, FezEngine")
                                     ?? throw new InvalidOperationException("Could not find FezEngine.Tools.RasterizerCombiner type");
        
        var applyMethod = rasterizerCombinerType.GetMethod("Apply", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                          ?? throw new InvalidOperationException("Could not find Apply method on RasterizerCombiner");
        
        _applyHook = new Hook(applyMethod, (orig_Apply orig, RasterizerCombiner self, GraphicsDevice device) =>
        {
            orig(self, device);
            if (device.RasterizerState != null)
            {
                var scissorTestEnable = self.GetScissorTestEnable();
                device.RasterizerState.ScissorTestEnable = scissorTestEnable;
            }
        });
    }
    
    /// <summary>
    /// Removes the runtime hook and cleans up resources.
    /// </summary>
    public static void Dispose()
    {
        _applyHook?.Dispose();
        _applyHook = null;
    }
    
    /// <summary>
    /// Retrieves the scissor test enable state for a RasterizerCombiner instance.
    /// </summary>
    /// <param name="rasterizerCombiner">The RasterizerCombiner instance to query.</param>
    /// <returns>True if scissor testing is enabled, false otherwise.</returns>
    public static bool GetScissorTestEnable(this RasterizerCombiner rasterizerCombiner)
    {
        return _scissorTestData.TryGetValue(rasterizerCombiner, out var data) && data.ScissorTestEnable;
    }

    /// <summary>
    /// Sets the scissor test enable state for a RasterizerCombiner instance.
    /// </summary>
    /// <param name="rasterizerCombiner">The RasterizerCombiner instance to modify.</param>
    /// <param name="value">True to enable scissor testing, false to disable.</param>
    public static void SetScissorTestEnable(this RasterizerCombiner rasterizerCombiner, bool value)
    {
        var data = _scissorTestData.GetOrCreateValue(rasterizerCombiner);
        data.ScissorTestEnable = value;
    }
    
    /// <summary>
    /// Stores scissor test state for individual RasterizerCombiner instances.
    /// </summary>
    private class ScissorTestData
    {
        /// <summary>
        /// Gets or sets whether scissor testing is enabled.
        /// </summary>
        public bool ScissorTestEnable { get; set; }
    }
}