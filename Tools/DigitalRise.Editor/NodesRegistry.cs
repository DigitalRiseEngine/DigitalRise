﻿using DigitalRise.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DigitalRise.Editor
{
	class NodeTypeInfo
	{
		public string Category { get; }
		public Type Type { get; }
		public Type SubType { get; }

		public NodeTypeInfo(string category, Type type, Type subType)
		{
			if (string.IsNullOrEmpty(category))
			{
				throw new ArgumentNullException(nameof(category));
			}

			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			Category = category;
			Type = type;
			SubType = subType;
		}
	}

	internal static class NodesRegistry
	{
		private static readonly List<Assembly> _assemblies = new List<Assembly>();
		private static readonly SortedDictionary<string, List<NodeTypeInfo>> _nodesByCategories = new SortedDictionary<string, List<NodeTypeInfo>>();

		public static IReadOnlyDictionary<string, List<NodeTypeInfo>> NodesByCategories => _nodesByCategories;

		public static void AddAssembly(Assembly assembly)
		{
			if (_assemblies.Contains(assembly))
			{
				return;
			}

			foreach (var type in assembly.GetTypes())
			{
				var attrs = type.GetCustomAttributes<EditorInfoAttribute>(true);
				if (attrs == null)
				{
					continue;
				}

				foreach (var attr in attrs)
				{
					DR.LogInfo($"Adding node of type {type}");

					List<NodeTypeInfo> types;
					if (!_nodesByCategories.TryGetValue(attr.Category, out types))
					{
						types = new List<NodeTypeInfo>();
						_nodesByCategories[attr.Category] = types;
					}

					var typeInfo = new NodeTypeInfo(attr.Category, type, attr.SubType);
					types.Add(typeInfo);
				}
			}

			_assemblies.Add(assembly);
		}
	}
}