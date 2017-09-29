// <copyright file="RollingXmlWriterTraceListener.cs" license="GNU Lesser General Public License v3" />
// <author>Marc Wittke</author>
// <email>marc@wittke-web.SpamBotGoAway.de</email>
// <date>2008-11-13</date>
// <summary>Contains the RollingXmlWriterTraceListener class.</summary>
namespace Enferno.Public.Logging
{
    #region usings
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// An extended XmlWriterTraceListener that starts a new file after a configured trace file size. Trace files will be numbered with a four character number suffix.
    /// <example>
    ///     <code>
    ///         <sharedListeners>
    ///             <add type="MarcW.Tools.Diagnostics.RollingXmlWriterTraceListener, MarcW.Tools" name="System.ServiceModel.XmlTrace.Listener" traceOutputOptions="None" initializeData="C:\Logs\MyTraceFileName.svclog" MaxTraceFileSize="1048576" />
    ///         </sharedListeners>
    ///     </code>
    /// </example>
    /// </summary>
    [HostProtection(Synchronization = true)]
    [Obsolete("This one is for Diagnostics. Use RollingXmlTraceListener insted. It for EntLib.logging.")]
    public class RollingXmlWriterTraceListener : XmlWriterTraceListener
    {
        #region private fields
        /// <summary>
        /// This is the named capture group to find the numeric suffix of a trace file
        /// </summary>
        private static readonly string LogFileNumberCaptureName = "LogFileNumber";

        /// <summary>
        /// This field will be used to remember whether or not we have loaded the custom attributes from the config yet. The 
        /// initial value is, of course, false.
        /// </summary>
        private bool attributesLoaded = false;

        /// <summary>
        /// This expression is used to find the number of a trace file in its file name by searching for an underscore (_), a
        /// numeric expression with any repetitions and a dot (that marks the beginning of the file extension). The named 
        /// capture group named by the constant &quot;LogFileNumberCaptureName&quot; will contain the number.
        /// </summary>
        private Regex logfileSuffixExpression = new Regex(@"_(?<" + LogFileNumberCaptureName + @">\d*)\.", RegexOptions.Compiled);

        /// <summary>
        /// The current numeric suffix for trace file names
        /// </summary>
        private int currentFileSuffixNumber = 0;

        /// <summary>
        /// The size in bytes of a trace file before a new file is started. The default value is 128 Mbytes
        /// </summary>
        private long maxTraceFileSize = 128 * 1024 * 1024;

        /// <summary>
        /// The basic trace file name as it is configured in configuration file's system.diagnostics section. However, this
        /// class will append a numeric suffix to the file name (respecting the original file extension).
        /// </summary>
        private string basicTraceFileName = String.Empty;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RollingXmlWriterTraceListener"/> class by specifying the trace file
        /// name.
        /// </summary>
        /// <param name="filename">The trace file name.</param>
        public RollingXmlWriterTraceListener(string filename)
            : base(filename)
        {
            this.basicTraceFileName = filename;
            this.currentFileSuffixNumber = this.GetTraceFileNumber();
            this.StartNewTraceFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollingXmlWriterTraceListener"/> class by specifying the trace file
        /// name and the name of the new instance.
        /// </summary>
        /// <param name="filename">The trace file name.</param>
        /// <param name="name">The name of the new instance.</param>
        public RollingXmlWriterTraceListener(string filename, string name)
            : base(filename, name)
        {
            this.basicTraceFileName = filename;
            this.StartNewTraceFile();
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets the name of the current trace file. It is combined from the configured trace file plus an increasing number
        /// </summary>
        /// <value>The name of the current trace file.</value>
        public string CurrentTraceFileName
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(this.basicTraceFileName),
                    Path.GetFileNameWithoutExtension(this.basicTraceFileName) + "_" + this.currentFileSuffixNumber.ToString().PadLeft(4, '0') + Path.GetExtension(this.basicTraceFileName));
            }
        }

        /// <summary>
        /// Gets or sets the maximum size of the trace file.
        /// </summary>
        /// <value>The maximum size of the trace file.</value>
        public long MaxTraceFileSize
        {
            get
            {
                if (!this.attributesLoaded)
                {
                    this.LoadAttributes();
                }

                return this.maxTraceFileSize;
            }

            set
            {
                if (!this.attributesLoaded)
                {
                    this.LoadAttributes();
                }

                this.maxTraceFileSize = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the condition to roll over the trace file is reached.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the condition to roll over the trace file is reached; otherwise, <c>false</c>.
        /// </value>
        protected bool IsRollingConditionReached
        {
            get
            {
                // go down to the file stream
                StreamWriter streamWriter = (StreamWriter)this.Writer;
                FileStream fileStream = (FileStream)streamWriter.BaseStream;
                string traceFileName = fileStream.Name;

                FileInfo traceFileInfo = new FileInfo(traceFileName);

                if (traceFileInfo.Length > this.MaxTraceFileSize)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Emits an error message to the listener.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        public override void Fail(string message)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.Fail(message);
        }

        /// <summary>
        /// Emits an error message and a detailed message to the listener.
        /// </summary>
        /// <param name="message">The error message to write.</param>
        /// <param name="detailMessage">The detailed error message to append to the error message.</param>
        public override void Fail(string message, string detailMessage)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.Fail(message, detailMessage);
        }

        /// <summary>
        /// Writes trace information, a data object, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">A data object to emit.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Writes trace information, data objects, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An array of data objects to emit.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <PermissionSet>
        ///     <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///     <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode"/>
        /// </PermissionSet>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.TraceEvent(eventCache, source, eventType, id);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Writes trace information, a formatted message, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items that correspond to objects in the <paramref name="args"/> array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        /// <summary>
        /// Writes trace information including the identity of a related activity, a message, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A trace message to write.</param>
        /// <param name="relatedActivityId">A <see cref="T:System.Guid"/> structure that identifies a related activity.</param>
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
        }

        /// <summary>
        /// Writes the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        public override void Write(object o)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.Write(o);
        }

