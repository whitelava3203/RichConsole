using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConsoleApp;
using RichConsole.Argument;

namespace RichConsole
{
    public static class Console
    {
        private static IConsoleApp consoleApp;
        private static bool IsInitialized = false;
        static Console()
        {
            if (!IsInitialized) Initialize();
            IsInitialized = true;
        }
        [STAThread]
        public static void Initialize()
        {
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            */
            ConsoleApp.ConsoleManager f = new ConsoleApp.ConsoleManager();
            Thread thread = new Thread(() =>
            {
                Application.Run(f);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();


            while (!f.Initialized) Thread.Sleep(25);

            consoleApp = f;
            return;
        }


        private static string Half2Full(string sHalf)
        {
            char[] ch = sHalf.ToCharArray(0, sHalf.Length);
            for (int i = 0; i < sHalf.Length; ++i)
            {
                if (ch[i] > 0x21 && ch[i] <= 0x7e)
                    ch[i] += (char)0xfee0;
                else if (ch[i] == 0x20)
                    ch[i] = (char)0x3000;
            }
            return (new string(ch));
        }
        private static bool 받침확인(char c)
        {
            if (c < 0xAC00 || c > 0xD7A3)
            {
                return false;
            }
            return (c - 0xAC00) % 28 > 0;
        }

        internal static string FixString(string origin)
        {
            StringBuilder sb = new StringBuilder(origin.Replace("을/를", SystemWords[0].ToString()).Replace("이/가", SystemWords[1].ToString()));
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == SystemWords[0])
                {
                    if (받침확인(sb[i - 1]))
                    {
                        sb[i] = '을';
                    }
                    else
                    {
                        sb[i] = '를';
                    }
                }
                else if (sb[i] == SystemWords[1])
                {
                    if (받침확인(sb[i - 1]))
                    {
                        sb[i] = '이';
                    }
                    else
                    {
                        sb[i] = '가';
                    }
                }
            }
            return Half2Full(sb.ToString());


        }

        private static char[] SystemWords = { '◈', '▣', '◐', '◑' };
        public class Format
        {
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach(TextData data in datalist)
                {
                    if(data is TextData.NextLine)
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Append((data as TextData.Writeable).str);
                    }
                }
                return sb.ToString();
            }

            static Format()
            {
                if (!IsInitialized) Initialize();
                IsInitialized = true;
            }

            public Format()
            {

            }
            public Format(params string[] tags)
            {
                this.tags.AddRange(tags);
            }
            public Format(IEnumerable<TextData> datalist)
            {
                this.datalist = datalist.ToList();
            }

