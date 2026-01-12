using Microsoft.Xna.Framework;
using FezEngine.Services;
using FezEngine.Tools;
using IMGUI.Patches;
using ImGuiNET;

namespace IMGUI;

public class IMGUI : DrawableGameComponent
{
    private ImGuiRenderer _imGuiRenderer;

    public IMGUI(Game game) : base(game)
    {
        Enabled = true;
        DrawOrder = int.MaxValue;
        RasterizerCombinerPatch.Apply();
    }

    public override void Initialize()
    {
        base.Initialize();
        if (!ImGuiNativeLoader.TryToLoad())
        {
            Enabled = false;
            return;
        }
        
        DrawActionScheduler.Schedule(() =>
        {
            _imGuiRenderer = ImGuiRenderer.Create(Game);
        });
        
        ServiceHelper.Get<ITargetRenderingManager>().PreDraw += gameTime =>
        {
            _imGuiRenderer.BeforeLayout(gameTime);
        };
    }

    public override void Draw(GameTime gameTime)
    {
#region DEBUG
        ImGuiX.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
        ImGui.ShowAboutWindow();
        ImGuiX.SetNextWindowSize(new Vector2(30, 30), ImGuiCond.FirstUseEver);
        ImGui.ShowDemoWindow();
#endregion
        _imGuiRenderer?.AfterLayout();
    }

    protected override void Dispose(bool disposing)
    {
        RasterizerCombinerPatch.Dispose();
        ImGuiNativeLoader.Unload();
        base.Dispose(disposing);
    }
}