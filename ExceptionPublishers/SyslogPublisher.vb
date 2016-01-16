' Class EmailPublisher
' Documentation:  Publishes an exception to an email address.

Option Explicit On 
Option Strict On

Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Text
Imports System.Collections.Specialized
Imports System.Reflection
Imports System.ComponentModel
Imports System.Configuration
Imports MSExMgmt = Microsoft.ApplicationBlocks.ExceptionManagement

Public Class SyslogPublisher
	Implements MSExMgmt.IExceptionPublisher

#Region "Private Members"

	Private _syslog As String
	Private _facilty As Facility
	Private _applicationName As String
	Private TEXT_SEPARATOR As String = "*********************************************"

#End Region

#Region "Public Methods"

	Public Overridable Sub Publish(ByVal exception As Exception,
						   ByVal additionalInfo As NameValueCollection,
						   ByVal configSettings As NameValueCollection) Implements MSExMgmt.IExceptionPublisher.Publish

		' initialize the settings
		InitializeSettings(configSettings)

		' Send the entry to the Syslog
		WriteToLog(exception, additionalInfo)

	End Sub

	Protected Overridable Sub InitializeSettings(ByVal configSettings As NameValueCollection)

		' Load Config values if they are provided.
		If configSettings IsNot Nothing Then
			If Not String.IsNullOrEmpty(configSettings("syslogServer")) Then
				_syslog = configSettings("syslogServer")
			Else
				' must have a syslog server
				Return
			End If

			If Not String.IsNullOrEmpty(configSettings("facility")) Then
				If Not System.Enum.TryParse(configSettings("facility"), _facilty) Then
					If IsNumeric(configSettings("facility")) Then
						_facilty = CType(CInt(configSettings("facility")), Facility)
					Else
						_facilty = Facility.Local0
					End If
				End If
			Else
				_facilty = Facility.Local0
			End If

			If Not String.IsNullOrEmpty(configSettings("applicationName")) Then
				_applicationName = configSettings("applicationName")
			Else
				' get the process name of the sender
				' the TAG of a syslog message must be full alpha-numeric
				_applicationName = System.Environment.GetCommandLineArgs()(0)
				_applicationName = _applicationName.Substring(_applicationName.LastIndexOf("\"c) + 1)
				_applicationName = _applicationName.Substring(0, _applicationName.IndexOf("."c))
				_applicationName = _applicationName.Replace("\W", "")
			End If
		End If

	End Sub	'IExceptionPublisher.Publish

#End Region

#Region "Protected Methods"

	' Helper function to write an entry to the log file.
	' Parameters:
	' -entry - The entry to enter into the Event Log. 
	Protected Overridable Sub WriteToLog(ByVal exception As Exception, ByVal additionalInfo As NameValueCollection)
		Dim client As New SyslogClient()
		If Not String.IsNullOrEmpty(_syslog) Then
			Dim server As String() = _syslog.Split(":"c)
			client.ServerIp = server(0)
			If server.Length > 1 AndAlso IsNumeric(server(1)) Then
				client.ServerPort = CInt(server(1))
			End If
		End If

		Dim message As SyslogMessage
		' send the entry to an email recipient
		Try
			message = New SyslogMessage
			message.Facility = _facilty
			message.Level = Level.Error

			message.Text = _applicationName & ": " & CreateLogEntry(exception, additionalInfo)
			client.Send(message)
		Catch
			Throw
		Finally
			client.Close()
		End Try
	End Sub	'WriteToLog

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Creates a string containing the exception information.
	''' </summary>
	''' <param name="exception"></param>
	''' <param name="additionalInfo"></param>
	''' <returns></returns>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2010-08-31	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Protected Overridable Function CreateLogEntry(ByVal exception As Exception, _
					   ByVal additionalInfo As NameValueCollection) As String


		' Create StringBuilder to maintain publishing information.
		Dim strInfo As New StringBuilder
		Dim currentException As exception
		Dim intExceptionCount As Integer = 1 ' Count variable to track the number of exceptions in the chain.
		Dim currentAdditionalInfo As NameValueCollection
		Dim aryPublicProperties As PropertyInfo()
		Dim p As PropertyInfo
		Dim i As String
		Dim j, k As Integer

		' Record the contents of the additionalInfo collection.
		If Not (additionalInfo Is Nothing) Then
			strInfo.AppendFormat("{0}General Information {0}{1}{0}Additional Info:", Environment.NewLine, TEXT_SEPARATOR)

			For Each i In additionalInfo
				strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, i, additionalInfo.Get(i))
			Next i
		End If

		' Append the exception text
		If exception Is Nothing Then
			strInfo.AppendFormat("{0}{0}No Exception.{0}", Environment.NewLine)
		Else
			' Loop through each exception class in the chain of exception objects.

			' Temp variable to hold InnerException object during the loop.
			currentException = exception '

			Do
				' Write title information for the exception object.
				strInfo.AppendFormat("{0}{0}{1}) Exception Information{0}{2}", Environment.NewLine, intExceptionCount.ToString(), TEXT_SEPARATOR)
				strInfo.AppendFormat("{0}Exception Type: {1}", Environment.NewLine, currentException.GetType().FullName)

				' Loop through the public properties of the exception object and record their value.
				aryPublicProperties = currentException.GetType().GetProperties()  '

				For Each p In aryPublicProperties
					' Do not log information for the InnerException or StackTrace. This information is 
					' captured later in the process.
					If p.Name <> "InnerException" And p.Name <> "StackTrace" And p.Name <> "TargetSite" Then
						Dim obj As Object = Nothing
						Try
							obj = p.GetValue(currentException, Nothing)
							If obj Is Nothing Then
								strInfo.AppendFormat("{0}{1}: NULL", Environment.NewLine, p.Name)
							Else
								' Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
								If p.Name = "AdditionalInformation" And TypeOf currentException Is MSExMgmt.BaseApplicationException Then
									' Verify the collection is not null.

									' Cast the collection into a local variable.
									currentAdditionalInfo = CType(obj, NameValueCollection)

									' Check if the collection contains values.
									If currentAdditionalInfo.Count > 0 Then
										strInfo.AppendFormat("{0}AdditionalInformation:", Environment.NewLine)

										' Loop through the collection adding the information to the string builder.
										k = currentAdditionalInfo.Count - 1
										For j = 0 To k
											strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, currentAdditionalInfo.GetKey(j), Me.ObjectToString(currentAdditionalInfo(j), 1))
											' strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, currentAdditionalInfo.GetKey(j), currentAdditionalInfo(j))
										Next
									End If

								Else
									' Otherwise just write the ToString() value of the property.
									strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, p.Name, Me.ObjectToString(obj, 1))
								End If
							End If
						Catch tex As System.Reflection.TargetInvocationException
							strInfo.AppendFormat("{0}{1}: {{exc:""{2}""}}", Environment.NewLine, p.Name, tex.InnerException.Message)
						Catch ex As exception
							If Not ex.InnerException Is Nothing Then
								strInfo.AppendFormat("{0}{1}: {{exc:""{2}"",""{3}""}}", Environment.NewLine, p.Name, ex.Message, ex.InnerException.Message)
							Else
								strInfo.AppendFormat("{0}{1}: {{exc:""{2}""}}", Environment.NewLine, p.Name, ex.Message)
							End If
						End Try
					End If
				Next p

				If Not currentException.TargetSite Is Nothing Then
					strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, "TargetSite", currentException.TargetSite.ToString())
				End If

				' Record the StackTrace with separate label.
				If Not (currentException.StackTrace Is Nothing) Then '
					strInfo.AppendFormat("{0}{0}StackTrace Information{0}{1}", Environment.NewLine, TEXT_SEPARATOR)
					strInfo.AppendFormat("{0}{1}", Environment.NewLine, currentException.StackTrace)
				End If

				' Reset the temp exception object and iterate the counter.
				currentException = currentException.InnerException
				intExceptionCount += 1
			Loop While Not (currentException Is Nothing)
		End If

		' add a couple blank lines
		strInfo.AppendFormat("{0}{0}{1}{0}{1}{0}{0}{0}", Environment.NewLine, TEXT_SEPARATOR)

		Return strInfo.ToString()
	End Function

	''' <summary>
	''' Provides more comprehensive ToString() information using reflection than the Object.ToString() method.
	''' </summary>
	''' <param name="obj"></param>
	''' <returns></returns>
	Protected Overridable Function ObjectToString(ByVal obj As Object, ByVal tier As Integer) As String
		Dim prop As System.Reflection.PropertyInfo
		Dim typ As System.Type
		Dim sb As New System.Text.StringBuilder

		If obj Is Nothing Then
			sb.Append(" NULL")
		Else
			typ = obj.GetType()
			sb.Append("{" & typ.FullName & "}")

			If TypeOf obj Is String OrElse TypeOf obj Is Date OrElse TypeOf obj Is TimeSpan OrElse typ.IsPrimitive() Then
				sb.Append(" " & obj.ToString())
			ElseIf tier > 5 Then
				' stop endless recursion
				sb.Append(" " & obj.ToString())
			ElseIf TypeOf obj Is DictionaryEntry Then
				'sb.Append(Environment.NewLine & Microsoft.VisualBasic.StrDup(tier, Microsoft.VisualBasic.ControlChars.Tab))
				sb.Append(" Key=" & CType(obj, DictionaryEntry).Key.ToString() & ": Value is " & ObjectToString(CType(obj, DictionaryEntry).Value, tier + 1))
			ElseIf TypeOf obj Is IEnumerable Then
				For Each subobj As Object In CType(obj, IEnumerable)
					sb.Append(Environment.NewLine & Microsoft.VisualBasic.StrDup(tier, Microsoft.VisualBasic.ControlChars.Tab))
					sb.Append(ObjectToString(subobj, tier + 1))
				Next
			ElseIf obj.ToString() <> obj.GetType().FullName Then
				sb.Append(" " & obj.ToString())
			Else
				' enumerate the properties
				sb.Append(Environment.NewLine)
				For Each prop In typ.GetProperties(Reflection.BindingFlags.Public Or Reflection.BindingFlags.Instance)
					Dim val As Object = prop.GetValue(obj, Nothing)
					sb.Append(Environment.NewLine & Microsoft.VisualBasic.StrDup(tier, Microsoft.VisualBasic.ControlChars.Tab))
					Try
						sb.Append(prop.Name & ":  " & ObjectToString(val, tier + 1))
					Catch ex As Exception
						sb.Append(prop.Name & ": error accessing ")
					End Try
				Next
			End If
		End If

		Return sb.ToString
	End Function

#End Region

	Public Class SyslogMessage

		Public Sub New()
		End Sub

		Public Sub New(facility As Integer, level As Integer, text As String)
			Me.Facility = facility
			Me.Level = level
			Me.Text = text
		End Sub

		Public Property Facility() As Integer

		Public Property Level() As Integer

		Public Property Text() As String
	End Class

	'''<summary>
	''' need this helper class to expose the Active propery of UdpClient
	''' </summary>
	Public Class UdpClientEx
		Inherits System.Net.Sockets.UdpClient

		Public Sub New()
			MyBase.New()
		End Sub

		Public Sub New(ipe As IPEndPoint)
			MyBase.New(ipe)
		End Sub

		Protected Overrides Sub Finalize()
			Try
				If Me.Active Then
					Me.Close()
				End If
			Finally
				MyBase.Finalize()
			End Try
		End Sub

		Public ReadOnly Property IsActive() As Boolean
			Get
				Return Me.Active
			End Get
		End Property
	End Class

	Public Class SyslogClient
		Private ipHostInfo As IPHostEntry
		Private ipAddress As IPAddress
		Private ipLocalEndPoint As IPEndPoint
		Private udpClient As UdpClientEx
		Private _ip As String = "127.0.0.1"	' default localhost
		Private _port As Integer = 514		' default syslog port

		Public Sub New()
			ipHostInfo = Dns.GetHostEntry(Dns.GetHostName())
			ipAddress = ipHostInfo.AddressList(0)
			ipLocalEndPoint = New IPEndPoint(ipAddress, 0)
			udpClient = New UdpClientEx(ipLocalEndPoint)
		End Sub

		Public ReadOnly Property IsActive() As Boolean
			Get
				Return udpClient.IsActive
			End Get
		End Property

		Public Property ServerIp() As String
			Get
				Return _ip
			End Get
			Set(value As String)
				If Not IsActive Then
					_ip = value
				End If
			End Set
		End Property

		Public Property ServerPort() As Integer
			Get
				Return _port
			End Get
			Set(value As Integer)
				_port = value
			End Set
		End Property

		Public Sub Send(message As SyslogMessage)
			If Not udpClient.IsActive Then
				udpClient.Connect(_ip, _port)
			End If
			If udpClient.IsActive Then
				Dim priority As Integer = message.Facility * 8 + message.Level
				Dim msg As String = System.[String].Format("<{0}>{1} {2} {3}", priority, DateTime.Now.ToString("MMM dd HH:mm:ss", Globalization.CultureInfo.GetCultureInfo("en-US")), ipLocalEndPoint.Address, message.Text)
				If msg.Length > 1024 Then msg = msg.Substring(0, 1024)
				Dim bytes As Byte() = System.Text.Encoding.ASCII.GetBytes(msg)
				udpClient.Send(bytes, bytes.Length)
			Else
				Throw New Exception("Syslog client Socket is not connected. Please set the SysLogServerIp property")
			End If
		End Sub

		Public Sub Close()
			If udpClient.IsActive Then
				udpClient.Close()
			End If
		End Sub

	End Class

	Public Enum Level
		Emergency = 0
		Alert = 1
		Critical = 2
		[Error] = 3
		Warning = 4
		Notice = 5
		Information = 6
		Debug = 7
	End Enum

	Public Enum Facility
		Kernel = 0
		User = 1
		Mail = 2
		Daemon = 3
		Auth = 4
		Syslog = 5
		Lpr = 6
		News = 7
		UUCP = 8
		Cron = 9
		Sec = 10
		Ftp = 11
		Ntp = 12
		Audit = 13
		Alert = 14
		Clock = 15
		Local0 = 16
		Local1 = 17
		Local2 = 18
		Local3 = 19
		Local4 = 20
		Local5 = 21
		Local6 = 22
		Local7 = 23
	End Enum

End Class