            List<TextData> datalist = new List<TextData>();
            public List<string> tags = new List<string>();
            public Format AddTags(params string[] tags)
            {
                this.tags.AddRange(tags);
                foreach (TextData data in datalist)
                {
                    data.arg.tags = data.arg.tags.Concat(tags).ToArray();
                }
                return this;
            }
            public Format Append(string str)
            {
                datalist.Add(new TextData.Text(str, tags.ToArray()));
                return this;
            }
            public Format AppendLine(string str)
            {
                Append(str);
                datalist.Add(new TextData.NextLine(tags.ToArray()));
                return this;
            }
            public Format AppendLine()
            {
                datalist.Add(new TextData.NextLine(tags.ToArray()));
                return this;
            }
            public Format AppendButton(string str)
            {
                return AppendButton(str, str);
            }
            public Format AppendButton(string str, string name)
            {
                datalist.Add(new TextData.Button(str, name, tags.ToArray()));
                return this;
            }
            public Format AppendButton(string str, string name, params string[] tags)
            {
                List<string> newtags = tags.ToList();
                newtags.Add(name);
                datalist.Add(new TextData.Button(str, name, newtags.ToArray()));
                return this;
            }
            public Format AppendInputText(string str, string name)
            {
                datalist.Add(new TextData.InputText(str, name, tags.ToArray()));
                return this;
            }
            public Format AppendInputText(string str, string name, params string[] tags)
            {
                List<string> newtags = tags.ToList();
                datalist.Add(new TextData.InputText(str, name, newtags.ToArray()));
                return this;
            }
            public Format ToButton(string str)
            {
                return ToButton(str, str);
            }
            public Format ToButton(string str, string name)
            {
                return ToType<TextData.Button>(str, name, "");
            }
            public Format ToButton(string str, string name, params string[] tags)
            {
                return ToType<TextData.Button>(str, name, tags);
            }
            public Format ToInputText(string str, string name)
            {
                return ToType<TextData.InputText>(str, name);
            }
            public Format ToInputText(string str, string name, params string[] tags)
            {
                return ToType<TextData.InputText>(str, name, tags);
            }
            public void Print()
            {
                LowConsole.Write(datalist);
            }
            public void PrintLine()
            {
                this.AppendLine();
                LowConsole.Write(datalist);
            }
            private Format ToType<T>(string strorigin, string name, params string[] tags) where T : TextData.Writeable, new()
            {
                string str = FixString(strorigin);

                foreach (TextData arg in datalist)
                {
                    if (arg is TextData.Text)
                    {
                        TextData.Text text = arg as TextData.Text;
                        {
                            if (text.str.Contains(str))
                            {
                                if (text.str.StartsWith(str))
                                {
                                    T b1 = new T();
                                    b1.str = text.str.Substring(0, str.Length);
                                    b1.arg.name = name;
                                    b1.arg.tags = text.arg.tags;
                                    b1.arg.tags = b1.arg.tags.Concat(tags).ToArray();
                                    TextData.Text t2 = text;
                                    t2.str = text.str.Substring(str.Length);

                                    datalist.Replace<TextData>(text, (TextData)b1, t2);
                                    return this;
                                }
                                else if (text.str.EndsWith(str))
                                {
                                    TextData.Text t1 = text;
                                    t1.str = text.str.Substring(0, str.Length);

                                    T b2 = new T();
                                    b2.str = text.str.Substring(str.Length);
                                    b2.arg.name = name;
                                    b2.arg.tags = text.arg.tags;
                                    b2.arg.tags = b2.arg.tags.Concat(tags).ToArray();


                                    datalist.Replace<TextData>(text, t1, (TextData)b2);
                                    return this;
                                }
                                else
                                {
                                    int index = text.str.IndexOf(str);
                                    int length = str.Length;

                                    TextData.Text t1 = text;
                                    t1.str = text.str.Substring(0, index);

                                    T b2 = new T();
                                    b2.str = text.str.Substring(index, length);
                                    b2.arg.name = name;
                                    b2.arg.tags = text.arg.tags;
                                    b2.arg.tags = b2.arg.tags.Concat(tags).ToArray();

                                    TextData.Text t3 = text;
                                    t3.str = text.str.Substring(index + length);


                                    datalist.Replace<TextData>(text, t1, (TextData)b2, t3);
                                    return this;
                                }
                            }
                        }

                    }
                }
                return this;
            }
            public static Format Concat(params Format[] formats)
            {
                if(formats.Length == 0)
                {
                    return new Format();
                }

                IEnumerable<TextData> datalist = formats[0].datalist;

                if (formats.Length == 1)
                {
                    return new Format(datalist);
                }

                for (int i = 1; i < formats.Length; i++) 
                {
                    datalist = datalist.Concat(formats[i].datalist);
                }

                return new Format(datalist);
            }
            public static Format Stack(params Format[] formats)
            {
                if (formats.Length == 1)
                {
                    return formats[0];
                }

                Format output = formats[0];

                for(int i=1;i<formats.Length -1; i++)
                {
                    output = Stack(output, formats[i]);
                }

                return output;
            }
            private static Format Stack(Format f1, Format f2)
            {
                List<List<TextData>> f1lists = f1.SeperateFormatToLines();
                List<List<TextData>> f2lists = f2.SeperateFormatToLines();

                int min = Math.Min(f1lists.Count, f2lists.Count);

                for (int i = 0; i < min; i++)
                {
                    f1lists[i].AddRange(f2lists[i]);
                }


                if (f1lists.Count == f2lists.Count)
                {
                    return CombineLinesToFormat(f1lists);
                }
                else if (f1lists.Count > f2lists.Count)
                {
                    return CombineLinesToFormat(f1lists);

                }
                else   //f1lists.Count < l2lists.Count
                {
                    for (int i = min; i < f2lists.Count; i++)
                    {
                        f1lists.Add(f2lists[i]);
                    }
                    return CombineLinesToFormat(f1lists);
                }
            }
            private List<List<TextData>> SeperateFormatToLines()
            {
                List<List<TextData>> arglist = new List<List<TextData>>();


                List<TextData> imshi = new List<TextData>();
                foreach (TextData arg in this.datalist)
                {
                    if (arg is TextData.NextLine)
                    {
                        arglist.Add(imshi);
                        imshi = new List<TextData>();
                    }
                    else
                    {
                        imshi.Add(arg);
                    }
                }
                arglist.Add(imshi);

                return arglist;
            }
            private static Format CombineLinesToFormat(List<List<TextData>> arglist)
            {
                Format format = new Format();

                foreach (List<TextData> arg in arglist)
                {
                    format.datalist.AddRange(arg);
                    format.datalist.Add(new TextData.NextLine());
                }

                return format;
            }


