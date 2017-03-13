using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KUtils;
using KFrameWork;
using System.IO;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;

[Serializable]
public class CodeInfoList
{
    public List<SharpCodeModel> CodeList = new List<SharpCodeModel>();
}

[InitializeOnLoad]
public class SharpCodeGenerate  {
    #region fields
    private static SharpCodeGenerate mIns;
    private CodeInfoList Codes;

    private List<FieldCode> FieldList = new List<FieldCode>();

    private int SyCnt;
    private SeekStatus lastStatus;
    private SeekStatus status = SeekStatus.usingref;
    #endregion
    static SharpCodeGenerate()
    {
        mIns = new SharpCodeGenerate();
        //this.inspectFiles();

        // LogMgr.LogError(Regex.Match("Dictionary<int,", "Dictionary<((?!(int|uint|short|ushort|bool|long|ulong|float|double|string|byte)).)*,").Captures.Count);
        //LogMgr.LogError("string", "((?!(int|uint|short|ushort|bool|long|ulong|float|double|string|byte)).)*");
        // LogMgr.LogError(Regex.Match("Dictionary<{0b}", "^\\w+<{[0-9]?b}").Captures.Count);
        //string s = "public Lis<int, int> PlayerMoveInfo =new Lis<int,int>();";
        //LogMgr.Log(Regex.Match("public Lis<int, int> PlayerMoveInfo =new Lis<int,int>();",
        //    "^(public|private|protected|internal)?\\s?([0-9A-Za-z_?]+|\\w+<[0-9A-Za-z<,_> ?]+>)\\s+[a-z0-9A-Z_]+\\s?\\w*").Captures.Count);

    }



    [MenuItem("Assets/Tools/GenCode")]
    private static void GetCodes()
    {
        mIns.inspectFiles();
    }

    private void AddTest()
    {
        SharpCodeModel model = new SharpCodeModel();
        model.Flag = FileSearchFlag.Absolute | FileSearchFlag.EndWith | FileSearchFlag.IgnoreUpLower;
        model.FlagString = "Resp.cs";
        model.TargetPath = "Assets/Game/Scripts/Net";

        SubSharpCode sub = new SubSharpCode();
        sub.replaceContent = "info";
        sub.replaceflag = ReplaceFlag.InsertEndWithMethod;
        sub.seekContent = "";
        sub.seekFlag = SeekFlag.ToMethod | SeekFlag.Field;

        model.SubCodes.Add(sub);

        CodeInfoList l = new CodeInfoList();
        l.CodeList.Add(model);
        string result = JsonUtility.ToJson(l);
        File.WriteAllText(Path.GetFullPath(Application.dataPath + "/test.txt"), result);
    }

    private bool IsFlag(FileSearchFlag left, FileSearchFlag right)
    {
        return ((int)left & (int)right) == (int)right;
    }

    private bool IsFlag(SeekFlag left, SeekFlag right)
    {
        return ((int)left & (int)right) == (int)right;
    }

    private bool IsFlag(ReplaceFlag left, ReplaceFlag right)
    {
        return ((int)left & (int)right) == (int)right;
    }

    public void inspectFiles()
    {
        this.Codes = null;
        this.FieldList.Clear();

        string fullpath = Path.GetFullPath(Application.dataPath);
        DirectoryInfo Dir = new DirectoryInfo(fullpath);

        FileInfo[] fs = Dir.GetFiles("*.codemodel", SearchOption.AllDirectories);

        foreach (FileInfo f in fs)
        {
            string info = File.ReadAllText(f.FullName);
            CodeInfoList code = JsonUtility.FromJson<CodeInfoList>(info);
            this.Codes = code;
        }

        if (Codes.CodeList.Count > 0)
        {
            if (EditorUtility.DisplayDialog("Tips","发现有代码生成文件，需要自动生成么？","Ok","No"))
            {
                this.StartGenerate();
            }
        }
    }

    private void StartGenerate()
    {
        for (int i = 0; i < Codes.CodeList.Count; ++i)
        {
            this.GenerateCode(Codes.CodeList[i]);
           
        }
    }

