using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

namespace Tobo.Net
{
    internal class PacketRegister
    {
        internal static void Register()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            MethodInfo method = typeof(Packet).GetMethod(nameof(Packet.Register), flags);

            foreach (Type t in GetTypesOf(typeof(Packet)))
            {
                try
                {
                    MethodInfo generic = method.MakeGenericMethod(t);
                    generic.Invoke(null, null);
                }
                catch (ArgumentException)
                {
                    Debug.LogError($"Error registering packet, ensure {t} has a public, parameterless [new()] constructor.");
                    throw;
                }
                
                //Packet.Register<>();
                //Activator.CreateInstance
            }
        }

        static List<Type> GetTypesOf(Type attribType)
        {
            List<Type> allTypes = new List<Type>();
            //BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            //Type packet = typeof(Packet);
            string name = attribType.FullName;

            foreach (Assembly assembly in GetAssemblies())
            {
                Type[] methods = assembly.GetTypes()
                        //.Where(t => t.GetCustomAttributes(attribType, false).Length > 0)
                        .Where(t => t.BaseType?.FullName == name)
                        .ToArray();

                allTypes.AddRange(methods);
            }

            return allTypes;
        }

        static List<Assembly> GetAssemblies()
        {
            var assemblies = new List<Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Mono.Cecil"))
                    continue;

                if (assembly.FullName.StartsWith("UnityScript"))
                    continue;

                if (assembly.FullName.StartsWith("Boo.Lan"))
                    continue;

                if (assembly.FullName.StartsWith("System"))
                    continue;

                if (assembly.FullName.StartsWith("I18N"))
                    continue;

                if (assembly.FullName.StartsWith("UnityEngine"))
                    continue;

                if (assembly.FullName.StartsWith("UnityEditor"))
                    continue;

                if (assembly.FullName.StartsWith("mscorlib"))
                    continue;

                assemblies.Add(assembly);
            }

            return assemblies;
        }

        /*
        public void CreateMessageHandlersDictionary(byte messageHandlerGroupId)
        {
            MethodInfo[] methods = FindMessageHandlers();

            messageHandlers = new Dictionary<ushort, MessageHandler>(methods.Length);
            foreach (MethodInfo method in methods)
            {
                MessageHandlerAttribute attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
                if (attribute.GroupId != messageHandlerGroupId)
                    continue;

                if (!method.IsStatic)
                    throw new NonStaticHandlerException(method.DeclaringType, method.Name);

                Delegate clientMessageHandler = Delegate.CreateDelegate(typeof(MessageHandler), method, false);
                if (clientMessageHandler != null)
                {
                    // It's a message handler for Client instances
                    if (messageHandlers.ContainsKey(attribute.MessageId))
                    {
                        MethodInfo otherMethodWithId = messageHandlers[attribute.MessageId].GetMethodInfo();
                        throw new DuplicateHandlerException(attribute.MessageId, method, otherMethodWithId);
                    }
                    else
                        messageHandlers.Add(attribute.MessageId, (MessageHandler)clientMessageHandler);
                }
                else
                {
                    // It's not a message handler for Client instances, but it might be one for Server instances
                    Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(Server.MessageHandler), method, false);
                    if (serverMessageHandler == null)
                        throw new InvalidHandlerSignatureException(method.DeclaringType, method.Name);
                }
            }
        }
        */
    }
}
