//===============================================================================
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

using Enferno.Public.Logging.Configuration;

namespace Enferno.Public.Logging
{
    [ConfigurationElementType(typeof(RollingXmlTraceListenerData))]
    public class RollingXmlTraceListener : XmlTraceListener
    {
        private const string ExtensionZipFile = ".zip";

        private readonly StreamWriterRollingHelper rollingHelper;

        private readonly RollFileExistsBehavior rollFileExistsBehavior;
        private readonly RollInterval rollInterval;
        private readonly int rollSizeInBytes;
        private readonly string timeStampPattern;
        private readonly int maxArchivedFiles;

        public RollingXmlTraceListener(string fileName)
            : base(fileName)
        {
        }

        public RollingXmlTraceListener(
                string fileName,
                int rollSizeKB,
                string timeStampPattern,
                RollFileExistsBehavior rollFileExistsBehavior,
                RollInterval rollInterval,
                int maxArchivedFiles) 
            : base (fileName)
        {
            this.rollSizeInBytes = rollSizeKB * 1024;
            this.timeStampPattern = timeStampPattern;
            this.rollFileExistsBehavior = rollFileExistsBehavior;
            this.rollInterval = rollInterval;
            this.maxArchivedFiles = maxArchivedFiles;

            this.rollingHelper = new StreamWriterRollingHelper(this);
        }

        /// <summary>
        /// Gets the <see cref="StreamWriterRollingHelper"/> for the file.
        /// </summary>
        /// <value>
        /// The <see cref="StreamWriterRollingHelper"/> for the file.
        /// </value>
        public StreamWriterRollingHelper RollingHelper
        {
            get { return rollingHelper; }
        }

        /// <summary>
        /// Writes trace information, a data object and event information to the file, performing a roll if necessary.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        public override void TraceData(TraceEventCache eventCache,
                                       string source,
                                       TraceEventType eventType,
                                       int id,
                                       object data)
        {
            rollingHelper.RollIfNecessary();

            var logEntry = data as Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry;

            if (logEntry != null && !string.IsNullOrWhiteSpace(logEntry.Message))
                source = logEntry.Message.Length > 80 ? logEntry.Message.Substring(0, 79) : logEntry.Message;

            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// A data time provider.
        /// </summary>
        public class DateTimeProvider
        {
            /// <summary>
            /// Gets the current data time.
            /// </summary>
            /// <value>
            /// The current data time.
            /// </value>
            public virtual DateTime CurrentDateTime
            {
                get { return DateTime.Now; }
            }
        }

        /// <summary>
        /// Encapsulates the logic to perform rolls.
        /// </summary>
        /// <remarks>
        /// If no rolling behavior has been configured no further processing will be performed.
        /// </remarks>
        public sealed class StreamWriterRollingHelper
        {
            DateTimeProvider dateTimeProvider;

            /// <summary>
            /// A tally keeping writer used when file size rolling is configured.<para/>
            /// The original stream writer from the base trace listener will be replaced with
            /// this listener.
            /// </summary>
            TallyKeepingFileStreamWriter managedWriter;

            DateTime? nextRollDateTime;

            /// <summary>
            /// The trace listener for which rolling is being managed.
            /// </summary>
            RollingXmlTraceListener owner;

            /// <summary>
            /// A flag indicating whether at least one rolling criteria has been configured.
            /// </summary>
            bool performsRolling;

            /// <summary>
            /// Initialize a new instance of the <see cref="StreamWriterRollingHelper"/> class with a <see cref="RollingXmlTraceListener"/>.
            /// </summary>
            /// <param name="owner">The <see cref="RollingXmlTraceListener"/> to use.</param>
            public StreamWriterRollingHelper(RollingXmlTraceListener owner)
            {
                this.owner = owner;
                dateTimeProvider = new DateTimeProvider();

                performsRolling = this.owner.rollInterval != RollInterval.None || this.owner.rollSizeInBytes > 0;
            }

            /// <summary>
            /// Gets the provider for the current date. Necessary for unit testing.
            /// </summary>
            /// <value>
            /// The provider for the current date. Necessary for unit testing.
            /// </value>
            public DateTimeProvider DateTimeProvider
            {
                set { dateTimeProvider = value; }
            }

            /// <summary>
            /// Gets the next date when date based rolling should occur if configured.
            /// </summary>
            /// <value>
            /// The next date when date based rolling should occur if configured.
            /// </value>
            public DateTime? NextRollDateTime
            {
                get { return nextRollDateTime; }
            }

