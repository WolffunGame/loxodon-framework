﻿using Loxodon.Log;
using System;
using System.Reflection;

namespace Loxodon.Framework.Binding.Reflection
{
    public abstract class AbstractProxyMethodInfo : IProxyMethodInfo
    {
        protected readonly MethodInfo methodInfo;
        protected bool isValueType = false;
        public AbstractProxyMethodInfo(MethodInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("Parameters' info 'cannot be null!");

            this.methodInfo = info;

#if NETFX_CORE
            this.isValueType = info.DeclaringType.GetTypeInfo().IsValueType;
#else
            this.isValueType = info.DeclaringType.IsValueType;
#endif
        }

        public MethodInfo MethodInfo { get { return this.methodInfo; } }

        public bool IsStatic { get { return this.methodInfo.IsStatic; } }

        public string Name { get { return this.methodInfo.Name; } }

        public Type DeclaringType { get { return this.methodInfo.DeclaringType; } }

        public Type ReturnType { get { return this.methodInfo.ReturnType; } }

        public ParameterInfo[] Parameters { get { return this.methodInfo.GetParameters(); } }

        public ParameterInfo ReturnParameter { get { return this.methodInfo.ReturnParameter; } }

        public abstract object Invoke(object target, params object[] args);
    }

    public class ProxyMethodInfo : AbstractProxyMethodInfo
    {
        public ProxyMethodInfo(MethodInfo info) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");
        }

        public override object Invoke(object target, params object[] args)
        {
            return this.methodInfo.Invoke(target, args);
        }
    }

    public class ProxyFuncInfo<T, TResult> : AbstractProxyMethodInfo, IProxyFuncInfo<T, TResult>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyFuncInfo<T, TResult>));

        private Func<T, TResult> function;

        public ProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public ProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyFuncInfo(string methodName, Func<T, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes, Func<T, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }

        public ProxyFuncInfo(MethodInfo info, Func<T, TResult> function) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(TResult).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = this.MakeFunc(this.methodInfo);
        }

        private Func<T, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return (Func<T, TResult>)this.methodInfo.CreateDelegate(typeof(Func<T, TResult>));
#elif !UNITY_IOS
                return (Func<T, TResult>)Delegate.CreateDelegate(typeof(Func<T, TResult>), this.methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual TResult Invoke(T target)
        {
            if (this.function != null)
                return this.function(target);

            return (TResult)this.methodInfo.Invoke(target, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target);
        }
    }

    public class ProxyFuncInfo<T, P1, TResult> : AbstractProxyMethodInfo, IProxyFuncInfo<T, P1, TResult>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyFuncInfo<T, P1, TResult>));
        private Func<T, P1, TResult> function;
        public ProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public ProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes, Func<T, P1, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }

        public ProxyFuncInfo(string methodName, Func<T, P1, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }


        public ProxyFuncInfo(MethodInfo info, Func<T, P1, TResult> function) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(TResult).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = this.methodInfo.GetParameters();
            if (parameters.Length != 1 || !typeof(P1).Equals(parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = this.MakeFunc(this.methodInfo);
        }

        private Func<T, P1, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return (Func<T, P1, TResult>)methodInfo.CreateDelegate(typeof(Func<T, P1, TResult>));
#elif !UNITY_IOS
                return (Func<T, P1, TResult>)Delegate.CreateDelegate(typeof(Func<T, P1, TResult>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual TResult Invoke(T target, P1 p1)
        {
            if (this.function != null)
                return this.function(target, p1);

            return (TResult)this.methodInfo.Invoke(target, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target, (P1)args[0]);
        }
    }

    public class ProxyFuncInfo<T, P1, P2, TResult> : AbstractProxyMethodInfo, IProxyFuncInfo<T, P1, P2, TResult>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyFuncInfo<T, P1, P2, TResult>));
        private Func<T, P1, P2, TResult> function;

        public ProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }


        public ProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyFuncInfo(string methodName, Func<T, P1, P2, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes, Func<T, P1, P2, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }

        public ProxyFuncInfo(MethodInfo info, Func<T, P1, P2, TResult> function) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(TResult).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = this.methodInfo.GetParameters();
            if (parameters.Length != 2 || !typeof(P1).Equals(parameters[0].ParameterType) || !typeof(P2).Equals(parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = this.MakeFunc(this.methodInfo);
        }

        private Func<T, P1, P2, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return (Func<T, P1, P2, TResult>)methodInfo.CreateDelegate(typeof(Func<T, P1, P2, TResult>));
#elif !UNITY_IOS
                return (Func<T, P1, P2, TResult>)Delegate.CreateDelegate(typeof(Func<T, P1, P2, TResult>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual TResult Invoke(T target, P1 p1, P2 p2)
        {
            if (this.function != null)
                return this.function(target, p1, p2);

            return (TResult)this.methodInfo.Invoke(target, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target, (P1)args[0], (P2)args[1]);
        }
    }

    public class ProxyFuncInfo<T, P1, P2, P3, TResult> : AbstractProxyMethodInfo, IProxyFuncInfo<T, P1, P2, P3, TResult>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyFuncInfo<T, P1, P2, P3, TResult>));
        private Func<T, P1, P2, P3, TResult> function;

        public ProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {

        }
        public ProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyFuncInfo(string methodName, Func<T, P1, P2, P3, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }

        public ProxyFuncInfo(string methodName, Type[] parameterTypes, Func<T, P1, P2, P3, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }

        public ProxyFuncInfo(MethodInfo info, Func<T, P1, P2, P3, TResult> function) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(TResult).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = this.methodInfo.GetParameters();
            if (parameters.Length != 3 || !typeof(P1).Equals(parameters[0].ParameterType) || !typeof(P2).Equals(parameters[1].ParameterType) || !typeof(P3).Equals(parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = this.MakeFunc(this.methodInfo);
        }

        private Func<T, P1, P2, P3, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return  (Func<T, P1, P2, P3, TResult>)methodInfo.CreateDelegate(typeof(Func<T, P1, P2, P3, TResult>));
#elif !UNITY_IOS
                return (Func<T, P1, P2, P3, TResult>)Delegate.CreateDelegate(typeof(Func<T, P1, P2, P3, TResult>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual TResult Invoke(T target, P1 p1, P2 p2, P3 p3)
        {
            if (this.function != null)
                return this.function(target, p1, p2, p3);

            return (TResult)this.methodInfo.Invoke(target, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target, (P1)args[0], (P2)args[1], (P3)args[2]);
        }
    }

    public class ProxyActionInfo<T> : AbstractProxyMethodInfo, IProxyActionInfo<T>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyActionInfo<T>));
        private Action<T> action;

        public ProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public ProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyActionInfo(string methodName, Action<T> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes, Action<T> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }

        public ProxyActionInfo(MethodInfo info, Action<T> action) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(void).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = this.MakeAction(this.methodInfo);
        }

        private Action<T> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return  (Action<T>)methodInfo.CreateDelegate(typeof(Action<T>));
