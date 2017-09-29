//===============================================================================
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq.Expressions;

using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Design;

namespace Enferno.Public.Logging.Configuration
{
    /// <summary>
    /// Represents the configuration data for a <see cref="RollingXmlTraceListener"/>.
    /// </summary>	
    [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataDescription")]
    [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataDisplayName")]
    public class RollingXmlTraceListenerData : TraceListenerData
    {
        const string FileNamePropertyName = "fileName";
        const string RollFileExistsBehaviorPropertyName = "rollFileExistsBehavior";
        const string RollIntervalPropertyName = "rollInterval";
        const string RollSizeKBPropertyName = "rollSizeKB";
        const string TimeStampPatternPropertyName = "timeStampPattern";
        const string MaxArchivedFilesPropertyName = "maxArchivedFiles";

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceListenerData"/> class.
        /// </summary>
        public RollingXmlTraceListenerData()
            : base(typeof(RollingXmlTraceListener))
        {
            ListenerDataType = typeof(RollingXmlTraceListenerData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollingXmlTraceListenerData"/> class.
        /// </summary>
        /// <param name="name">The name for the configuration object.</param>
        /// <param name="traceOutputOptions">The trace options.</param>
        /// <param name="fileName"></param>
        /// <param name="rollSizeKB"></param>
        /// <param name="timeStampPattern"></param>
        /// <param name="rollFileExistsBehavior"></param>
        /// <param name="rollInterval"></param>
        public RollingXmlTraceListenerData(string name,
                                                string fileName,
                                                int rollSizeKB,
                                                string timeStampPattern,
                                                RollFileExistsBehavior rollFileExistsBehavior,
                                                RollInterval rollInterval,
                                                TraceOptions traceOutputOptions)
            : base(name, typeof(RollingXmlTraceListener), traceOutputOptions)
        {
            FileName = fileName;
            RollSizeKB = rollSizeKB;
            RollFileExistsBehavior = rollFileExistsBehavior;
            RollInterval = rollInterval;
            TimeStampPattern = timeStampPattern;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollingXmlTraceListenerData"/> class.
        /// </summary>
        /// <param name="name">The name for the configuration object.</param>
        /// <param name="traceOutputOptions">The trace options.</param>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="fileName"></param>
        /// <param name="rollSizeKB"></param>
        /// <param name="timeStampPattern"></param>
        /// <param name="rollFileExistsBehavior"></param>
        /// <param name="rollInterval"></param>
        public RollingXmlTraceListenerData(string name,
                                                string fileName,
                                                int rollSizeKB,
                                                string timeStampPattern,
                                                RollFileExistsBehavior rollFileExistsBehavior,
                                                RollInterval rollInterval,
                                                TraceOptions traceOutputOptions,
                                                SourceLevels filter)
            : base(name, typeof(RollingXmlTraceListener), traceOutputOptions, filter)
        {
            FileName = fileName;
            RollSizeKB = rollSizeKB;
            RollFileExistsBehavior = rollFileExistsBehavior;
            RollInterval = rollInterval;
            TimeStampPattern = timeStampPattern;
        }

        /// <summary>
        /// FileName
        /// </summary>
        [ConfigurationProperty(FileNamePropertyName, DefaultValue = "rolling.svclog")]
        [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataDisplayName")]
        [System.ComponentModel.Editor(CommonDesignTime.EditorTypes.FilteredFilePath, CommonDesignTime.EditorTypes.UITypeEditor)]
        [FilteredFileNameEditor(typeof(DesignResources), "LogFileDialogFilter", CheckFileExists = false)]
        public string FileName
        {
            get { return (string)this[FileNamePropertyName]; }
            set { this[FileNamePropertyName] = value; }
        }

        /// <summary>
        /// Exists Behavior
        /// </summary>
        [ConfigurationProperty(RollFileExistsBehaviorPropertyName)]
        [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataRollFileExistsBehaviorDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataRollFileExistsBehaviorDisplayName")]
        public RollFileExistsBehavior RollFileExistsBehavior
        {
            get { return (RollFileExistsBehavior)this[RollFileExistsBehaviorPropertyName]; }
            set { this[RollFileExistsBehaviorPropertyName] = value; }
        }

        /// <summary>
        /// Roll Interval
        /// </summary>
        [ConfigurationProperty(RollIntervalPropertyName)]
        [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataRollIntervalDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataRollIntervalDisplayName")]
        public RollInterval RollInterval
        {
            get { return (RollInterval)this[RollIntervalPropertyName]; }
            set { this[RollIntervalPropertyName] = value; }
        }

        /// <summary>
        /// Roll Size KB 
        /// </summary>
        [ConfigurationProperty(RollSizeKBPropertyName)]
        [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataRollSizeKBDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataRollSizeKBDisplayName")]
        public int RollSizeKB
        {
            get { return (int)this[RollSizeKBPropertyName]; }
            set { this[RollSizeKBPropertyName] = value; }
        }

        /// <summary>
        /// Time stamp
        /// </summary>
        [ConfigurationProperty(TimeStampPatternPropertyName, DefaultValue = "yyyy-MM-dd")]
        [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataTimeStampPatternDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataTimeStampPatternDisplayName")]
        public string TimeStampPattern
        {
            get { return (string)this[TimeStampPatternPropertyName]; }
            set { this[TimeStampPatternPropertyName] = value; }
        }

        /// <summary>
        /// Max rolled files
        /// </summary>
        [ConfigurationProperty(MaxArchivedFilesPropertyName)]
        [ResourceDescription(typeof(DesignResources), "RollingXmlTraceListenerDataMaxArchivedFilesDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RollingXmlTraceListenerDataMaxArchivedFilesDisplayName")]
        public int MaxArchivedFiles
        {
            get { return (int)this[MaxArchivedFilesPropertyName]; }
            set { this[MaxArchivedFilesPropertyName] = value; }
        }

        /// <summary>
        /// Returns a lambda expression that represents the creation of the trace listener described by this
        /// configuration object.
        /// </summary>
        /// <returns>A lambda expression to create a trace listener.</returns>
        protected Expression<Func<TraceListener>> GetCreationExpression()
        {
            return
                () =>
                    new RollingXmlTraceListener(
                        this.FileName,
                        this.RollSizeKB,
                        this.TimeStampPattern,
                        this.RollFileExistsBehavior,
                        this.RollInterval,
                        this.MaxArchivedFiles);
        }

        protected override TraceListener CoreBuildTraceListener(LoggingSettings settings)
        {
            return new RollingXmlTraceListener(
                FileName,
                RollSizeKB,
                TimeStampPattern,
                RollFileExistsBehavior,
                RollInterval,
                MaxArchivedFiles);
        }
    }
}
