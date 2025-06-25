/// <summary>
/// Interface that handles communication between save/load system and individual components.
/// </summary>
public interface ISaveableComponent
{
    /// <summary>
    /// Returns an array of strings. Each entry into the array should 
    /// follow this format: 
    /// 
    /// ValueName|Value
    /// 
    /// So for example RotationSpeed|20 to store the RotationSpeed value of a component with the value of 20.
    /// </summary>
    /// <returns></returns>
    public string[] SaveInfo();

    public string SaveName();
    public void LoadInfo(SavedComponent component);
}