    private void GenerateCode(SharpCodeModel model)
    {
        string searchName = model.FlagString;
        string searchPath = model.TargetPath;
        if (IsFlag(model.Flag, FileSearchFlag.Absolute))
        {
            searchPath = Path.GetFullPath(searchPath);
        }

        string[] filenames = null;

        List<string> resultFiles = new List<string>();

        if (IsFlag(model.Flag, FileSearchFlag.IgnoreUpLower))
        {
            searchName = searchName.ToLower();
            filenames = Directory.GetFiles(searchPath,"*.cs",SearchOption.AllDirectories);
            foreach (var f in filenames)
            {
                string ulname = f.ToLower().Trim();
                if (IsFlag(model.Flag, FileSearchFlag.StartWith))
                {
                    if (ulname.StartsWith(searchName))
                    {
                        resultFiles.Add(f);
                    }
                }
                else if (IsFlag(model.Flag, FileSearchFlag.EndWith))
                {
                    if (ulname.EndsWith(searchName))
                    {
                        resultFiles.Add(f);
                    }
                }
                else if (IsFlag(model.Flag, FileSearchFlag.Contains))
                {
                    if (ulname.Contains(searchName))
                    {
                        resultFiles.Add(f);
                    }
                }
            }
        }
        else
        {
            filenames = Directory.GetFiles(searchPath,searchName, SearchOption.AllDirectories);
            foreach (var f in filenames)
            {
                if (IsFlag(model.Flag, FileSearchFlag.StartWith))
                {
                    if (f.Trim().StartsWith(searchName))
                    {
                        resultFiles.Add(f);
                    }
                }
                else if (IsFlag(model.Flag, FileSearchFlag.EndWith))
                {
                    if (f.Trim().EndsWith(searchName))
                    {
                        resultFiles.Add(f);
                    }
                }
                else if (IsFlag(model.Flag, FileSearchFlag.Contains))
                {
                    if (f.Trim().Contains(searchName))
                    {
                        resultFiles.Add(f);
                    }
                }
            }
        }

        //start generate
        for (int i = 0; i < resultFiles.Count; ++i)
        {
            Generate_SubCodeByFile(resultFiles[i],model);
            this.FieldList.Clear();
            status = SeekStatus.usingref;
            SyCnt = 0;
        }
    }

    private void Generate_SubCodeByFile(string target, SharpCodeModel model)
    {
        string fullpath = Path.GetFullPath(target);

        string info = File.ReadAllText(fullpath);

        string[] lines = info.Split('\n');

        StringBuilder sb = new StringBuilder();
        List<string> lineList = new List<string>();

        foreach (var sub in lines)
        {
            lineList.AddRange(sub.Split('\r'));
        }

        int Index = -1;
        for (int j = lineList.Count - 1; j >= 0; --j)
        {
            if (lineList[j].Contains("}"))
            {
                Index = j;
                break;
            }
        }

        for (int j=0; j < lineList.Count;++j)
        {
            string subline = lineList[j];
            if (subline.Length == 0)
                continue;

            //LogMgr.Log(subline);
            for (int i = 0; i < model.SubCodes.Count; ++i)
            {
                Seek_Content(subline, model.SubCodes[i]);
            }

            if (Index != -1 && Index > j)
            {
                if(subline.EndsWith("\n"))
                    sb.Append(subline);
                else
                    sb.Append(subline+"\n");
            }
        }


        //prepare
        Prepare_Insert(sb, model, lineList);

        sb.AppendLine("}");

        string result = sb.ToString();

        if (inspectDup(result, lineList))
        {
            this.WriteToFile(result, target, model);
        }
        else
        {
            LogMgr.LogWarningFormat("已定义重复内容 {0} :result :{1}",target,result);
        }
    }

