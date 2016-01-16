' Class RayGunPublisher
' Documentation:  Publishes an exception to raygun.io 

Option Explicit On 
Option Strict On

Imports System.Reflection
Imports System.Text
Imports System.Collections.Specialized
Imports MSExMgmt = Microsoft.ApplicationBlocks.ExceptionManagement
Imports OWSys = ISL.OneWeb4.Sys
Imports Mindscape.Raygun4Net

Public Class RaygunPublisher
	Implements MSExMgmt.IExceptionPublisher

#Region "Private Members"

	Private _apiKey As String
	Private _applicationName As String
	Private _excludeErrorsFromLocal As Boolean = True

#End Region

#Region "Public Methods"

	Public Overridable Sub Publish(ByVal exception As Exception,
						   ByVal additionalInfo As NameValueCollection,
						   ByVal configSettings As NameValueCollection) Implements MSExMgmt.IExceptionPublisher.Publish

		' initialize the settings
		InitializeSettings(configSettings)

		' Send the entry to Raygun
		WriteToLog(exception, additionalInfo)

	End Sub

#End Region

#Region "Protected Methods"

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Called to initialize the settings for the exception publisher.
	''' </summary>
	''' <param name="configSettings"></param>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2010-08-31	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Protected Overridable Sub InitializeSettings(ByVal configSettings As NameValueCollection)

		' Load Config values if they are provided.
		If configSettings IsNot Nothing Then
			If Not String.IsNullOrEmpty(configSettings("apiKey")) Then
				_apiKey = configSettings("apiKey")
			Else
				' must have a recipient address
				Return
			End If

			If Not String.IsNullOrEmpty(configSettings("excludeErrorsFromLocal")) Then
				Boolean.TryParse(configSettings("excludeErrorsFromLocal"), _excludeErrorsFromLocal)
			End If

			If Not String.IsNullOrEmpty(configSettings("applicationName")) Then
				_applicationName = configSettings("applicationName")
			Else
				_applicationName = Threading.Thread.GetDomain().FriendlyName
			End If
		End If

	End Sub

	''' <summary>
	''' Write the log entry into the log.
	''' log publishers
	''' </summary>
	''' <param name="entry"></param>
	''' <remarks>
	''' </remarks>
	Protected Overridable Sub WriteToLog(ByVal exception As Exception, ByVal additionalInfo As NameValueCollection)
		Dim client As RaygunClient

		' use apikey from publisher config or standard Raygun config
		client = If(Not String.IsNullOrEmpty(_apiKey), New RaygunClient(_apiKey), New RaygunClient())

		' retrieve the name of the assembly
		Dim t As Type = Type.GetType("ISL.OneWeb4.UI.BaseHttpApplication, ISL.OneWeb4.UI.Web")
		If t IsNot Nothing Then
			Dim assem As System.Reflection.Assembly = t.Assembly
			Dim version As System.Version = assem.GetName().Version
			client.ApplicationVersion = version.ToString()
		End If

		Dim userInfo As New Messages.RaygunIdentifierMessage("anonymous")
		userInfo.UUID = _applicationName
		If System.Threading.Thread.CurrentPrincipal IsNot Nothing AndAlso TypeOf System.Threading.Thread.CurrentPrincipal Is OWSys.Security.OneWebPrincipal Then
			Dim principal As OWSys.Security.OneWebPrincipal = DirectCast(System.Threading.Thread.CurrentPrincipal, OWSys.Security.OneWebPrincipal)
			If TypeOf principal.Identity Is OWSys.Security.OneWebIdentity AndAlso principal.Identity.IsAuthenticated Then
				userInfo.IsAnonymous = False
				With CType(principal.Identity, OWSys.Security.OneWebIdentity)
					userInfo.Identifier = .UserName
					userInfo.Email = .User.Email
					userInfo.FullName = .User.FullName
					userInfo.FirstName = .User.FirstName
				End With
			End If
		End If

		Dim dict As Collections.IDictionary = additionalInfo.AllKeys.ToDictionary(Function(k As String) k, Function(k As String) additionalInfo(k))
		client.Send(exception, Nothing, dict, userInfo)

	End Sub

#End Region

End Class
