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




public class TrialBlock
{
    public string BlockName { get; set; }
    public Dictionary<string, Experiment.ExperimentVal> BlockVariables { get; set; }

    public int ComputeNumTrials()
    {
        if (BlockVariables == null)
            return 0;
        
        int numTrials = 1;

        foreach (var kvp in BlockVariables)
        {
            numTrials *= kvp.Value.NumValues;
        }

        return numTrials;
    }
}

public class BlockVariable
{
    public string Name {get; set;}
    public Experiment.ExperimentVal Value { get; set; }

    public BlockVariable(string name, Experiment.ExperimentVal val)
    {
        Name = name;
        Value = val;
    }
}

public class ExperimentConfig
{

    public string ExperimentType { get; set; }

    public Dictionary<string, Experiment.ExperimentVal> StaticValues { get; set; }
    public List<TrialBlock> TrialBlocks { get; set; }

    public static ExperimentConfig LoadConfigFile(TextReader reader)
    {
        var yaml = BuildDeserializer();
        return yaml.Deserialize<ExperimentConfig>(reader);
    }

    public static ExperimentConfig LoadConfigFile(string path)
    {
        using (var reader = new StreamReader(path))
        {
            return LoadConfigFile(reader);
        }
    }

    public ExperimentConfig()
    {

    }

    public static IDeserializer BuildDeserializer()
    {
        var yaml = new DeserializerBuilder()
            .WithTagMapping("!double", typeof(Experiment.SingleVal<double>))
            .WithTagMapping("!float", typeof(Experiment.SingleVal<float>))
            .WithTagMapping("!string", typeof(Experiment.SingleVal<string>))
            .WithTagMapping("!int", typeof(Experiment.SingleVal<int>))
            .WithTagMapping("!vec3", typeof(Experiment.SingleVal<Vector3>))
            .WithTagMapping("!range", typeof(Experiment.FloatRange))
            .WithTagMapping("!doublerange", typeof(Experiment.DoubleRange))
            .WithTagMapping("!intrange", typeof(Experiment.IntRange))
            .WithTagMapping("!set", typeof(Experiment.ExpValSet<string>))
            .WithTagMapping("!floatset", typeof(Experiment.ExpValSet<float>))
            .WithTagMapping("!intset", typeof(Experiment.ExpValSet<int>))
            .WithTagMapping("!doubleset", typeof(Experiment.ExpValSet<double>))
            .WithTypeConverter(new YAMLConverter())
            .Build();

        return yaml;
    }

    public static ISerializer BuildSerializer()
    {
        var yaml = new SerializerBuilder()
        .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
        .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
        .Build();

        return yaml;
    }

    public int ComputeNumTrials()
    {
        if (TrialBlocks == null)
            return 0;

        int numTrials = 0;

        foreach (var block in TrialBlocks)
        {
            numTrials += block.ComputeNumTrials();
        }

        return numTrials;
    }