            /// <summary>
            /// Calculates the next roll date for the file.
            /// </summary>
            /// <param name="dateTime">The new date.</param>
            /// <returns>The new date time to use.</returns>
            public DateTime CalculateNextRollDate(DateTime dateTime)
            {
                switch (owner.rollInterval)
                {
                    case RollInterval.Minute:
                        return dateTime.AddMinutes(1);
                    case RollInterval.Hour:
                        return dateTime.AddHours(1);
                    case RollInterval.Day:
                        return dateTime.AddDays(1);
                    case RollInterval.Week:
                        return dateTime.AddDays(7);
                    case RollInterval.Month:
                        return dateTime.AddMonths(1);
                    case RollInterval.Year:
                        return dateTime.AddYears(1);
                    case RollInterval.Midnight:
                        return dateTime.AddDays(1).Date;
                    default:
                        return DateTime.MaxValue;
                }
            }

            /// <summary>
            /// Checks whether rolling should be performed, and returns the date to use when performing the roll.
            /// </summary>
            /// <returns>The date roll to use if performing a roll, or <see langword="null"/> if no rolling should occur.</returns>
            /// <remarks>
            /// Defer request for the roll date until it is necessary to avoid overhead.<para/>
            /// Information used for rolling checks should be set by now.
            /// </remarks>
            public DateTime? CheckIsRollNecessary()
            {
                // check for size roll, if enabled.
                if (owner.rollSizeInBytes > 0
                    && (managedWriter != null && managedWriter.Tally > owner.rollSizeInBytes))
                {
                    return dateTimeProvider.CurrentDateTime;
                }

                // check for date roll, if enabled.
                DateTime currentDateTime = dateTimeProvider.CurrentDateTime;
                if (owner.rollInterval != RollInterval.None
                    && (nextRollDateTime != null && currentDateTime.CompareTo(nextRollDateTime.Value) >= 0))
                {
                    return currentDateTime;
                }

                // no roll is necessary, return a null roll date
                return null;
            }

