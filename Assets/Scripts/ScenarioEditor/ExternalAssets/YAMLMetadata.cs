using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using YamlDotNet.Serialization;
using System;

public static class YAMLMetadata
{

    private static ISerializer _serializer;
    private static IDeserializer _deserializer;

    private static ISerializer BuildSerializer()
    {
        var serializer = new SerializerBuilder()
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .DisableAliases()
            .Build();

        return serializer;
    }

    private static IDeserializer BuildDeserializer()
    {
        return new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    }

    public static void Save(string filename, object data)
    {
        if (_serializer == null)
            _serializer = BuildSerializer();

        using (StreamWriter streamWriter = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
        {
            _serializer.Serialize(streamWriter, data);
        }
    }

    public static T Load<T>(string filename) where T : class
    {
        T result = null;

        if (_deserializer == null)
            _deserializer = BuildDeserializer();

        using (StreamReader reader = new StreamReader(filename, Encoding.UTF8))
        {
            result = _deserializer.Deserialize<T>(reader);
        }

        return result;
    }
}
