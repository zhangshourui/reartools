/********************************************************************
	created:	2019年9月30日
	filename: 	Common\Log5.cs
	file base:	Log5
	file ext:	cs
	author:		Cupid

	purpose:	写日志的类
 * 自动生成文件名的匹配规则（区分大小写）：
 * %src：原始文件名；例如：namespace.to.clazz。
 * %y：年（四位数字）
 * %M：月(固定2位）
 * %d：日（固定2位）
 * %H：小时（固定2位）
 * %m：分钟（固定2位）
 * %s：秒（固定2位）
 * 例如："%y-%M\\%cls-%y-%M-%d.log"
 *
*********************************************************************/

#pragma warning disable CA1034 // 嵌套类型应不可见
#pragma warning disable CA2000 // 丢失范围之前释放对象
#pragma warning disable IDE0068 // 使用建议的 dispose 模式

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Utility.Log5;

namespace Utility
{
    public sealed class Log5 : IDisposable
    {
        public static string BaseLogPath { get; set; } = System.IO.Path.DirectorySeparatorChar + "log";

        public bool IsDebugEnabled { get; set; } = false;

        public bool IsInfoEnabled { get; set; } = true;

        public bool IsErrorEnabled { get; set; } = true;

        private readonly string InfoLogPath;
        private readonly string WarningLogPath;
        private readonly string ErrorLogPath;
        private readonly string DebugLogPath;
        private readonly string ExceptionLogPath;

        private readonly Dictionary<string, LogEntry> _logEntryMap = new Dictionary<string, LogEntry>();
        private readonly string OwnerClassName = string.Empty;
        private static FileLocker FileLock = new FileLocker();

        private const int WriterReleaseTime = 60 * 1000;// writer过期时间，毫秒
        private const int WriterScanInterval = 10 * 1000;// writer过期时间，毫秒

        /// <summary>
        /// 缓存写入磁盘的最大等待时间，毫秒
        /// </summary>
        private const int FlushInterval = 1000 * 5;

        /// <summary>
        /// 标准构造函数
        /// </summary>
        public Log5()
        {
            InfoLogPath = System.IO.Path.Combine("%y-%M", "info", "%y-%M-%d-%cls-info.log");
            WarningLogPath = System.IO.Path.Combine("%y-%M", "warn", "%y-%M-%d-%cls-warn.log");
            ErrorLogPath = System.IO.Path.Combine("%y-%M", "error", "%y-%M-%d-%cls-error.log");
            DebugLogPath = System.IO.Path.Combine("%y-%M", "debug", "%y-%M-%d-%cls-debug.log");
            ExceptionLogPath = System.IO.Path.Combine("%y-%M", "exception", "%y-%M-%d-%cls-exception.log");


            try
            {
                var ss = new System.Diagnostics.StackTrace(false);
                var mb = ss.GetFrame(1).GetMethod();
                if (mb != null)
                {
                    OwnerClassName = mb.DeclaringType.FullName;
                }
                else
                {
                    OwnerClassName = "_unkown";
                }

                AddDefaultLogs();
            }
            catch (Exception)
            {
            }
        }

        private void AddDefaultLogs()
        {
            // 预设一些日志。
            var infoLogCfg = new LogEntryConfig() { FilePathTemplate = Path.Combine(BaseLogPath, InfoLogPath), OwnerClassName = OwnerClassName };
            var warningLogCfg = new LogEntryConfig() { FilePathTemplate = Path.Combine(BaseLogPath, WarningLogPath), OwnerClassName = OwnerClassName };
            var errorLogCfg = new LogEntryConfig() { FilePathTemplate = Path.Combine(BaseLogPath, ErrorLogPath), OwnerClassName = OwnerClassName };
            var debugLogCfg = new LogEntryConfig() { FilePathTemplate = Path.Combine(BaseLogPath, DebugLogPath), OwnerClassName = OwnerClassName };
            var expLogCfg = new LogEntryConfig() { FilePathTemplate = Path.Combine(BaseLogPath, ExceptionLogPath), CacheSize = 0, OwnerClassName = OwnerClassName };

            CreateLog(infoLogCfg);
            CreateLog(warningLogCfg);
            CreateLog(errorLogCfg);
            CreateLog(debugLogCfg);
            CreateLog(expLogCfg);
        }

