using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/********************************************************************
	created:	2012/01/17
	created:	17:1:2012   10:16
	filename: 	Common\Log5.cs
	file base:	Log5
	file ext:	cs
	author:		Cupid

	purpose:	写日志的类
 * 一个类实例默认会创建三个日志，错误日志，普通日志和调试日志。理论上支持多线程，但没有经过测试。
 * 默认三个日志的日志位于log/年-月下，日志名自动生成。默认文件名前缀是log5实例所在的类的名字（包括命名空间）加上error/debug以及年-月-日.log。
 * 例如：AT321.Interfaces.Service.Handler.Recommend-error-2012-08-13.log
 * 自动生成文件名的匹配规则（区分大小写）：
 * %src：原始文件名；例如：AT321.Interfaces.Service.Handler.Recommend-error。
 * %y：年（四位数字）
 * %M：月(固定2位）
 * %d：日（固定2位）
 * %H：小时（固定2位）
 * %m：分钟（固定2位）
 * %s：秒（固定2位）
 * %f：日志等级
 * 例如："%y-%M\\%src-%y-%M-%d.log"
 *
*********************************************************************/

namespace Utility
{
    [Serializable]
    public sealed class Log5 : IDisposable
    {
        private string _errfile = "";
        private string _normalfile = "";
        private string _debugfile = "";

        /// <summary>
        /// 默认错误日志路径
        /// </summary>
        public string ErrorFile
        {
            get
            {
                return _errfile;
            }
            set
            {
                _errfile = value;
                Log5Info info = GetLogInfo(_sys_err_log_name);
                if (info != null)
                    info.Logfile = _errfile;
            }
        }

        /// <summary>
        /// 默认普通日志路径
        /// </summary>
        public string NormalFile
        {
            get
            {
                return _normalfile;
            }
            set
            {
                _normalfile = value;
                Log5Info info = GetLogInfo(_sys_info_log_name);
                if (info != null)
                    info.Logfile = _normalfile;
            }
        }

        /// <summary>
        /// 默认调试日志路径
        /// </summary>
        public string DebugFile
        {
            get
            {
                return _debugfile;
            }
            set
            {
                _debugfile = value;
                Log5Info info = GetLogInfo(_sys_debug_log_name);
                if (info != null)
                    info.Logfile = _debugfile;
            }
        }

        public bool IsErrorEnabled { get; set; }
        public bool IsDebugEnabled { get; set; }
        public bool IsInfoEnabled { get; set; }

        /// <summary>
        /// 缓存大小，单位字节。
        /// </summary>
        public int BufferSize { get; set; } = 10 * 1024;

        /// <summary>
        /// 默认是否在日志中显示行号
        /// </summary>
        public bool ShowLineNo { get; set; }

        /// <summary>
        /// 默认是否在日志中显示文件名
        /// </summary>
        public ShowFileType ShowFile { get; set; }

        /// <summary>
        /// 默认是否在日志中显示方法名
        /// </summary>
        public bool ShowMethod { get; set; }

        /// <summary>
        /// 默认日志中的日期格式，例如yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string Dateformat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 默认的日志编码
        /// </summary>
        public System.Text.Encoding LogEncoding { get; set; } = System.Text.Encoding.Default;

        /// <summary>
        /// 错误日志名
        /// </summary>
        public const string _sys_err_log_name = "error(default)";

        /// <summary>
        /// 普通日志名
        /// </summary>
        public const string _sys_info_log_name = "info(default)";

        /// <summary>
        /// 调试日志名
        /// </summary>
        public const string _sys_debug_log_name = "debug(default)";

        private Dictionary<string, Log5Info> _loghash = new Dictionary<string, Log5Info>();

        private string _parentClassName;

