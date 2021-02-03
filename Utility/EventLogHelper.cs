using System.Diagnostics;

namespace Utility
{
	public class EventLogHelper
	{
		private const string m_eventLogSource = "AT321V21";

		private EventLogHelper()
		{
		}

		// Checks for the existing of an event source. Returns true if the event   
		// source exists; otherwise false is returned.  
		public static bool Exists(string eventSourceName)
		{
			return EventLog.Exists(eventSourceName);
		}

		// Creates an event source name for the windows application event log.  
		public static void CreateSource(string eventSourceName)
		{
			if (!EventLog.Exists(eventSourceName))
			{
				EventLog.CreateEventSource(eventSourceName, "Application");
			}
		}

		// Removes an event source name from the windows application event log.  
		public static void RemoveSource(string eventSourceName)
		{
			if (EventLog.Exists(eventSourceName))
			{
				EventLog.DeleteEventSource(eventSourceName, "Application");
			}
		}

		// Logs an error to the application log.  
		public static void LogError(string message)
		{
			LogEvent(m_eventLogSource, message, EventLogEntryType.Error);
		}

		// Logs a failure audit message to the application event log.  
		public static void LogFailureAudit(string message)
		{
			LogEvent(m_eventLogSource, message, EventLogEntryType.FailureAudit);
		}

		// Logs a sucess audit message to the application event log.  
		public static void LogSuccessAudit(string message)
		{
			LogEvent(m_eventLogSource, message, EventLogEntryType.SuccessAudit);
		}

		// Logs a warning message to the application event log.  
		public static void LogWarning(string message)
		{
			LogEvent(m_eventLogSource, message, EventLogEntryType.Warning);
		}

		// Logs an information message to the application event log.  
		public static void LogInformation(string message)
		{
			LogEvent(m_eventLogSource, message, EventLogEntryType.Information);
		}

		// Log an message to the Windows Application Event Log with a specified type.  
		private static void LogEvent(string eventLogSource, string message, EventLogEntryType eventLogEntryType)
		{
			EventLog.WriteEntry(eventLogSource, message, eventLogEntryType);
		}
	}
}
