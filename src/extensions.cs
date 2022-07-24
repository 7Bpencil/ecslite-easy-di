// -----------------------------------------------------------------------------------------
// The MIT License
// Dependency injection for LeoECS Lite https://github.com/Leopotam/ecslite-di
// Copyright (c) 2021-2022 Leopotam <leopotam@gmail.com>
// Copyright (c) 2022 7Bpencil <Edward.Ekb@yandex.ru>
// -----------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Leopotam.EcsLite;

namespace SevenBoldPencil.EasyDi
{
    [AttributeUsage (AttributeTargets.Field)]
    public sealed class EcsSharedAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.Field)]
    public sealed class EcsInjectAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.Field)]
    public sealed class EcsWorldAttribute : Attribute
    {
        public readonly string World;

        public EcsWorldAttribute (string world = default)
        {
            World = world;
        }
    }

    [AttributeUsage (AttributeTargets.Field)]
    public sealed class EcsPoolAttribute : Attribute
    {
        public readonly string World;

        public EcsPoolAttribute (string world = default)
        {
            World = world;
        }
    }

    public static class Extensions
    {
        private static readonly Type WorldType = typeof (EcsWorld);
        private static readonly Type WorldAttrType = typeof (EcsWorldAttribute);
        private static readonly Type PoolType = typeof (EcsPool<>);
        private static readonly Type PoolAttrType = typeof (EcsPoolAttribute);
        private static readonly Type SharedAttrType = typeof (EcsSharedAttribute);
        private static readonly Type InjectAttrType = typeof (EcsInjectAttribute);
        private static readonly MethodInfo WorldGetPoolMethod = typeof (EcsWorld).GetMethod (nameof (EcsWorld.GetPool));
        private static readonly Dictionary<Type, MethodInfo> GetPoolMethodsCache = new Dictionary<Type, MethodInfo> (256);

        public static IEcsSystems Inject (this IEcsSystems systems, params object[] injects)
        {
            if (injects == null) { injects = Array.Empty<object> (); }
            var allSystems = systems.GetAllSystems();
            var shared = systems.GetShared<object> ();
            var sharedType = shared?.GetType ();

            foreach (var system in allSystems) {
                foreach (var f in system.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                    // skip statics.
                    if (f.IsStatic) { continue; }
                    // EcsWorld.
                    if (InjectWorld (f, system, systems)) { continue; }
                    // EcsPool.
                    if (InjectPool (f, system, systems)) { continue; }
                    // Shared.
                    if (InjectShared (f, system, shared, sharedType)) { continue; }
                    // Inject.
                    if (InjectCustomData (f, system, injects)) { continue; }
                }
            }

            return systems;
        }

        private static bool InjectWorld (FieldInfo fieldInfo, IEcsSystem system, IEcsSystems systems) {
            if (fieldInfo.FieldType != WorldType) {
                return false;
            }
            if (!Attribute.IsDefined(fieldInfo, WorldAttrType)) {
                return true;
            }

            var worldAttr = (EcsWorldAttribute) Attribute.GetCustomAttribute (fieldInfo, WorldAttrType);
            fieldInfo.SetValue (system, systems.GetWorld (worldAttr.World));
            return true;
        }

        private static bool InjectPool (FieldInfo fieldInfo, IEcsSystem system, IEcsSystems systems) {
            if (!fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.GetGenericTypeDefinition() != PoolType) {
                return false;
            }
            if (!Attribute.IsDefined(fieldInfo, PoolAttrType)) {
                return true;
            }

            var poolAttr = (EcsPoolAttribute) Attribute.GetCustomAttribute (fieldInfo, PoolAttrType);
            var world = systems.GetWorld (poolAttr.World);
            var componentTypes = fieldInfo.FieldType.GetGenericArguments ();
            fieldInfo.SetValue (system, GetGenericGetPoolMethod (componentTypes[0]).Invoke (world, null));
            return true;
        }

        private static MethodInfo GetGenericGetPoolMethod (Type componentType) {
            if (!GetPoolMethodsCache.TryGetValue (componentType, out var pool)) {
                pool = WorldGetPoolMethod.MakeGenericMethod (componentType);
                GetPoolMethodsCache[componentType] = pool;
            }
            return pool;
        }

        private static bool InjectShared (FieldInfo fieldInfo, IEcsSystem system, object shared, Type sharedType) {
            if (shared == null || !Attribute.IsDefined(fieldInfo, SharedAttrType)) {
                return false;
            }
            if (fieldInfo.FieldType.IsAssignableFrom (sharedType)) {
                fieldInfo.SetValue (system, shared);
            }
            return true;
        }

        private static bool InjectCustomData (FieldInfo fieldInfo, IEcsSystem system, object[] injects) {
            if (injects.Length <= 0 || !Attribute.IsDefined(fieldInfo, InjectAttrType)) {
                return false;
            }

            foreach (var inject in injects) {
                if (fieldInfo.FieldType.IsInstanceOfType (inject)) {
                    fieldInfo.SetValue (system, inject);
                    break;
                }
            }
            return true;
        }
    }

}
