
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Object = System.Object;
namespace KUtils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingleTonAttribute : Attribute
    {
        /// <summary>
        /// -1为框架启动初始化，其他为自定义enum，为了多支持使用int存储
        /// </summary>
        public int initTp { get; protected set; }
        /// <summary>
        /// -1为框架退出时候销毁/或者依托于系统关闭app的内存空间释放，其他为自定义enum，为了多支持使用int存储
        /// </summary>
        public int destroyTp { get; protected set; }

        public SingleTonAttribute():this(-1,-1){}
        public SingleTonAttribute(int value) : this(value, -1) { }
        public SingleTonAttribute(int init ,int destroy)
        {
            this.initTp = init;
            this.destroyTp = destroy;
        }
        /// <summary>
        /// Emit gen 接口
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mb"></param>
        /// <returns></returns>
        internal TypeBuilder Gen(Type target, ModuleBuilder mb)
        {

            var tb = mb.DefineType(target.Name + "Singleton", TypeAttributes.Class | TypeAttributes.Public);
            tb.SetParent(target);
            tb.DefineDefaultConstructor(MethodAttributes.Public);

            var fb = tb.DefineField("_mins", target, FieldAttributes.Private | FieldAttributes.Static);
            var getmethod = tb.DefineMethod("getInstance", MethodAttributes.Static | MethodAttributes.Public);
            getmethod.SetReturnType(target);
            var ins = target.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);


            var il = getmethod.GetILGenerator();
            //var localvalue_0 = il.DeclareLocal(target);
            var localvalue_1 = il.DeclareLocal(typeof(bool));
            var truecond = il.DefineLabel();
            var falsecond = il.DefineLabel();
            var end = il.DefineLabel();

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldsfld, fb);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Stloc_1, localvalue_1);
            il.Emit(OpCodes.Ldloc_1, localvalue_1);
            il.Emit(OpCodes.Brtrue_S, truecond);

            il.MarkLabel(falsecond);

            il.Emit(OpCodes.Newobj, ins);
            il.Emit(OpCodes.Stsfld, fb);

            il.MarkLabel(truecond);

            il.Emit(OpCodes.Ldsfld, fb);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Br_S, end);
            il.MarkLabel(end);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return tb;

        }

    }


}