        #region construction
        public Log5()
        {
            try
            {
                _parentClassName = null;
                InitVars();
                AddDefaultLogs();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 标准构造函数
        /// </summary>
        public Log5(string baseLogName)
        {
            try
            {
                _parentClassName = baseLogName;
                InitVars();
                AddDefaultLogs();
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 初始化默认值，可以从文件配置中读取
        /// </summary>
        private void InitVars()
        {
            Dateformat = "yyyy-MM-dd HH:mm:ss";
            LogEncoding = System.Text.Encoding.UTF8;
            IsDebugEnabled = false;
            ShowLineNo = true;
            ShowMethod = true;
            ShowFile = ShowFileType.none;
            BufferSize = 0;

            IsErrorEnabled = true;
#if DEBUG
            IsDebugEnabled = true;
#else
            IsDebugEnabled = false;
#endif
            IsInfoEnabled = true;
            var parentClassName = string.Empty;

            if (string.IsNullOrEmpty(this._parentClassName))
            {
                System.Diagnostics.StackTrace ss = new System.Diagnostics.StackTrace(false);
                System.Reflection.MethodBase mb = ss.GetFrame(2)?.GetMethod();
                if (mb != null)
                {
                    parentClassName = mb.DeclaringType?.FullName;
                    if (string.IsNullOrEmpty(parentClassName))
                    {
                        parentClassName = "UnknownFile" + ss.GetFrame(2)?.GetNativeOffset();
                    }

                }
                else
                {
                    System.Diagnostics.Trace.TraceError("Cannot get MethodBase");
                    throw new Exception("Cannot get MethodBase");
                }
            }
            else
            {
                parentClassName = this._parentClassName;
            }

            NormalFile = parentClassName;
            ErrorFile = parentClassName + "-error";
            DebugFile = parentClassName + "-debug";
        }

        #endregion


        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="err">日志文本</param>
        public void Error(string err)
        {
            Log5Info loginf = GetLogInfo(_sys_err_log_name);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif

            loginf.PushLogData(err);
        }

        /// <summary>
        /// 写含有参数的错误日志
        /// </summary>
        /// <param name="err">日志文本</param>
        /// <param name="args">参数</param>
        public void Error(string err, params Object[] args)
        {
            string lo = string.Format(err, args);
            Log5Info loginf = GetLogInfo(_sys_err_log_name);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif

            loginf.PushLogData(lo);
        }


        public void Info(string p)
        {
            Log5Info loginf = GetLogInfo(_sys_info_log_name);
            if (loginf == null)
            {
                System.Diagnostics.Trace.WriteLine($"LogInfo is not found in the following list ({_loghash.Count}):");
                foreach (var item in _loghash.Keys)
                {
                    System.Diagnostics.Trace.WriteLine("> " + item);
                }
#if DEBUG
                throw new Exception("LogInfo is not found. " + _sys_info_log_name);
#else
                return;
#endif
            }

            loginf.PushLogData(p);
        }

        public void Info(string p, params Object[] args)
        {
            string lo = string.Format(p, args);
            Log5Info loginf = GetLogInfo(_sys_info_log_name);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.PushLogData(lo);
        }

        /// <summary>
        /// 写调试日志
        /// </summary>
        /// <param name="debugstr">调试文本</param>
        public void Debug(string debugstr)
        {
            if (IsDebugEnabled)
            {
                Log5Info loginf = GetLogInfo(_sys_debug_log_name);
                if (loginf == null)
#if DEBUG
                    throw new Exception("LogInfo is not found.");
#else
                    return;
#endif

                loginf.PushLogData(debugstr);
            }
        }

        /// <summary>
        /// 写含有参数的调试日志
        /// </summary>
        /// <param name="debugstr">调试文本</param>
        /// <param name="args">参数</param>
        public void Debug(string debugstr, params object[] args)
        {
            if (IsDebugEnabled)
            {
                string lo = string.Format(debugstr, args);
                Log5Info loginf = GetLogInfo(_sys_debug_log_name);
                if (loginf == null)
#if DEBUG
                    throw new Exception("LogInfo is not found.");
#else
                    return;
#endif

                loginf.PushLogData(lo);
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logname">已创建的日志名称</param>
        /// <param name="str">日志文本</param>
        public void Log(string logname, string str)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif

            loginf.PushLogData(str);
        }

        /// <summary>
        /// 写带有参数的日志
        /// </summary>
        /// <param name="logname">已创建的日志名称</param>
        /// <param name="fmt">日志文本</param>
        /// <param name="args">参数</param>
        public void Log(string logname, string fmt, params object[] args)
        {
            string lo = string.Format(fmt, args);
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif

            loginf.PushLogData(lo);
        }


        /// <summary>
        /// 创建日志
        /// </summary>
        /// <param name="logname">日志名称</param>
        /// <param name="filepath">日志路径</param>
        /// <param name="showlineno">是否添加行号</param>
        /// <param name="showfile">是否添加文件名</param>
        /// <param name="buffsize">缓存大小</param>
        /// <param name="enc">日志编码</param>
        /// <param name="dateformat">日期格式</param>
        public void CreateLog(string logname, string filepath, bool showlineno = true, ShowFileType showfile = ShowFileType.none, int buffsize = -1, System.Text.Encoding enc = null, string dateformat = null)
        {
            Log5Info loginf = new Log5Info(logname, filepath)
            {
                BufferSize = buffsize < 0 ? this.BufferSize : buffsize,
                Dateformat = string.IsNullOrEmpty(dateformat) ? this.Dateformat : dateformat,
                LogEncoding = enc ?? System.Text.Encoding.UTF8,
                ShowFile = showfile,
                ShowLineNo = showlineno
            };
            if (_loghash.ContainsKey(loginf.Logname))
            {
#if DEBUG
                throw new Exception("The log is alreay exist.");
#else
                return;
#endif
            }
            else
                _loghash.Add(loginf.Logname, loginf);
        }

        /// <summary>
        /// 自动生成文件名
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="bauto">打开或关闭自动生成</param>
        /// <param name="dir">日志目录</param>
        /// <param name="autoname">文件名格式</param>
        public void SetAutoGenerateFilename(string logname, bool bAuto, string dir, string autoname)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif

            loginf.AutoGenerateFilename = bAuto;
            if (bAuto && !string.IsNullOrEmpty(autoname))
            {
                loginf.AutoFileDir = dir;
                loginf.AutofilenameExpr = autoname;
            }
        }

        /// <summary>
        /// 设置日志缓存大小，单位字节
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="newbuffersize">缓存大小</param>
        public void SetBufferSize(string logname, int newbuffersize)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.BufferSize = newbuffersize;
        }

        /// <summary>
        /// 设置日志是否显示行号
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="bshow">是否显示行号</param>
        public void SetShowLineNumber(string logname, bool bshow)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.ShowLineNo = bshow;
        }

        /// <summary>
        /// 设置日志是否显示文件路径
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="bshow">是否显示路径</param>
        public void SetShowFilePath(string logname, ShowFileType show)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.ShowFile = show;
        }

        /// <summary>
        /// 设置日志是否显示方法名
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="bshow">是否显示方法名</param>
        public void SetShowMethod(string logname, bool bshow)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.ShowMethod = bshow;
        }

