using Microsoft.Xna.Framework;
using FezEngine.Services;
using FezEngine.Tools;
using IMGUI.Patches;
using ImGuiNET;

namespace IMGUI;

public class IMGUI : DrawableGameComponent
{
    [ServiceDependency] public ITargetRenderingManager TargetRenderer { private get; set; }

    private ImGuiRenderer _imGuiRenderer;

    public IMGUI(Game game) : base(game)
    {
        Enabled = true;
        DrawOrder = int.MaxValue;
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
            RasterizerCombinerPatch.Apply();
            _imGuiRenderer = ImGuiRenderer.Create(Game);
            TargetRenderer.PreDraw += PreDraw;
        });
    }

    private void PreDraw(GameTime gameTime)
    {
        _imGuiRenderer?.BeforeLayout(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        #region DEBUG
        if (_imGuiRenderer != null)
        {
            ImGuiX.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
            ImGui.ShowAboutWindow();
            ImGuiX.SetNextWindowSize(new Vector2(30, 30), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow();
        }
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