        /// <summary>
        /// 创建日志
        /// </summary>
        /// <param name="cfg">日志配置</param>
        public void CreateLog(LogEntryConfig cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }
            if (cfg.FilePathTemplate == null)
            {
                throw new ArgumentNullException(nameof(cfg), "必须指定log路径或路径模板，字段：FilePathTemplate");
            }
            if (string.IsNullOrEmpty(cfg.OwnerClassName))
            {
                cfg.OwnerClassName = OwnerClassName;
            }

            var loginf = new LogEntry()
            {
                LogConfig = cfg,

                // LogName = cfg.FilePathTemplate,

                // Logfile = filePath,
            };

            lock (this)
            {
                try
                {
                    if (_logEntryMap.ContainsKey(cfg.FilePathTemplate))
                    {
#if DEBUG
                        throw new Exception("The log is alreay exist.");
#else
                        return;
#endif
                    }
                    else
                        _logEntryMap.Add(cfg.FilePathTemplate, loginf);
                }
                finally
                {
                }
            }
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="err">日志文本</param>
        /// <param name="args">参数</param>
        public void Error(string err, params object[] args)
        {
            if (IsErrorEnabled)
            {
                var newFilePath = Path.Combine(BaseLogPath, ErrorLogPath);
                Log(newFilePath, err, args);
            }
        }

        /// <summary>
        /// 写普通日志
        /// </summary>
        /// <param name="normalstr">日志文本</param>
        public void Info(string verboseContent, params object[] args)
        {
            if (IsInfoEnabled)
            {
                var newFilePath = Path.Combine(BaseLogPath, InfoLogPath);
                Log(newFilePath, verboseContent, args);
            }
        }

        /// <summary>
        /// 写调试日志
        /// </summary>
        /// <param name="normalstr">日志文本</param>
        public void Debug(string debugContent, params object[] args)
        {
            if (IsDebugEnabled)
            {
                var newFilePath = Path.Combine(BaseLogPath, DebugLogPath);
                Log(newFilePath, debugContent, args);
            }
        }

        /// <summary>
        /// 写警告日志
        /// </summary>
        /// <param name="normalstr">日志文本</param>
        public void Warn(string warnContent, params object[] args)
        {
            var newFilePath = Path.Combine(BaseLogPath, WarningLogPath);
            Log(newFilePath, warnContent, args);
        }
        /// <summary>
        /// 输出异常信息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="includeInnerException"></param>
        /// <param name="msg"></param>
        public void Exception(Exception exception, bool includeInnerException = true, string msg = null)
        {
            Exception(exception, includeInnerException, msg, null);
        }

        /// <summary>
        /// 输出异常信息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="includeInnerException"></param>
        /// <param name="msg"></param>
        public void Exception(Exception exception, params (string paramName, object paramValue)[] paramList)
        {
            Exception(exception, true, null, paramList);
        }
        /// <summary>
        /// 输出异常信息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="includeInnerException"></param>
        /// <param name="msg"></param>
        public void Exception(Exception exception, bool includeInnerException, string msg, params (string paramName, object paramValue)[] paramList)
        {
            var newFilePath = Path.Combine(BaseLogPath, ExceptionLogPath);
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(msg))
            {
                sb.AppendLine(msg);
            }
            if (paramList != null && paramList.Length > 0)
            {
                sb.AppendLine("----- PARAMETERS -----");

                foreach (var p in paramList)
                {
                    sb.Append(p.paramName);
                    sb.Append(" -> ");
                    sb.Append(p.paramValue);
                    sb.AppendLine();
                }
            }
            var ex = exception;
            while (ex != null)
            {
                sb.AppendLine("----- EXCEPTION -----");
                sb.AppendLine(ex.ToString());
                if (includeInnerException)
                {
                    ex = ex.InnerException;
                }
                else
                {
                    break;
                }
            }
            if (sb.Length > 0)
            {
                sb.Append('-', 25);// output: "-" * 25

                sb.Replace("\r\n", "\r\n    ");
                Log(newFilePath, sb.ToString());
            }
        }