        /// <summary>
        /// 设置新的日志路径
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="newfile">新路径</param>
        public void SetLogFile(string logname, string newfile)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.Logfile = newfile;
        }

        /// <summary>
        /// 设置日志的文件编码
        /// </summary>
        /// <param name="logname">已存在的日志名</param>
        /// <param name="enc">编码</param>
        public void SetLogEncoding(string logname, System.Text.Encoding enc)
        {
            Log5Info loginf = GetLogInfo(logname);
            if (loginf == null)
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            loginf.LogEncoding = enc;
        }

        /// <summary>
        /// 获取一个日志实体
        /// </summary>
        /// <param name="logname"></param>
        /// <returns></returns>
        private Log5Info GetLogInfo(string logname)
        {
            if (_loghash.ContainsKey(logname))
                return (Log5Info)_loghash[logname];
            else
                return null;
        }

        private void AddDefaultLogs()
        {
            // 预设三个日志。
            CreateLog(_sys_err_log_name, ErrorFile, true, ShowFileType.none);
            CreateLog(_sys_info_log_name, NormalFile, false, ShowFileType.none);
            CreateLog(_sys_debug_log_name, DebugFile, true, ShowFileType.filename);

            //System.Diagnostics.Trace.TraceWarning("Adding log " + _sys_err_log_name);
            //System.Diagnostics.Trace.TraceWarning("Adding log " + _sys_normal_log_name);
            //System.Diagnostics.Trace.TraceWarning("Adding log " + _sys_debug_log_name);

            SetAutoGenerateFilename(_sys_err_log_name, true, "log\\", "%y-%M\\%f\\%y-%M-%d-%src.log");
            SetAutoGenerateFilename(_sys_info_log_name, true, "log\\", "%y-%M\\%f\\%y-%M-%d-%src.log");
            SetAutoGenerateFilename(_sys_debug_log_name, true, "log\\", "%y-%M\\%f\\%y-%M-%d-%src.log");
        }

        /// <summary>
        /// IDispose接口
        /// </summary>
        public void Dispose()
        {
            foreach (var key in _loghash.Keys)
            {
                GC.SuppressFinalize((Log5Info)_loghash[key]);
            }
            System.Diagnostics.Trace.WriteLine("log disposed");
        }

        public class FileLocker : IDisposable
        {
            private static Hashtable fileTable = new Hashtable();