        /// <summary>
        /// Writes a category name and the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(object o, string category)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.Write(o, category);
        }

        /// <summary>
        /// Writes a verbatim message without any additional context information to the file or stream.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void Write(string message)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.Write(message);
        }

        /// <summary>
        /// Writes a category name and a message to the listener.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(string message, string category)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.Write(message, category);
        }

        /// <summary>
        /// Writes the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        public override void WriteLine(object o)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.WriteLine(o);
        }

        /// <summary>
        /// Writes a category name and the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(object o, string category)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.WriteLine(o, category);
        }

        /// <summary>
        /// Writes a verbatim message without any additional context information followed by the current line terminator to the file or stream.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.WriteLine(message);
        }

        /// <summary>
        /// Writes a category name and a message to the listener, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(string message, string category)
        {
            if (this.IsRollingConditionReached)
            {
                this.StartNewTraceFile();
            }

            base.WriteLine(message, category);
        }

        #endregion

        #region protected methods
        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>
        /// A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.
        /// </returns>
        protected override string[] GetSupportedAttributes()
        {
            return new string[1] { "MaxTraceFileSize" };
        }
        #endregion

        #region private methods
        /// <summary>
        /// Causes the writer to start a new trace file with an increased number in the file names suffix
        /// </summary>
        private void StartNewTraceFile()
        {
            // get the underlying file stream
            StreamWriter streamWriter = (StreamWriter)this.Writer;
            FileStream fileStream = (FileStream)streamWriter.BaseStream;

            // close it
            fileStream.Close();

            // increase the suffix number
            this.currentFileSuffixNumber++;

            // create a new file stream and a new stream writer and pass it to the listener
            this.Writer = new StreamWriter(new FileStream(this.CurrentTraceFileName, FileMode.Create));
        }

        /// <summary>
        /// Gets the trace file number by checking whether similar trace files are already existant. The method will find the latest trace 
        /// file and return its number.
        /// </summary>
        /// <returns>The number of the latest trace file</returns>
        private int GetTraceFileNumber()
        {
            string directoryName = Path.GetDirectoryName(this.basicTraceFileName);
            string basicTraceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.basicTraceFileName);
            string basicTraceFileNameExtension = Path.GetExtension(this.basicTraceFileName);
            string[] existingLogFiles = Directory.GetFiles(directoryName, basicTraceFileNameWithoutExtension + "*");

            int highestNumber = -1;
            foreach (string existingLogFile in existingLogFiles)
            {
                Match match = this.logfileSuffixExpression.Match(existingLogFile);
                if (match != null)
                {
                    int tempInt;
                    if (match.Groups.Count >= 1 && int.TryParse(match.Groups[LogFileNumberCaptureName].Value, out tempInt) && tempInt >= highestNumber)
                    {
                        highestNumber = tempInt;
                    }
                }
            }

            return highestNumber;
        }

        /// <summary>
        /// Reads the custom attributes' values from the configuration file. We call this method the first time the attributes
        /// are accessed.
        /// <remarks>We do not do this when the listener is constructed becausethe attributes will not yet have been read 
        /// from the configuration file.</remarks>
        /// </summary>
        private void LoadAttributes()
        {
            if (Attributes.ContainsKey("MaxTraceFileSize") && !String.IsNullOrEmpty(Attributes["MaxTraceFileSize"]))
            {
                long tempLong = 0;
                string attributeValue = Attributes["MaxTraceFileSize"];

                if (long.TryParse(attributeValue, out tempLong))
                {
                    this.maxTraceFileSize = long.Parse(Attributes["MaxTraceFileSize"], NumberFormatInfo.InvariantInfo);
                }
                else
                {
                    throw new ConfigurationErrorsException(String.Format("Trace listener {0} has an unparseable configuration attribute \"MaxTraceFileSize\". The value \"{1}\" cannot be parsed to a long value.", this.Name, attributeValue));
                }
            }

            this.attributesLoaded = true;
        }
        #endregion
    }
}