    public class YAMLConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            if (type == typeof(Experiment.SingleVal<Vector3>) ||
                type == typeof(Experiment.SingleVal<double>) ||
                type == typeof(Experiment.SingleVal<float>) ||
                type == typeof(Experiment.SingleVal<int>) ||
                type == typeof(Experiment.SingleVal<string>) ||
                type == typeof(Experiment.ExpValSet<string>) ||
                type == typeof(Experiment.ExpValSet<float>) ||
                type == typeof(Experiment.ExpValSet<double>) ||
                type == typeof(Experiment.ExpValSet<int>)
            )
                return true;
            else
                return false;
        }

        public object ReadYaml(IParser parser, Type type)
        {
            if (type == typeof(Experiment.SingleVal<Vector3>))
                return ParseExpValVec3(parser);
            else if (type == typeof(Experiment.SingleVal<string>))
                return ParseExpValString(parser);
            else if (type == typeof(Experiment.SingleVal<double>))
                return ParseExpValDouble(parser);
            else if (type == typeof(Experiment.SingleVal<float>))
                return ParseExpValFloat(parser);
            else if (type == typeof(Experiment.SingleVal<int>))
                return ParseExpValInt(parser);
            else if (type == typeof(Experiment.ExpValSet<string>))
                return ParseSet<string>(parser);
            else if (type == typeof(Experiment.ExpValSet<double>))
                return ParseSet<double>(parser);
            else if (type == typeof(Experiment.ExpValSet<float>))
                return ParseSet<float>(parser);
            else if (type == typeof(Experiment.ExpValSet<int>))
                return ParseSet<int>(parser);
            else
            {
                parser.SkipThisAndNestedEvents();
                return null;
            }
        }


        private Experiment.SingleVal<Vector3> ParseExpValVec3(IParser parser)
        {
            Vector3 v = new Vector3();

            if (parser.Allow<SequenceStart>() != null)
            {
                var x = parser.Expect<Scalar>();
                var y = parser.Expect<Scalar>();
                var z = parser.Expect<Scalar>();
                parser.Expect<SequenceEnd>();

                v.x = float.Parse(x.Value);
                v.y = float.Parse(y.Value);
                v.z = float.Parse(z.Value);


                // while (parser.Allow<SequenceEnd>() == null)
                // {
                // 	Debug.Log(parser.Current);
                // 	parser.MoveNext();
                // }
            }
            else
            {
                parser.Expect<MappingStart>();

                while (parser.Allow<MappingEnd>() == null)
                {
                    var key = parser.Expect<Scalar>();
                    var val = parser.Expect<Scalar>();

                    float fval;
                    if (!float.TryParse(val.Value, out fval))
                    {
                        throw new Exception("YAML Parser error in Vector3 value");
                    }

                    switch (key.Value.ToLower())
                    {
                        case "x":
                            v.x = fval;
                            break;

                        case "y":
                            v.y = fval;
                            break;

                        case "z":
                            v.z = fval;
                            break;
                    }
                }
            }

            return new Experiment.SingleVal<Vector3>(v);
        }

        private Experiment.SingleVal<String> ParseExpValString(IParser parser)
        {
            var val = parser.Expect<Scalar>();
            return new Experiment.SingleVal<String>(val.Value);
        }

        private Experiment.SingleVal<double> ParseExpValDouble(IParser parser)
        {
            var val = parser.Expect<Scalar>();
            double num = double.Parse(val.Value);
            return new Experiment.SingleVal<double>(num);
        }

        private Experiment.SingleVal<float> ParseExpValFloat(IParser parser)
        {
            var val = parser.Expect<Scalar>();
            float num = float.Parse(val.Value);
            return new Experiment.SingleVal<float>(num);
        }

        private Experiment.SingleVal<int> ParseExpValInt(IParser parser)
        {
            var val = parser.Expect<Scalar>();
            int num = int.Parse(val.Value);
            return new Experiment.SingleVal<int>(num);
        }

        private Experiment.ExpValSet<T> ParseSet<T>(IParser parser)
        {
            Experiment.ExpValSet<T> set = new Experiment.ExpValSet<T>();
            set.Values = new List<T>();

            parser.Expect<SequenceStart>();

            while (parser.Allow<SequenceEnd>() == null)
            {
                var val = parser.Expect<Scalar>();
                
                var converted = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(val.Value);
                set.Values.Add((T)converted);
            }

            return set;
        }

        // private Experiment.StringSet ParseStringSet(IParser parser)
        // {
        // 	Experiment.StringSet set = new Experiment.StringSet();
        // 	set.Values = new List<string>();

        // 	parser.Expect<SequenceStart>();

        // 	while (parser.Allow<SequenceEnd>() == null)
        // 	{
        // 		var val = parser.Expect<Scalar>();
        // 		set.Values.Add(val.Value);
        // 	}

        // 	return set;
        // }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
        }
    }
}