    private int CharNum(string str, string search)
    {
        if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(search))
        {
            if (str.Contains(search))
            {
                string rep = str.Replace(search, "");
                return (str.Length - rep.Length) / search.Length;
            }
        }
        return 0;
    }

    private bool inspectDup(string result, List<string> lineList)
    {
        foreach (var sub in lineList)
        {
            if (sub.Contains("///"))
            {
                continue;
            }

            int cnt = CharNum(result, sub.Trim().Replace("{","").Replace("}",""));
            if (cnt > 1)
            {
                //LogMgr.LogError("重复 "+ cnt+"  Content :"+sub);
                return false;
            }
        }
        return true;
    }

    private void WriteToFile(string result , string filename, SharpCodeModel model)
    {
        string fullDirpath = null;
        if (string.IsNullOrEmpty(model.OutPutPath))
        {
            fullDirpath = Path.GetDirectoryName(filename);
        }
        else
        {
            fullDirpath = Path.GetFullPath(model.OutPutPath);
        }
 
        if (!Directory.Exists(fullDirpath))
        {
            Directory.CreateDirectory(fullDirpath);
        }

        string outputPath = Path.Combine(fullDirpath, Path.GetFileName(filename).Split('.')[0] + ".cs");

        File.WriteAllText(outputPath, result);
    }

    private bool ContainsFlag(SharpCodeModel model, ReplaceFlag flag)
    {
        for (int i = 0; i < model.SubCodes.Count; ++i)
        {
            SubSharpCode c = model.SubCodes[i];
            if (c.replaceflag == flag)
            {
                return true;
            }
        }
        return false;
    }


    private void Prepare_Insert(StringBuilder sb,SharpCodeModel model, List<string> lines)
    {
        for (int i = 0; i < model.SubCodes.Count; ++i)
        {
            SubSharpCode c = model.SubCodes[i];
            if (IsFlag(c.replaceflag,ReplaceFlag.InsertHead))
            {
                if (lines.Find(p => p.Trim().Equals(c.replaceContent)) == null)
                {
                    string result = sb.ToString();
                    sb.Length = 0;
                    sb.AppendLine(c.replaceContent);
                    sb.Append(result);
                }
                   
            }

            if (IsFlag(c.replaceflag, ReplaceFlag.InsertField))
            {

            }

            if (IsFlag(c.replaceflag, ReplaceFlag.InsertMethodHead))
            {

            }

            if (IsFlag(c.replaceflag, ReplaceFlag.ReplaceImmediately))
            {

            }

            if (IsFlag(c.replaceflag, ReplaceFlag.InsertEndWithMethod))
            {
                if (c.seekFlag == (SeekFlag.ToMethod | SeekFlag.Field))
                {
                    for (int k = 0; k < this.FieldList.Count; ++k)
                    {
                        FieldCode field = this.FieldList[k];
                        if (!field.isgeneric)
                        {
                            bool enable = false;
                            if (Regex.Match(c.seekContent, "{[0-9]?b}").Captures.Count > 0)
                            {
                                string temp = Regex.Replace(c.seekContent, "{[0-9]?b}", "(int|uint|short|ushort|bool|long|ulong|float|double|string|byte)");
                                enable = Regex.Match(field.TypeName, temp).Captures.Count > 0;
                                //enable = Regex.Match(field.TypeName, "^\\w+<(int|uint|short|ushort|bool|long|ulong|float|double|string)").Captures.Count > 0;
                            }
                            else if (Regex.Match(c.seekContent, "{[0-9]?!b}").Captures.Count > 0)
                            {
                                //enable = Regex.Match(field.TypeName, "^\\w+<(int|uint|short|ushort|bool|long|ulong|float|double|string)").Captures.Count == 0;
                                string temp = Regex.Replace(c.seekContent, "{[0-9]?!b}", "^((?!(int|uint|short|ushort|bool|long|ulong|float|double|string|byte)).)*$");
                                enable = Regex.Match(field.TypeName, temp).Captures.Count > 0;
                            }
                            else if (field.TypeName.ToLower().Contains(c.seekContent.ToLower()))
                            {
                                enable = true;
                            }

                            if (enable)
                            {
                                string modified = Regex.Replace(c.replaceContent, "{[0-9]+f}", field.name);
                                modified = Regex.Replace(modified, "{[0-9]+t}", field.TypeName);
                                if (modified.EndsWith("\n"))
                                    sb.Append(modified);
                                else
                                    sb.Append(modified + "\n");
                            }
                        }
                        else
                        {
                            LogMgr.LogWarningFormat("这是一个泛型字段 {0}",field.name);
                        }

                    }
                }
                else if (c.seekFlag == (SeekFlag.ToMethod | SeekFlag.Field | SeekFlag.ContentByFieldType))
                {
                    for (int k = 0; k < this.FieldList.Count; ++k)
                    {
                        FieldCode field = this.FieldList[k];
                        if (field.TypeName.ToLower() == c.seekContent.ToLower())
                        {

                        }
                    }
                }
                else if (c.seekFlag == (SeekFlag.ToMethod | SeekFlag.Field | SeekFlag.ContentByContainsFieldType))
                {
                    for (int k = 0; k < this.FieldList.Count; ++k)
                    {
                        bool enable = false;

                        FieldCode field = this.FieldList[k];
                        if (Regex.Match(c.seekContent, "^\\w+<{[0-9]?b}").Captures.Count > 0)//generic
                        {
                            string temp = Regex.Replace(c.seekContent, "{[0-9]?b}", "(int|uint|short|ushort|bool|long|ulong|float|double|string|byte)");
                            enable = Regex.Match(field.TypeName, temp).Captures.Count >0;
                            // Regex.Match(field.TypeName,"^\\w+<(int|uint|short|ushort|bool|long|ulong|float|double|string)").Captures.Count >0;
                        }
                        else if (Regex.Match(c.seekContent, "^\\w+<{[0-9]?!b}").Captures.Count > 0)//generic
                        {
                            string temp = Regex.Replace(c.seekContent, "{[0-9]?!b}", "((?!(int|uint|short|ushort|bool|long|ulong|float|double|string|byte)).)*");
                            enable = Regex.Match(field.TypeName, temp).Captures.Count > 0;
                            // enable = Regex.Match(field.TypeName, "^\\w+<(int|uint|short|ushort|bool|long|ulong|float|double|string)").Captures.Count == 0;

                        }
                        else if (field.TypeName.ToLower().Contains( c.seekContent.ToLower()))
                        {
                            enable = true;
                        }

                        if (enable)
                        {
                            int leftIndex = field.TypeName.IndexOf('<');
                            int rightIndex = field.TypeName.IndexOf('>');
                            bool isgeneric = leftIndex != -1 && rightIndex != -1;
                            if (isgeneric)
                            {
                                string substring = field.TypeName.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
                                if (substring.Length > 0)
                                {
                                    string[] gs = substring.Split(',');
                                    string modified = c.replaceContent.Replace("{0T}", gs[0]);
                                    if (gs.Length > 1)
                                        modified = modified.Replace("{1T}", gs[1]);

                                    modified = Regex.Replace(modified, "{[0-9]+f}", field.name);
                                    modified = Regex.Replace(modified, "{[0-9]+t}", field.TypeName);

                                    if (modified.EndsWith("\n"))
                                        sb.Append(modified);
                                    else
                                        sb.Append(modified + "\n");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (c.replaceContent.EndsWith("\n"))
                        sb.Append(c.replaceContent);
                    else
                        sb.Append(c.replaceContent + "\n");
                }
            }
        }
    }

    private void Seek_Status(string line )
    {
        if (this.status == SeekStatus.usingref && line.Contains("using"))
        {
            this.status = SeekStatus.usingref;
        }
        else if (this.status == SeekStatus.usingref && line.Contains(" class "))
        {
            lastStatus = SeekStatus.usingref;
            this.status = SeekStatus.ClassDefine;
            if (line.Contains("{"))
            {
                lastStatus = status;
                this.status = SeekStatus.syntaxStart;
                this.SyCnt++;
            }
        }
        else if (this.status == SeekStatus.ClassDefine && line.Contains("{"))
        {
            lastStatus = status;
            this.status = SeekStatus.syntaxStart;
            this.SyCnt++;
        }
        else if (this.status == SeekStatus.syntaxStart && line.Contains("}"))
        {
            lastStatus = status;
            this.status = SeekStatus.syntaxEnd;
            SyCnt--;
            if (SyCnt == 0)
            {
                lastStatus = status;
                this.status = SeekStatus.ClassDefineEnd;
            }
                
        }
    }

    private void Seek_Content(string line, SubSharpCode config)
    {
        Seek_Status(line);

        if (IsFlag(config.seekFlag, SeekFlag.Field))
        {
            string triminfo = line.Trim();
            string[] parts = triminfo.Split(' ');
            if (parts.Length > 1 
                &&  (this.lastStatus == SeekStatus.ClassDefine && this.status == SeekStatus.syntaxStart)
                && triminfo.EndsWith(";")
                && Regex.Match(triminfo, "^(public|private|protected|internal)?\\s?([0-9A-Za-z_?]+|\\w+<[0-9A-Za-z<,_> ?]+>)\\s+[a-z0-9A-Z_]+\\s?").Captures.Count >0)
            {
                string fieldName =null;
                string fieldType =null;

                if (parts[0].ToLower() == "public" || parts[0].ToLower() == "private" || parts[0].ToLower() == "protected" || parts[0].ToLower() == "internal")
                {
                    fieldType = parts[1].Replace(';', ' ');
                    fieldName = parts[2].Replace(';', ' ');
                    if (fieldName.Contains(">"))
                    {
                        fieldType = fieldType + fieldName;
                        fieldName = parts[3].Replace(';', ' ');
                    }

                }
                else
                {
                    fieldType = parts[0].Replace(';', ' ');
                    fieldName = parts[1].Replace(';', ' ');
                    if (fieldName.Contains(">"))
                    {
                        fieldType = fieldType + fieldName;
                        fieldName = parts[2].Replace(';', ' ');
                    }
                }

                int leftIndex = fieldType.IndexOf('<');
                int rightIndex = fieldType.IndexOf('>');
                bool isgeneric = leftIndex != -1 && rightIndex != -1;

                FieldCode code = this.FieldList.Find(p => p.name == fieldName && p.TypeName == fieldType);
                if (code == null)
                {
                    code = new FieldCode();
                    code.name = fieldName;
                    code.TypeName = fieldType;
                    code.isgeneric = isgeneric;

                    this.FieldList.Add(code);
                }
            }
        }
        else if (IsFlag(config.seekFlag, SeekFlag.Class))
        {

        }
        else if (IsFlag(config.seekFlag, SeekFlag.Method))
        {

        }
    }

}