            public object CurrentLockObj = null;
            public FileLocker(string file)
            {
                if (string.IsNullOrEmpty(file))
                    return;
                lock (fileTable)
                {
                    CurrentLockObj = fileTable[file];
                    if (CurrentLockObj == null)
                    {
                        CurrentLockObj = new object();
                        fileTable[file] = CurrentLockObj;
                    }

                    Monitor.Enter(CurrentLockObj);
                }
            }

            public void Dispose()
            {
                if (CurrentLockObj == null)
                    return;
                System.Threading.Monitor.Exit(CurrentLockObj);

            }
        }

    }

    internal class Log5Info : IDisposable
    {
        private string _logfile = "";
        private string _logdir = "";
        /// <summary>
        /// 日志缓存
        /// </summary>
        // private System.Collections.Generic.Queue<string> _logQueue = new System.Collections.Generic.Queue<string>();
        private StringBuilder _logCache = new StringBuilder();
        private static readonly Hashtable _fileHandlerMap = new Hashtable();

        private Timer _timer = null;

        /// <summary>
        /// 日志名称
        /// </summary>
        public string Logname { get; set; }



        private int _bufferSize;

        /// <summary>
        /// 日志路径
        /// </summary>
        public string Logfile
        {
            get { return _logfile; }
            set
            {
                _logfile = ConvertPath(value);
            }
        }

        /// <summary>
        /// 缓存大小
        /// </summary>
        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                _bufferSize = value;
                if (value > 0 && _timer == null)
                {
                    _timer = new Timer(TimerFlush, null, 0, 5 * 1000);
                }
                else if (value == 0 && _timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        /// <summary>
        /// 是否显示行号
        /// </summary>
        public bool ShowLineNo { get; internal set; }

        /// <summary>
        /// 显示文件名，文件路径，或者不显示
        /// </summary>
        public ShowFileType ShowFile { get; internal set; }

        /// <summary>
        /// 是否显示方法名
        /// </summary>
        public bool ShowMethod { get; set; }

        /// <summary>
        /// 日期格式
        /// </summary>
        public string Dateformat { get; internal set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 是否自动生成文件名，当为true时，Logfile无效
        /// </summary>
        public bool AutoGenerateFilename { get; internal set; }

        /// <summary>
        /// 自动生成文件名的格式
        /// </summary>
        public string AutofilenameExpr { get; internal set; } = "";

        /// <summary>
        /// 自动生成文件名是，文件的目录
        /// </summary>
        public string AutoFileDir
        {
            get { return _logdir; }
            set
            {
                _logdir = ConvertPath(value);
            }
        }

        /// <summary>
        /// 日志编码
        /// </summary>
        public System.Text.Encoding LogEncoding { get; set; } = System.Text.Encoding.UTF8;

        public Log5Info()
        {

        }

        /// <summary>
        /// 构造一个日志
        /// </summary>
        /// <param name="logname">日志名称</param>
        /// <param name="logfile">日志路径</param>
        public Log5Info(string logname, string logfile) : this()
        {
            BufferSize = 0;
            ShowLineNo = true;
            ShowFile = ShowFileType.none;
            ShowMethod = true;
            Logname = logname;
            Logfile = logfile;
            AutoGenerateFilename = false;
            if (!Logfile.EndsWith(".log"))
            {
                Logfile += ".log";
            }

        }


        /// <summary>
        /// 构造一个日志
        /// </summary>
        /// <param name="logname">日志名称</param>
        /// <param name="logdir">日志目录</param>
        /// <param name="filename">日志文件名，不含路径</param>
        public Log5Info(string logname, string logdir, string filename) : this()
        {
            BufferSize = 0;
            ShowLineNo = true;
            ShowFile = ShowFileType.none;
            Logname = logname;
            Logfile = System.IO.Path.Combine(logdir, filename);
            AutoGenerateFilename = false;
            if (!Logfile.EndsWith(".log"))
            {
                Logfile += ".log";
            }
        }

        public void PushLogData(string str)
        {

            try
            {

                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ShowLineNo && ShowFile != ShowFileType.none);
                int lineno = 0;
                string file = null;
                string method = null;
                if (st.FrameCount >= 3)
                {
                    System.Diagnostics.StackFrame sf = st.GetFrame(2);
                    lineno = sf.GetFileLineNumber();
                    file = sf.GetFileName();
                    if (ShowMethod)
                        method = sf.GetMethod().Name;
                }
                lock (_logCache)
                {
                    _logCache.AppendFormat("[{0}]", DateTime.Now.ToString(Dateformat));
                    if (ShowLineNo)
                        _logCache.AppendFormat("[{0:0000}]", lineno);
                    if (ShowMethod)
                        _logCache.AppendFormat("[{0}]", method ?? "Method");
                    if (ShowFile != ShowFileType.none)
                    {
                        _logCache.AppendFormat("[{0}]",
                            file == null ? "filename" : (ShowFile == ShowFileType.filename ? System.IO.Path.GetFileName(file) : file));
                    }
                    _logCache.Append(str);


                    System.Diagnostics.Trace.WriteLine(str);

                }
                var task = CheckSize();


            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        public override int GetHashCode()
        {
            return Logname.GetHashCode();
        }


        async private Task CheckSize()
        {
            if (_logCache == null || _logCache.Length == 0)
                return;
            if (_logCache.Length > BufferSize)
            {
                // System.Threading.Tasks.Task.Run(() =>
                //   {
                // Thread.Sleep(10000);
                await Flush();
                //  });
            }

        }

        async private Task Flush(bool writeImmediatly = false)
        {

            string filename = Logfile;
            var logStr = "";
            lock (_logCache)
            {
                if (_logCache.Length == 0)
                    return;
                _logCache.Append("\r\n");
                logStr = _logCache?.ToString();
                _logCache.Clear();

            }

            if (string.IsNullOrEmpty(filename) || filename.EndsWith("\\"))
            {
                filename = Logname + ".log";
            }

            if (AutoGenerateFilename && !string.IsNullOrEmpty(AutofilenameExpr))
            {
                var logdate = DateTime.Now;
                filename = AutofilenameExpr.Replace("%f", Logname);
                filename = filename.Replace("%src", System.IO.Path.GetFileNameWithoutExtension(Logfile));
                filename = filename.Replace("%y", logdate.Year.ToString());
                filename = filename.Replace("%M", logdate.ToString("MM"));
                filename = filename.Replace("%d", logdate.ToString("dd"));
                filename = filename.Replace("%H", logdate.ToString("HH"));
                filename = filename.Replace("%m", logdate.ToString("mm"));
                filename = filename.Replace("%s", logdate.ToString("ss"));
                string dir = ConvertPath(AutoFileDir);
                filename = System.IO.Path.Combine(dir, filename);
                dir = System.IO.Path.GetDirectoryName(filename);

                System.IO.Directory.CreateDirectory(dir);
            }
            else
            {
                string dir = System.IO.Path.GetDirectoryName(Logfile);
                System.IO.Directory.CreateDirectory(dir);
            }
            //lock (_lock)
            StreamWriter sw = null;
            using (var fileLocker = new Log5.FileLocker(filename))
            {
                try
                {
                    sw = _fileHandlerMap[filename] as StreamWriter;
                    if (sw == null)
                    {
                        sw = new StreamWriter(filename, true, LogEncoding, 1024 * 10);

                        _fileHandlerMap[filename] = sw;
                    }
                }
                catch (Exception)
                {

                    throw;
                }

            }
            try
            {
                var textWriter = TextWriter.Synchronized(sw);

                if (writeImmediatly)
                {
                    textWriter.Write(logStr);
                    textWriter.Flush();
                    //  textWriter.BaseStream.Flush();
                }
                else
                {
                    await textWriter.WriteAsync(logStr);
                    await textWriter.FlushAsync();
                    //  await textWriter.BaseStream.FlushAsync();

                }
                // sw.Close();
                // _fileHandlerMap[filename]
                //   _fileHandlerMap.Remove(filename);

            }
            catch
            {
#if DEBUG
                throw;
#endif
            }

        }

        private void TimerFlush(object state)
        {
            var t = this.Flush(true);

        }

        private static string ConvertPath(string path)
        {
            string syspath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            if (string.IsNullOrEmpty(path)) // 默认当前路径
            {
                return syspath;
            }

            path = path.Replace('/', '\\');

            if (path.IndexOf(':') == 1) // 如果是绝对路径，无需转换
                return path;
            return syspath + (path.StartsWith("\\") ? path.Substring(1, path.Length - 1) : path);
        }

        public void Dispose()
        {
            var task = Flush(true);
            task.Wait();

            foreach (var key in _fileHandlerMap)
            {
                var sw = _fileHandlerMap[key] as StreamWriter;
                if (sw != null)
                {
                    try
                    {

                        sw.Close();
                    }
                    catch
                    {
                        continue;
                    }
                }
            }


        }
    }

    public enum ShowFileType
    {
        none = 0,
        filename = 1,
        filepath = 2,
    }
}