        /// <summary>
        /// 输出内容到特定日志
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logContent"></param>
        /// <param name="args"></param>
        public void Log(string logName, string logContent, params object[] args)
        {
            if (string.IsNullOrEmpty(logContent))
            {
                return;
            }

            var loginf = GetLogInfo(logName);
            if (loginf == null)
            {
#if DEBUG
                throw new Exception("LogInfo is not found.");
#else
                return;
#endif
            }

            string content = args == null || args.Length == 0 ? logContent : string.Format(logContent, args);

            loginf.AppendLogContent(content);
        }

        /// <summary>
        /// 获取一个日志实体
        /// </summary>
        /// <param name="logname"></param>
        /// <returns></returns>
        private LogEntry GetLogInfo(string filePath)
        {
            if (_logEntryMap.ContainsKey(filePath))
            {
                return _logEntryMap[filePath];
            }
            else
            {
                CreateLog(new LogEntryConfig() { FilePathTemplate = filePath });
                return _logEntryMap[filePath];
            }
        }

        public static string FormatFileName(string templateStr, string clsName, DateTime logDate)
        {
            if (templateStr is null)
            {
                throw new ArgumentNullException(nameof(templateStr));
            }

            var realFilePath = templateStr.Replace("%cls", clsName);
            realFilePath = realFilePath.Replace("%y", logDate.Year.ToString())
                               .Replace("%M", logDate.ToString("MM"))
                               .Replace("%d", logDate.ToString("dd"))
                               .Replace("%H", logDate.ToString("HH"))
                               .Replace("%m", logDate.ToString("mm"))
                               .Replace("%s", logDate.ToString("ss"));
            return realFilePath;
        }

        public static string ConvertToAbsPath(string path)
        {
            string syspath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            if (string.IsNullOrEmpty(path)) // 默认当前路径
            {
                return syspath;
            }

            path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);