            /// <summary>
            /// Gets the file name to use for archiving the file.
            /// </summary>
            /// <param name="actualFileName">The actual file name.</param>
            /// <param name="currentDateTime">The current date and time.</param>
            /// <returns>The new file name.</returns>
            public string ComputeArchiveFileName(string actualFileName, DateTime currentDateTime)
            {
                string directory = Path.GetDirectoryName(actualFileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(actualFileName);

                StringBuilder fileNameBuilder = new StringBuilder(fileNameWithoutExtension);
                if (!string.IsNullOrEmpty(owner.timeStampPattern))
                {
                    fileNameBuilder.Append('.');
                    fileNameBuilder.Append(currentDateTime.ToString(owner.timeStampPattern, CultureInfo.InvariantCulture));
                }

                if (owner.rollFileExistsBehavior == RollFileExistsBehavior.Increment)
                {
                    // look for max sequence for date
                    int newSequence = FindMaxSequenceNumber(directory, fileNameBuilder.ToString(), ExtensionZipFile) + 1;
                    fileNameBuilder.Append('.');
                    fileNameBuilder.Append(newSequence.ToString(CultureInfo.InvariantCulture));
                }

                fileNameBuilder.Append(ExtensionZipFile);

                return Path.Combine(directory, fileNameBuilder.ToString());
            }

            /// <summary>
            /// Finds the max sequence number for a log file.
            /// </summary>
            /// <param name="directoryName">The directory to scan.</param>
            /// <param name="fileName">The file name.</param>
            /// <param name="extension">The extension to use.</param>
            /// <returns>The next sequence number.</returns>
            public static int FindMaxSequenceNumber(string directoryName,
                                                    string fileName,
                                                    string extension)
            {
                string[] existingFiles = Directory.GetFiles(directoryName,
                                                            string.Format("{0}*{1}", fileName, extension));

                int maxSequence = 0;

                // Fix for issue #31932, not escaping filename: http://entlib.codeplex.com/workitem/31932
                Regex regex = new Regex(string.Format(@"{0}\.(?<sequence>\d+){1}", Regex.Escape(fileName), Regex.Escape(extension)));
                
                for (int i = 0; i < existingFiles.Length; i++)
                {
                    Match sequenceMatch = regex.Match(existingFiles[i]);
                    if (sequenceMatch.Success)
                    {
                        int currentSequence = 0;

                        string sequenceInFile = sequenceMatch.Groups["sequence"].Value;
                        if (!int.TryParse(sequenceInFile, out currentSequence))
                            continue; // very unlikely

                        if (currentSequence > maxSequence)
                        {
                            maxSequence = currentSequence;
                        }
                    }
                }

                return maxSequence;
            }

            static Encoding GetEncodingWithFallback()
            {
                Encoding encoding = (Encoding)new UTF8Encoding(false).Clone();
                encoding.EncoderFallback = EncoderFallback.ReplacementFallback;
                encoding.DecoderFallback = DecoderFallback.ReplacementFallback;
                return encoding;
            }

            /// <summary>
            /// Perform the roll for the next date.
            /// </summary>
            /// <param name="rollDateTime">The roll date.</param>
            public void PerformRoll(DateTime rollDateTime)
            {
                string actualFileName = ((FileStream)((StreamWriter)owner.Writer).BaseStream).Name;

                if (this.owner.rollFileExistsBehavior == RollFileExistsBehavior.Overwrite
                    && string.IsNullOrEmpty(this.owner.timeStampPattern))
                {
                    // no roll will be actually performed: no timestamp pattern is available, and 
                    // the roll behavior is overwrite, so the original file will be truncated
                    owner.Writer.Close();
                    File.WriteAllText(actualFileName, string.Empty);
                }
                else
                {
                    // calculate archive name
                    string archiveFileName = ComputeArchiveFileName(actualFileName, rollDateTime);
                    // close file
                    owner.Writer.Close();
                    // move file
                    SafeMove(actualFileName, archiveFileName, rollDateTime);
                    // purge if necessary
                    PurgeArchivedFiles(actualFileName);
                }

                // update writer - let TWTL open the file as needed to keep consistency
                owner.Writer = null;
                managedWriter = null;
                nextRollDateTime = null;
                UpdateRollingInformationIfNecessary();
            }

            /// <summary>
            /// Rolls the file if necessary.
            /// </summary>
            public void RollIfNecessary()
            {
                if (!performsRolling)
                {
                    // avoid further processing if no rolling has been configured.
                    return;
                }

                if (!UpdateRollingInformationIfNecessary())
                {
                    // an error was detected while handling roll information - avoid further processing
                    return;
                }

                DateTime? rollDateTime;
                if ((rollDateTime = CheckIsRollNecessary()) != null)
                {
                    PerformRoll(rollDateTime.Value);
                }
            }

            void SafeMove(string actualFileName, string archiveFileName, DateTime currentDateTime)
            {
                try
                {
                    if (File.Exists(archiveFileName))
                    {
                        File.Delete(archiveFileName);
                    }
                    // Take care of tunneling issues http://support.microsoft.com/kb/172190.
                    File.SetCreationTime(actualFileName, currentDateTime);
                    using (var zip = ZipFile.Open(archiveFileName, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(actualFileName, Path.GetFileName(actualFileName));
                    }
                    File.Delete(actualFileName);
                }
                catch (IOException)
                {
                    // Catch errors and attempt move to a new file with a GUID.
                    var directory = Path.GetDirectoryName(archiveFileName);
                    var fileName = Path.GetFileName(archiveFileName);
                    var guidArchiveFileName =
                        directory == null
                        ? Guid.NewGuid() + "_" + fileName
                        : Path.Combine(directory, Guid.NewGuid() + "_" + fileName);

                    try
                    {
                        using (var zip = ZipFile.Open(guidArchiveFileName, ZipArchiveMode.Create))
                        {
                            zip.CreateEntryFromFile(actualFileName, Path.GetFileName(actualFileName));
                        }
                        File.Delete(actualFileName);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            private void PurgeArchivedFiles(string actualFileName)
            {
                if (this.owner.maxArchivedFiles > 0)
                {
                    var directoryName = Path.GetDirectoryName(actualFileName);
                    var fileName = Path.GetFileName(actualFileName);
                    // Fix extension as we archive to zip.
                    fileName = Path.ChangeExtension(fileName, ExtensionZipFile);

                    new RollingFlatFilePurger(directoryName, fileName, this.owner.maxArchivedFiles).Purge();
                }
            }

            /// <summary>
            /// Updates bookeeping information necessary for rolling, as required by the specified
            /// rolling configuration.
            /// </summary>
            /// <returns>true if update was successful, false if an error occurred.</returns>
            public bool UpdateRollingInformationIfNecessary()
            {
                StreamWriter currentWriter = null;

                // replace writer with the tally keeping version if necessary for size rolling
                if (owner.rollSizeInBytes > 0 && managedWriter == null)
                {
                    currentWriter = owner.Writer as StreamWriter;
                    if (currentWriter == null)
                    {
                        // TWTL couldn't acquire the writer - abort
                        return false;
                    }
                    String actualFileName = ((FileStream)currentWriter.BaseStream).Name;

                    currentWriter.Close();

                    FileStream fileStream = null;
                    try
                    {
                        fileStream = File.Open(actualFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                        managedWriter = new TallyKeepingFileStreamWriter(fileStream, GetEncodingWithFallback());
                    }
                    catch (Exception)
                    {
                        // there's a slight chance of error here - abort if this occurs and just let TWTL handle it without attempting to roll
                        return false;
                    }

                    owner.Writer = managedWriter;
                }

                // compute the next roll date if necessary
                if (owner.rollInterval != RollInterval.None && nextRollDateTime == null)
                {
                    try
                    {
                        // casting should be safe at this point - only file stream writers can be the writers for the owner trace listener.
                        // it should also happen rarely
                        nextRollDateTime
                            = CalculateNextRollDate(File.GetCreationTime(((FileStream)((StreamWriter)owner.Writer).BaseStream).Name));
                    }
                    catch (Exception)
                    {
                        nextRollDateTime = DateTime.MaxValue; // disable rolling if not date could be retrieved.

                        // there's a slight chance of error here - abort if this occurs and just let TWTL handle it without attempting to roll
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Represents a file stream writer that keeps a tally of the length of the file.
        /// </summary>
        public sealed class TallyKeepingFileStreamWriter : StreamWriter
        {
            long tally;

            /// <summary>
            /// Initialize a new instance of the <see cref="TallyKeepingFileStreamWriter"/> class with a <see cref="FileStream"/>.
            /// </summary>
            /// <param name="stream">The <see cref="FileStream"/> to write to.</param>
            public TallyKeepingFileStreamWriter(FileStream stream)
                : base(stream)
            {
                tally = stream.Length;
            }

            /// <summary>
            /// Initialize a new instance of the <see cref="TallyKeepingFileStreamWriter"/> class with a <see cref="FileStream"/>.
            /// </summary>
            /// <param name="stream">The <see cref="FileStream"/> to write to.</param>
            /// <param name="encoding">The <see cref="Encoding"/> to use.</param>
            public TallyKeepingFileStreamWriter(FileStream stream,
                                                Encoding encoding)
                : base(stream, encoding)
            {
                tally = stream.Length;
            }

            /// <summary>
            /// Gets the tally of the length of the string.
            /// </summary>
            /// <value>
            /// The tally of the length of the string.
            /// </value>
            public long Tally
            {
                get { return tally; }
            }

            ///<summary>
            ///Writes a character to the stream.
            ///</summary>
            ///
            ///<param name="value">The character to write to the text stream. </param>
            ///<exception cref="T:System.ObjectDisposedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed. </exception>
            ///<exception cref="T:System.NotSupportedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream. </exception>
            ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>1</filterpriority>
            public override void Write(char value)
            {
                base.Write(value);
                tally += Encoding.GetByteCount(new char[] { value });
            }

            ///<summary>
            ///Writes a character array to the stream.
            ///</summary>
            ///
            ///<param name="buffer">A character array containing the data to write. If buffer is null, nothing is written. </param>
            ///<exception cref="T:System.ObjectDisposedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed. </exception>
            ///<exception cref="T:System.NotSupportedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream. </exception>
            ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>1</filterpriority>
            public override void Write(char[] buffer)
            {
                base.Write(buffer);
                tally += Encoding.GetByteCount(buffer);
            }

            ///<summary>
            ///Writes a subarray of characters to the stream.
            ///</summary>
            ///
            ///<param name="count">The number of characters to read from buffer. </param>
            ///<param name="buffer">A character array containing the data to write. </param>
            ///<param name="index">The index into buffer at which to begin writing. </param>
            ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
            ///<exception cref="T:System.ObjectDisposedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed. </exception>
            ///<exception cref="T:System.NotSupportedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream. </exception>
            ///<exception cref="T:System.ArgumentOutOfRangeException">index or count is negative. </exception>
            ///<exception cref="T:System.ArgumentException">The buffer length minus index is less than count. </exception>
            ///<exception cref="T:System.ArgumentNullException">buffer is null. </exception><filterpriority>1</filterpriority>
            public override void Write(char[] buffer,
                                       int index,
                                       int count)
            {
                base.Write(buffer, index, count);
                tally += Encoding.GetByteCount(buffer, index, count);
            }

            ///<summary>
            ///Writes a string to the stream.
            ///</summary>
            ///
            ///<param name="value">The string to write to the stream. If value is null, nothing is written. </param>
            ///<exception cref="T:System.ObjectDisposedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and current writer is closed. </exception>
            ///<exception cref="T:System.NotSupportedException"><see cref="P:System.IO.StreamWriter.AutoFlush"></see> is true or the <see cref="T:System.IO.StreamWriter"></see> buffer is full, and the contents of the buffer cannot be written to the underlying fixed size stream because the <see cref="T:System.IO.StreamWriter"></see> is at the end the stream. </exception>
            ///<exception cref="T:System.IO.IOException">An I/O error occurs. </exception><filterpriority>1</filterpriority>
            public override void Write(string value)
            {
                base.Write(value);
                tally += Encoding.GetByteCount(value);
            }
        }
    }
}
