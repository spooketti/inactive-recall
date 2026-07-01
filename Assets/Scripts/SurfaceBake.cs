using Godot;
using System.Collections.Generic;

public partial class SurfaceBake : Node3D
{
    [Export] public Viewport captureViewport;
    [Export] public Camera3D camera;

    private List<Vector3> seenPoints = new List<Vector3>();

    private Image depthImage;

    public override void _Process(double delta)
    {
        CaptureFrame();
    }

    private void CaptureFrame()
    {
        var vp = captureViewport;

        var img = vp.GetTexture().GetImage();

        int step = 6; // downsample for performance

        for (int y = 0; y < img.GetHeight(); y += step)
        {
            for (int x = 0; x < img.GetWidth(); x += step)
            {
                Color c = img.GetPixel(x, y);

                //probably useless
                if (c.A < 0.1f)
                    continue;

                Vector3 worldPos = ScreenPointToWorld(x, y, vp);

                seenPoints.Add(worldPos);
            }
        }

    }

    private Vector3 ScreenPointToWorld(int x, int y, Viewport vp)
    {
        var cam = camera;

        float width = vp.GetWindow().Size.X;
        // vp.GetWindow().Size.X
        float height = vp.GetWindow().Size.Y;

        // normalize screen coords
        Vector2 ndc = new Vector2(
            x / width * 2f - 1f,
            1f - y / height * 2f
        );

        Vector3 origin = cam.ProjectRayOrigin(new Vector2(x, y));
        Vector3 dir = cam.ProjectRayNormal(new Vector2(x, y));
        PhysicsRayQueryParameters3D parameters = PhysicsRayQueryParameters3D.Create(origin, dir);
        var space = GetWorld3D().DirectSpaceState;

        var result = space.IntersectRay(
            parameters
        );

        if (result.Count > 0)
        {
            return (Vector3)result["position"];
        }

        return Vector3.Zero;
    }

    public void BakeMesh()
    {
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        var vertices = new Godot.Collections.Array();

        foreach (var p in seenPoints)
        {
            vertices.Add(p);
        }

        arrays[(int)Mesh.ArrayType.Vertex] = vertices;

        ArrayMesh mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Points, arrays);

        MeshInstance3D mi = new MeshInstance3D
        {
            Mesh = mesh
        };

        AddChild(mi);
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("bake"))
        {
           BakeMesh();
        }
    }

}