            if (path.IndexOf(':') == 1) // 如果是绝对路径，无需转换
                return path;
            return Path.Combine(syspath, (path.StartsWith(System.IO.Path.DirectorySeparatorChar) ? path.Substring(1, path.Length - 1) : path));
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
            if (_logEntryMap != null)
            {
                foreach (var pair in _logEntryMap)
                {
                    // GC.SuppressFinalize(this);
                    if (pair.Value != null)
                    {
                        pair.Value.Dispose();
                    }
                }
            }
        }

        internal class LogEntry : IDisposable
        {
            #region def & ctor

            public LogEntryConfig LogConfig
            {
                get => Config;
                set
                {
                    Config = value;
                    if (Config != null)
                    {
                        if (Config.CacheSize > 0 && _timer == null)
                        {
                            _timer = new Timer(TimerFlush, null, 0, FlushInterval);
                        }
                        else if (Config.CacheSize == 0 && _timer != null)
                        {
                            _timer.Dispose();
                            _timer = null;
                        }
                    }
                }
            }

            ///// <summary>
            ///// 日志名称
            ///// </summary>
            //public string LogName { get; set; }

            /// <summary>
            /// 日志路径
            /// </summary>
            public string LogID { get; set; }

            /// <summary>
            /// 日志缓存
            /// </summary>
            private StringBuilder _logCache = new StringBuilder();

            private Timer _timer = null;

            private LogEntryConfig Config;

            public LogEntry()
            {
            }

            #endregion def & ctor

            public void AppendLogContent(string str)
            {
                try
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(Config.ShowLineNo || Config.ShowFile != ShowFileType.None);
                    int lineno = 0;
                    string file = null;
                    string method = null;
                    if (Config.ShowMethod || Config.ShowLineNo || Config.ShowFile != ShowFileType.None)
                    {
                        var index = 2;
                        while (true)
                        {
                            try
                            {
                                var sf = st.GetFrame(index);
                                if (sf != null)
                                {
                                    var declareType = sf.GetMethod()?.DeclaringType;
                                    if (declareType != null)
                                    {
                                        //Utility.Log5
                                        if (!declareType.FullName.StartsWith($"{typeof(Utility.Log5).FullName}"))
                                        {
                                            lineno = sf.GetFileLineNumber();
                                            file = sf.GetFileName();
                                            if (Config.ShowMethod)
                                            {
                                                method = sf.GetMethod().Name;
                                            }

                                            break;
                                        }
                                    }
                                }

                                index++;
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                    }

                    lock (_logCache)
                    {
                        _logCache.AppendFormat("[{0}]", DateTime.Now.ToString(Config.Dateformat));
                        if (Config.ShowLineNo)
                            _logCache.AppendFormat("[{0:0000}]", lineno);
                        if (Config.ShowMethod)
                            _logCache.AppendFormat("[{0}]", method ?? "Method");
                        if (Config.ShowFile != ShowFileType.None)
                        {
                            _logCache.AppendFormat("[{0}]",
                                file == null ? "filename" : (Config.ShowFile == ShowFileType.FileName ? System.IO.Path.GetFileName(file) : file));
                        }
                        _logCache.AppendLine(str);

                        System.Diagnostics.Trace.WriteLine(str);
                    }
                    var task = CheckSize();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }
            }

            async private Task CheckSize()
            {
                if (_logCache == null || _logCache.Length == 0)
                    return;
                if (_logCache.Length >= LogConfig.CacheSize)
                {
                    await Flush().ConfigureAwait(false);
                }
            }

            async private Task Flush(bool writeImmediatly = false)
            {
                var logStr = "";
                lock (_logCache)
                {
                    if (_logCache.Length == 0)
                        return;
                    logStr = _logCache?.ToString();
                    _logCache.Clear();
                }
                string filePath = Config.FilePathTemplate;
                if (!filePath.EndsWith(".log"))
                {
                    filePath += ".log";
                }
                if (LogConfig.IsAuto)
                {
                    filePath = FormatFileName(filePath, Config.OwnerClassName, DateTime.Now);
                    filePath = ConvertToAbsPath(filePath);
                    var dir = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(dir);
                }
                else
                {
                    string dir = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(dir);
                }

                //lock (_lock)
                using (var fileWriter = FileLock.CaptureFile(filePath, out bool isNewFile, LogConfig.LogEncoding))
                {
                    try
                    {
                        var textWriter = TextWriter.Synchronized(fileWriter.Writer);

                        if (writeImmediatly)
                        {
                            if (isNewFile && !string.IsNullOrWhiteSpace(Config.FirstLine))
                            {
                                textWriter.Write(Config.FirstLine + "\r\n");
                            }
                            textWriter.Write(logStr);
                            textWriter.Flush();
                        }
                        else
                        {
                            if (isNewFile && !string.IsNullOrWhiteSpace(Config.FirstLine))
                            {
                                await textWriter.WriteAsync(Config.FirstLine + "\r\n").ConfigureAwait(false);
                            }
                            await textWriter.WriteAsync(logStr).ConfigureAwait(false);
                            await textWriter.FlushAsync().ConfigureAwait(false);
                        }
                    }
                    catch
                    {
#if DEBUG
                        throw;
#endif
                    }
                    finally
                    {
                    }
                }
            }

            private void TimerFlush(object state)
            {
                var t = this.Flush(true);
            }

            public void Dispose()
            {
                var task = Flush(true);
                task.Wait();
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        internal class FileLocker : IDisposable
        {
            private static IDictionary<string, FileShareWriter> FileWriterMap = new Dictionary<string, FileShareWriter>();
            private Timer _timer = null;

            public FileLocker()
            {
                _timer = new Timer(ScanInvalidFiles, null, WriterScanInterval, WriterScanInterval);
            }

            /// <summary>
            /// 获取一个文件，并锁定
            /// </summary>
            /// <param name="file"></param>
            /// <param name="LogEncoding"></param>
            /// <returns></returns>
            public FileShareWriter CaptureFile(string file, out bool isNewFile, Encoding LogEncoding = null)
            {
                if (string.IsNullOrEmpty(file))
                {
                    throw new ArgumentNullException(nameof(file));
                }
                if (LogEncoding == null)
                {
                    LogEncoding = Encoding.UTF8;
                }
                FileShareWriter fileLock = null;
                lock (FileWriterMap)
                {
                    isNewFile = !File.Exists(file);
                    if (!FileWriterMap.ContainsKey(file))
                    {
                        fileLock = new FileShareWriter(new StreamWriter(file, true, LogEncoding, 10 * 1024));

                        FileWriterMap.Add(file, fileLock);
                    }
                    else
                    {
                        fileLock = FileWriterMap[file];
                    }
                    fileLock.Enter();
                    return fileLock;
                }
            }

            public void Dispose()
            {
                lock (FileWriterMap)
                {
                    foreach (var pair in FileWriterMap)
                    {
                        pair.Value.Writer.Flush();
                        pair.Value.Writer.Close();
                        pair.Value.Dispose();
                    }
                }
                FileWriterMap = null; if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }

            /// <summary>
            /// 定时释放长时间没有更新的文件
            /// </summary>
            /// <param name="state"></param>
            private void ScanInvalidFiles(object state)
            {
                var invalidWriterKeyList = new List<string>();

                //不获取锁的情况下，先遍历一遍，找到过期的Wrter
                foreach (var pair in FileWriterMap)
                {
                    if ((DateTime.Now - pair.Value.LastCaptureDate).TotalMilliseconds > WriterReleaseTime)
                    {
                        invalidWriterKeyList.Add(pair.Key);
                    }
                }
                if (invalidWriterKeyList.Count > 0)
                {
                    // 获取锁，并释放过期的writer
                    lock (FileWriterMap)
                    {
                        foreach (var k in invalidWriterKeyList)
                        {
                            var v = FileWriterMap[k];
                            FileWriterMap.Remove(k);
                            v.Writer.Flush();
                            v.Writer.Close();
                            v.Dispose();
                        }
                    }
                }
            }

            internal class FileShareWriter : IDisposable
            {
                private object _lockObj = null;
                private int IsEnter = 0;

                public TextWriter Writer { get; set; }

                public DateTime LastCaptureDate { get; set; } = DateTime.Now;

                public FileShareWriter(TextWriter writer)
                {
                    Writer = writer;
                    _lockObj = Writer ?? throw new ArgumentNullException(nameof(writer));
                }

                public void Enter()
                {
                    lock (this)
                    {
                        LastCaptureDate = DateTime.Now;
                        Monitor.Enter(_lockObj);
                        Interlocked.Increment(ref IsEnter);
                    }
                }

                public void Dispose()
                {
                    if (_lockObj != null)
                    {
                        if (IsEnter >= 1)
                        {
                            Monitor.Exit(_lockObj);
                            Interlocked.Decrement(ref IsEnter);
                        }
                    }
                }
            }
        }

        public enum ShowFileType
        {
            None = 0,
            FileName = 1,
            FilePath = 2,
        }
    }

    public class LogEntryConfig
    {
        /// <summary>
        /// 是否根据FilePathTemplate自动生成文件
        /// </summary>
        public bool IsAuto { get; set; } = true;

        public int CacheSize { get; set; } = 4 * 1024;

        public ShowFileType ShowFile { get; set; } = ShowFileType.None;

        public bool ShowMethod { get; set; } = true;

        public bool ShowLineNo { get; set; } = true;

        /// <summary>
        /// 文件路径或者路径模板，当IsAuto为true时，按照FilePathTemplate自动生成路径；当IsAuto为false时，FilePathTemplate为最终文件路径。
        /// 当FilePathTemplate中包含“：”时，表示windows下的绝对路径。
        /// </summary>
        public string FilePathTemplate { get; set; }

        /// <summary>
        /// 每条日志前的日期格式
        /// </summary>
        public string Dateformat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 日志文件编码
        /// </summary>
        public Encoding LogEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 生成一个新的日志文件时，第一行的内容。不含日期前缀。通常为表格标题
        /// </summary>
        public string FirstLine { get; set; }

        internal string OwnerClassName = string.Empty;
    }
}

#pragma warning restore CA2000 // 丢失范围之前释放对象
#pragma warning restore IDE0068 // 使用建议的 dispose 模式
#pragma warning restore CA1034 // 嵌套类型应不可见