
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System;
using System.Reflection.Emit;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
/*using System.Dynamic;*/
using Object = System.Object;
using KUtils;

internal class CodeProvider
{

    private Dictionary<Type,List<KeyValuePair<Type,Assembly>>> AttContainer = new Dictionary<Type, List<KeyValuePair<Type, Assembly>>> ();

    //private CodeDomProvider provider;

    //private ICodeCompiler icc;

    //private CodeCompileUnit targetUnit;

    private AppDomain TargetApp;

    private AssemblyName _STName;

    public AssemblyName STName {
        get {
            if (_STName == null)
                _STName = new AssemblyName ("Singleton");
            return _STName;
        }
    }

    private AssemblyBuilder _builder;

    private AssemblyBuilder STbuilder {
        get {
            if (_builder == null) {
                _builder = this.TargetApp.DefineDynamicAssembly (STName, AssemblyBuilderAccess.RunAndSave);
            }
            return this._builder;
            
        }
    }

    private void Reset()
    {
        //this.targetUnit = null;
        this.TargetApp = null;
        this._builder = null;
        this._STName = null;
        //this.provider = null;
        this.AttContainer.Clear();
        //this.icc = null;

    }


    public void Gen (AppDomain app)
    {
        this.Reset();
        this.TargetApp = app;
        //provider = CodeDomProvider.CreateProvider("CSharp");
        //icc = provider.CreateCompiler();
        //targetUnit = new CodeCompileUnit();

        Assembly[] assems = app.GetAssemblies ();

        if (assems != null) {
            foreach (var asm in assems) {
                this._AddAtt<SingleTonAttribute> (asm);
            }
        }

        this._AddAttFileds ();

        CustomAttributeBuilder  runtimeCompAtr = new CustomAttributeBuilder(typeof(RuntimeCompatibilityAttribute).GetConstructor(new Type[]{}),new object[]{});
        CustomAttributeBuilder ComCompAtr = new CustomAttributeBuilder(typeof(CompilationRelaxationsAttribute).GetConstructor(new Type[] { typeof(int)}), new object[] { 8});
        this.STbuilder.SetCustomAttribute(runtimeCompAtr);
        this.STbuilder.SetCustomAttribute(ComCompAtr);

        var objs = this.STbuilder.GetCustomAttributes(typeof(RuntimeCompatibilityAttribute),true);
        if (objs != null && objs.Length >0)
        {
            foreach (var sub in objs)
            {
                if (sub is RuntimeCompatibilityAttribute)
                {
                    var runtimeatr = sub as RuntimeCompatibilityAttribute;
                    runtimeatr.WrapNonExceptionThrows = true;

                }
                else if (sub is CompilationRelaxationsAttribute)
                {
                    //var compatr = sub as CompilationRelaxationsAttribute;

                }
            }

        }

        this.STbuilder.Save (STName + ".dll");

    }

    //private void GenCode (string fileName)
    //{
    //    CodeGeneratorOptions options = new CodeGeneratorOptions ();

    //    using (StreamWriter sourceWriter = new StreamWriter (fileName)) {
    //        provider.GenerateCodeFromCompileUnit (
    //            targetUnit, sourceWriter, options);
    //    }
    //}

    private void _AddAttFileds ()
    {
        foreach (var att in AttContainer) {
            if (att.Key == typeof(SingleTonAttribute)) {

                SingleTonAttribute value = new SingleTonAttribute ();
                foreach (var tp in att.Value) {
                    string fname = "SingleTon";

                    var fileld = TrygetFiled ("FieldName", att.Key, value);
                    if (fileld != null) {
                        fname = fileld as string;
                    }

                    var flags = TrygetFiled ("bindFlags", att.Key, value);
                    if (flags != null) {
                        this.AddAttField (fname, value, tp.Key, tp.Value, (BindingFlags)flags);          
                    } else {
                        this.AddAttField (fname, value, tp.Key, tp.Value);
                    }

                }
                
            } else {
                throw new Exception ("not Supported type " + att.Key);
                //Debug.LogError("not Supported type "+ att.Key);
            }
        }
            
        
    }

    private Object TrygetFiled (string name, Type tp, object o)
    {
        var field = tp.GetField (name);
        if (field == null || o == null) {
            return null;
        } else {
            return field.GetValue (o);
        }
        
    }

    private void AddAttField (string fieldname, object att, Type target, Assembly asm, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
    {
        MethodInfo method = att.GetType ().GetMethod ("Gen", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        if (method == null) {
            throw new Exception ("No Gen method!");
            //LogMgr.Log("No Gen method!");
        } else {

            FieldInfo info = target.GetField (fieldname);
            if (info != null) {
                info.SetValue (target, method.Invoke (att, new object[]{ target }));
            } else {
                var mb = STbuilder.DefineDynamicModule (STName.Name,STName+".dll");

                var ret = method.Invoke(att,new object[]{target,mb});
                if(ret is TypeBuilder)
                {
                    var tb = ret as TypeBuilder;
                    tb.CreateType();
                }

            }
            
        }

    }

    private void _AddAtt<T> (Assembly asm) where T:Attribute
    {
        List<KeyValuePair<Type,Assembly>> attlist = this.FindAttribute<T> (asm);
        Type atttp = typeof(T);
        foreach (var at in attlist) {
            if (!AttContainer.ContainsKey (atttp)) {
                var list = new List<KeyValuePair<Type,Assembly>> ();
                list.Add (at);
                AttContainer.Add (atttp, list);
            } else {
                AttContainer [atttp].Add (at);
            }

        }

    }

    private List<KeyValuePair<Type,Assembly>>  FindAttribute<T > (Assembly asm) where T :Attribute
    {
        List<KeyValuePair<Type,Assembly>> l = new List<KeyValuePair<Type, Assembly>> ();
        if (asm != null) {
            Type[] tps = asm.GetTypes ();
            if (tps != null && tps.Length > 0) {
                
                foreach (Type tp in tps) {
                    object[] at = tp.GetCustomAttributes (true);
                    if (at != null) {
                        foreach (object subat in at) {
                            if (subat is T) {
                                l.Add (new KeyValuePair<Type, Assembly> (tp, asm));
                            }
                        }
                    }
                }

               
            }
        }
        return l;
    }
}