            public static Format operator +(Format f1, string str2)
            {
                return f1.Append(str2);
            }
            public static Format operator -(Format f1, (string str, string name) strs2)
            {
                return f1.AppendButton(strs2.str, strs2.name);
            }
            public static Format operator %(Format f1, (string str, string name) strs2)
            {
                return f1.AppendInputText(strs2.str, strs2.name);
            }

            public static Format operator -(Format f1, string str)
            {
                return f1.AppendButton(str);
            }

            public static Format operator %(Format f1, string str)
            {
                return f1.AppendInputText(str, str);
            }

            public Format Done()
            {
                return this;
            }
        }

        public static class RichConsole
        {
            static RichConsole()
            {
                if (!IsInitialized) Initialize();
                IsInitialized = true;
            }
        }

        public static class SimpleConsole
        {
            static SimpleConsole()
            {
                if (!IsInitialized) Initialize();
            }

            public static void Write(string str)
            {
                LowConsole.Write(Sepreate(str));
            }
            public static void WriteLine(string str)
            {
                List<TextData> arglist = Sepreate(str);
                arglist.Add(new TextData.NextLine());
                LowConsole.Write(arglist);
            }
            public static void WriteLine()
            {
                List<TextData> arglist = new List<TextData>();
                arglist.Add(new TextData.NextLine());
                LowConsole.Write(arglist);
            }

            private static List<TextData> Sepreate(string str)
            {
                List<TextData> arglist = new List<TextData>();
                string str2 = str.Replace(Environment.NewLine, SystemWords[0].ToString());
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < str2.Length; i++)
                {
                    if (str2[i] != SystemWords[0])
                    {
                        sb.Append(str2[i]);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            arglist.Add(new TextData.Text(sb.ToString()));
                            sb.Clear();
                        }
                        arglist.Add(new TextData.NextLine());
                    }
                }

                arglist.Add(new TextData.Text(sb.ToString()));
                sb.Clear();

                return arglist;
            }
            public static string ReadLine()
            {
                return MiddleConsole.ReadLine();
            }
            public static ButtonReturn ReadButton()
            {
                return MiddleConsole.ReadButton();
            }
            public static string ReadButtonName()
            {
                return MiddleConsole.ReadButton().name;
            }
            public static ButtonReturn ReadButtonSecure(params string[] tags)
            {
            entry1:
                ButtonReturn btn = MiddleConsole.ReadButton();
            entry2:
                Func<string, bool> Checker = (str) =>
                {
                    if (tags.Contains(str))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };

                if (btn.tags.Any(Checker))
                {
                    Format format = new Format("secure_ask");
                    format.AppendLine("정말　" + btn.name + "　선택합니까?");
                    format.AppendLine("[예]　　　　[아니오]");
                    format.ToButton("[예]", "예");
                    format.ToButton("[아니오]", "아니오");
                    format.Print();
                    ButtonReturn btn2 = SimpleConsole.ReadButton();
                    if (btn2.name == "예")
                    {
                        return btn;
                    }
                    else if (btn2.name == "아니오")
                    {
                        SimpleConsole.Clear("secure_ask");
                        goto entry1;
                    }
                    else
                    {
                        SimpleConsole.Clear("secure_ask");
                        goto entry2;
                    }
                }
                else
                {
                    SimpleConsole.Clear("secure_ask");
                    return btn;
                }
            }

            public static string GetInputText(string name)
            {
                return consoleApp.GetInputText(name);
            }
            public static void Clear()
            {
                LowConsole.Clear();
            }

            public static void Clear(string tag)
            {
                LowConsole.Clear((arg) =>
                {
                    if (arg.arg.tags.Contains(tag))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
            }
        }


        internal static class MiddleConsole
        {



            public static string ReadLine()
            {
                return Task.Run<string>(Console.consoleApp.ReadLine).Result;
            }

            public static ButtonReturn ReadButton()
            {
                return Task.Run<TextData.Button>(Console.consoleApp.ReadButton).Result.arg;
            }


        }
        internal static class LowConsole
        {
            public static List<TextData.NextLine> nextlinelist = new List<TextData.NextLine>();
            private static bool FindNextNull(out int index)
            {
                int i;
                for (i=0;i<nextlinelist.Count;i++)
                {
                    if (nextlinelist[i] == null)
                    {
                        index = i;
                        return true;
                    }
                }
                index = i;
                return false;
            }
            private static void AddNextLine(TextData.NextLine nextline)
            {
                if(FindNextNull(out int index))
                {
                    nextlinelist[index] = nextline;
                }
                else
                {
                    nextlinelist.Add(nextline);
                }
            }
            private static void FixLineEnd()
            {
                while(nextlinelist.Last() == null)
                {
                    nextlinelist.RemoveAt(nextlinelist.Count-1);
                }
            }
            private static void RemoveAllLine(Predicate<TextData.NextLine> predicate)
            {
                for(int i=0;i<nextlinelist.Count;i++)
                {
                    if(predicate(nextlinelist[i]))
                    {
                        nextlinelist[i] = null;
                    }
                }
                FixLineEnd();
            }
            private static int currentline
            {
                get
                {
                    FindNextNull(out int index);
                    return index + 1;
                }
            }



            public static void Write(IEnumerable<TextData> line)
            {
                IEnumerator<TextData> e = line.GetEnumerator();
                while (e.MoveNext())
                {
                    Write(e.Current);
                }
            }
            public static void Write(TextData obj)
            {
                if(obj is TextData.NextLine)
                {
                    AddNextLine(obj as TextData.NextLine);
                    currentxpos = 12;
                    return;
                }

                TextObject textObj = new TextObject();

                int width;

                textObj.data = obj as TextData.Writeable;
                width = GetStringWidth(textObj.data.str, Argument.StaticArgs.BaseFont);
                textObj.location = new Rectangle(currentxpos, currentypos, width, 20);
                currentxpos += width;
                Console.consoleApp.Write(textObj);

            }

            public static void Clear()
            {
                Console.consoleApp.Clear((_) => true);
                nextlinelist.Clear();
            }

            public static void Clear(Predicate<TextData> predicate)
            {
                Console.consoleApp.Clear(predicate);
                RemoveAllLine(predicate);
            }


            private static Point screensize = new Point();
            private static int lineborder = 20;
            private static int fontsize = 9;



            
            private static int currentypos
            {
                get
                {
                    return currentline * (lineborder + 1);
                }
            }
            private static int currentxpos = 12;





            private static int GetStringWidth(string text, Font font)
            {
                return text.Length * 17;


                int bytecount = new UTF8Encoding().GetByteCount(text);
                int length = text.Length;
                return ((bytecount - length) / 2 + length) * 9 - 1;

                using Image fakeImage = new Bitmap(1, 1);
                Graphics graphics = Graphics.FromImage(fakeImage);
                return graphics.MeasureString(text, font).ToSize().Width - 4;
            }
            private static int GetStringHeight(string text, Font font)
            {
                using Image fakeImage = new Bitmap(1, 1);
                Graphics graphics = Graphics.FromImage(fakeImage);
                return graphics.MeasureString(text, font).ToSize().Height;
            }
        }
    }



    namespace Argument
    {
        public static class StaticArgs
        {
            public static TextColor BaseTextColor = new TextColor(Color.White, Color.Black, Color.White, Color.Black);
            public static TextColor BaseButtonColor = new TextColor(Color.White, Color.Black, Color.Yellow, Color.Black);
            public static TextColor BaseInputTextColor = new TextColor(Color.Black, Color.White, Color.Black, Color.White);

            public static Font BaseFont;// { get; set;} = new Font("돋움체", 12f, FontStyle.Bold);
        }
        public struct TextColor
        {
            public Color basecolor;
            public Color backcolor;
            public Color hoverbasecolor;
            public Color hoverbackcolor;
            public TextColor(Color basecolor, Color backcolor, Color hoverbasecolor, Color hoverbackcolor)
            {
                this.basecolor = basecolor;
                this.backcolor = backcolor;
                this.hoverbasecolor = hoverbasecolor;
                this.hoverbackcolor = hoverbackcolor;
            }
        }
        public class ButtonReturn
        {
            public string name;
            public string[] tags;
        }
        public abstract class TextData
        {
            public abstract class Writeable : TextData
            {
                public override string ToString()
                {
                    return str;
                }
                public string str { get; set; }
                public TextColor textColor { get; set; }
                public Font font { get; set; }
            }
            public class Text : Writeable
            {
                public Text()
                { 
                    this.textColor = StaticArgs.BaseTextColor; 
                }
                public Text(string str) : this()
                {
                    this.str = str;
                }
                public Text(string str, params string[] tags) : this(str)
                {
                    this.arg.tags = tags;
                }
            }
            public class Button : Writeable
            {
                public Button() 
                {
                    this.textColor = StaticArgs.BaseButtonColor;
                }
                public Button(string str, string name) : this()
                {
                    this.str = str;
                    this.arg.name = name;
                }
                public Button(string str, string name, params string[] tags) : this(str,name)
                {
                    this.arg.tags = tags;
                }
            }
            public class InputText : Writeable
            {
                public InputText() 
                {
                    this.textColor = StaticArgs.BaseInputTextColor;
                }
                public InputText(string str, string name) : this()
                {
                    this.str = str;
                    this.arg.name = name;
                }
                public InputText(string str, string name, params string[] tags) : this(str, name)
                {
                    this.arg.tags = tags;
                }
            }
            public class NextLine : TextData
            {
                public override string ToString()
                {
                    return Environment.NewLine;
                }
                public NextLine() { }
                public NextLine(params string[] tags)
                {
                    this.arg.tags = tags;
                }


            }
            public ButtonReturn arg { get; set; } = new ButtonReturn();
        }
        public class TextObject
        {
            public Rectangle location;
            public TextData.Writeable data;
        }
        public class ImageObject
        {

        }
    }

    public interface IConsoleApp
    {
        public void Write(TextObject obj);
        public Task<string> ReadLine();
        public Task<TextData.Button> ReadButton();
        public string GetInputText(string name);
        public void Clear(Predicate<TextData> predicate);


        public bool Initialized { get; set; }

        public Action CloseEvent { get; set; }
    }
    internal static class ExtensionMethods
    {

        public static void Replace<T>(this List<T> ts, T origin, params T[] replace)
        {
            int index = ts.IndexOf(origin);
            ts.RemoveAt(index);
            ts.InsertRange(index, replace);
        }
        public static void Replace<T>(this List<T> ts, T origin, List<T> replace)
        {
            int index = ts.IndexOf(origin);
            ts.RemoveAt(index);
            ts.InsertRange(index, replace);
        }
        public static void ReplaceAll<T>(this List<T> ts, T origin, params T[] replace)
        {
            while (ts.Contains(origin))
            {
                int index = ts.IndexOf(origin);
                ts.RemoveAt(index);
                ts.InsertRange(index, replace);
            }
        }

        public static void ReplaceAll<T>(this List<T> ts, T origin, List<T> replace)
        {
            while (ts.Contains(origin))
            {
                int index = ts.IndexOf(origin);
                ts.RemoveAt(index);
                ts.InsertRange(index, replace);
            }
        }
    }
}
