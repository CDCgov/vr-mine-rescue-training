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

//additional classes for including comments in serialization - see https://github.com/aaubry/YamlDotNet/issues/152 for details

public class CommentGatheringTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector innerTypeDescriptor;

    public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
    {
        if (innerTypeDescriptor == null)
        {
            throw new ArgumentNullException("innerTypeDescriptor");
        }

        this.innerTypeDescriptor = innerTypeDescriptor;
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
    {
        return innerTypeDescriptor
            .GetProperties(type, container)
            .Select(d => new CommentsPropertyDescriptor(d));
    }

    private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor baseDescriptor;

        public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            this.baseDescriptor = baseDescriptor;
            Name = baseDescriptor.Name;
        }

        public string Name { get; set; }

        public Type Type { get { return baseDescriptor.Type; } }

        public Type TypeOverride
        {
            get { return baseDescriptor.TypeOverride; }
            set { baseDescriptor.TypeOverride = value; }
        }

        public int Order { get; set; }

        public ScalarStyle ScalarStyle
        {
            get { return baseDescriptor.ScalarStyle; }
            set { baseDescriptor.ScalarStyle = value; }
        }

        public bool CanWrite { get { return baseDescriptor.CanWrite; } }

        public void Write(object target, object value)
        {
            baseDescriptor.Write(target, value);
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return baseDescriptor.GetCustomAttribute<T>();
        }

        public IObjectDescriptor Read(object target)
        {
            var description = baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
            return description != null
                ? new CommentsObjectDescriptor(baseDescriptor.Read(target), description.Description)
                : baseDescriptor.Read(target);
        }
    }
}

public sealed class CommentsObjectDescriptor : IObjectDescriptor
{
    private readonly IObjectDescriptor innerDescriptor;

    public CommentsObjectDescriptor(IObjectDescriptor innerDescriptor, string comment)
    {
        this.innerDescriptor = innerDescriptor;
        this.Comment = comment;
    }

    public string Comment { get; private set; }

    public object Value { get { return innerDescriptor.Value; } }
    public Type Type { get { return innerDescriptor.Type; } }
    public Type StaticType { get { return innerDescriptor.StaticType; } }
    public ScalarStyle ScalarStyle { get { return innerDescriptor.ScalarStyle; } }
}

public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
{
    public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
        : base(nextVisitor)
    {
    }

    public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
    {
        var commentsDescriptor = value as CommentsObjectDescriptor;
        if (commentsDescriptor != null && commentsDescriptor.Comment != null)
        {
            context.Emit(new Comment(commentsDescriptor.Comment, false));
        }

        return base.EnterMapping(key, value, context);
    }
}
