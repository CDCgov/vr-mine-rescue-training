using Unity.AI.Navigation;
using UnityEngine;

public class ComponentInfo_NavMesh : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public string ComponentInspectorTitle => "Nav Mesh";

    [InspectableBoolProperty("Generate Nav Mesh", tooltip:"Generate a nav mesh for this object for NPCs to move on")]
    public bool GenerateNavMesh { get; set; }

    private NavMeshModifier _navMeshModifier;

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        GenerateNavMesh = component.GetParamValueBool("GenerateNavMesh", false);

        if (TryGetComponent<NavMeshModifier>(out _navMeshModifier))
        {
            _navMeshModifier.ignoreFromBuild = !GenerateNavMesh;
        }
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "GenerateNavMesh|" + GenerateNavMesh,
        };
    }

    public string SaveName()
    {
        return "CompInfoNavMesh";
    }

    void Awake()
    {
        GenerateNavMesh = false;
    }

}
