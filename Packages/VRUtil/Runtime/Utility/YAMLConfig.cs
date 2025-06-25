using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using System;
using System.Linq;


[HasCommandConsoleCommands]
public abstract class YAMLConfig
{
    public event Action ConfigSaved;

	[CommandConsoleCommand("show_config_folder", "Show the folder config files are saved in")]
	public static void CCShowConfigFolder()
	{
		CommandConsole.Print(GetConfigFolder());
	}

	public static string GetConfigFolder()
	{
        
		string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
		path = Path.Combine(path, "NIOSH", "VRMine", "Config");

		// string path = Application.dataPath;		
		// DirectoryInfo dir = Directory.GetParent(path);

		/*path = Application.persistentDataPath;
		path = Path.Combine(path, "Config"); */

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		} 

		return path;
	}

	public static T LoadConfig<T>(string filename) where T : YAMLConfig, new()
	{
		T config = null;
		string configFilename = Path.Combine(GetConfigFolder(), filename);
		try
		{			
			if (File.Exists(configFilename))
			{
				var yaml = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

				using (StreamReader reader = new StreamReader(configFilename, Encoding.UTF8))
				{
					config = yaml.Deserialize<T>(reader);
				}

				config._sourceFile = configFilename;
			}
			else
			{
				config = new T();
				config._sourceFile = configFilename;
				config.SaveConfig();
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log(ex.Message);
		}

		if (config == null)
		{
			//failed to load config....
			Debug.LogError($"Failed loading config file {configFilename}!");
		}

		return config;
	}

	public YAMLConfig()
	{
		LoadDefaults();
	}

	private string _sourceFile;

	public abstract void LoadDefaults();

	public void SaveConfig()
	{
		string configFilename = _sourceFile;

		var yaml = new SerializerBuilder()
			.WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
			.WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .DisableAliases()
			.Build();

		//var yaml = new SerializerBuilder().EmitDefaults().Build();
		using (StreamWriter writer = new StreamWriter(configFilename, false, Encoding.UTF8))
		{
			yaml.Serialize(writer, this);
		}

        try
        {
            ConfigSaved?.Invoke();
        }
        catch (Exception) { }
	}
}