#elif !UNITY_IOS
                return (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual void Invoke(T target)
        {
            if (this.action != null)
            {
                this.action(target);
                return;
            }

            this.methodInfo.Invoke(target, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target);
            return null;
        }
    }

    public class ProxyActionInfo<T, P1> : AbstractProxyMethodInfo, IProxyActionInfo<T, P1>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyActionInfo<T, P1>));
        private Action<T, P1> action;

        public ProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public ProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyActionInfo(string methodName, Action<T, P1> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes, Action<T, P1> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }

        public ProxyActionInfo(MethodInfo info, Action<T, P1> action) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(void).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = this.methodInfo.GetParameters();
            if (parameters.Length != 1 || !typeof(P1).Equals(parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = this.MakeAction(this.methodInfo);
        }

        private Action<T, P1> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return  (Action<T, P1>)methodInfo.CreateDelegate(typeof(Action<T, P1>));
#elif !UNITY_IOS
                return (Action<T, P1>)Delegate.CreateDelegate(typeof(Action<T, P1>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual void Invoke(T target, P1 p1)
        {
            if (this.action != null)
            {
                this.action(target, p1);
                return;
            }

            this.methodInfo.Invoke(target, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target, (P1)args[0]);
            return null;
        }
    }

    public class ProxyActionInfo<T, P1, P2> : AbstractProxyMethodInfo, IProxyActionInfo<T, P1, P2>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyActionInfo<T, P1, P2>));
        private Action<T, P1, P2> action;

        public ProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public ProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyActionInfo(string methodName, Action<T, P1, P2> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes, Action<T, P1, P2> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }

        public ProxyActionInfo(MethodInfo info, Action<T, P1, P2> action) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(void).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = this.methodInfo.GetParameters();
            if (parameters.Length != 2 || !typeof(P1).Equals(parameters[0].ParameterType) || !typeof(P2).Equals(parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = this.MakeAction(this.methodInfo);
        }

        private Action<T, P1, P2> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return  (Action<T, P1, P2>)methodInfo.CreateDelegate(typeof(Action<T, P1, P2>));
#elif !UNITY_IOS
                return (Action<T, P1, P2>)Delegate.CreateDelegate(typeof(Action<T, P1, P2>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual void Invoke(T target, P1 p1, P2 p2)
        {
            if (this.action != null)
            {
                this.action(target, p1, p2);
                return;

            }

            this.methodInfo.Invoke(target, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target, (P1)args[0], (P2)args[1]);
            return null;
        }
    }

    public class ProxyActionInfo<T, P1, P2, P3> : AbstractProxyMethodInfo, IProxyActionInfo<T, P1, P2, P3>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProxyActionInfo<T, P1, P2, P3>));
        private Action<T, P1, P2, P3> action;

        public ProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public ProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public ProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public ProxyActionInfo(string methodName, Action<T, P1, P2, P3> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }
        public ProxyActionInfo(string methodName, Type[] parameterTypes, Action<T, P1, P2, P3> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }

        public ProxyActionInfo(MethodInfo info, Action<T, P1, P2, P3> action) : base(info)
        {
            if (this.methodInfo.IsStatic)
                throw new ArgumentException("The method is static!");

            if (!typeof(void).Equals(this.methodInfo.ReturnType) || !this.methodInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = this.methodInfo.GetParameters();
            if (parameters.Length != 3 || !typeof(P1).Equals(parameters[0].ParameterType) || !typeof(P2).Equals(parameters[1].ParameterType) || !typeof(P3).Equals(parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = this.MakeAction(this.methodInfo);

        }

        private Action<T, P1, P2, P3> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (isValueType)
                    return null;
#if NETFX_CORE
                return (Action<T, P1, P2, P3>)methodInfo.CreateDelegate(typeof(Action<T, P1, P2, P3>));
#elif !UNITY_IOS
                return (Action<T, P1, P2, P3>)Delegate.CreateDelegate(typeof(Action<T, P1, P2, P3>), methodInfo);
#endif
            }
            catch (Exception e)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("{0}", e);
            }
            return null;
        }

        public virtual void Invoke(T target, P1 p1, P2 p2, P3 p3)
        {
            if (this.action != null)
            {
                this.action(target, p1, p2, p3);
                return;
            }

            this.methodInfo.Invoke(target, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target, (P1)args[0], (P2)args[1], (P3)args[2]);
            return null;
        }
    }

}
