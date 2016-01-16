Option Strict On
Option Explicit On 

Imports OWEnts = ISL.OneWeb4.Entities
Imports OWSys = ISL.OneWeb4.Sys
Imports ISL.OneWeb4.UI
Imports ISL.OneWeb4.UI.Components
Imports ISL.OneWeb4.UI.Components.Helpers.Extensions

''' Project	 : OneWeb7.Web
''' Class	 : OneWeb4.UI.Components.Analytics.PiwikAnalyticsProvider
''' <summary>
''' An AnalyticsProvider class that utilizes Piwik Analytics.
''' </summary>
''' <remarks>
''' The PiwikAnalyticsProvider has configuration settings that can
''' be utilized in the web.config:
'''     version: "current"|"debug" - determine the version of piwik.js code to load from the Piwik server (default is current)
'''trackerUrl
'''secureTrackerUrl
'''     forceSSL:              - ensure analytics tracking is done through SSL
''' </remarks>
Public Class PiwikAnalyticsProvider
	Implements OWSys.Analytics.IAnalyticsProvider


#Region "Private Members"

	Private _account As String = String.Empty
	Private _settings As Collections.Specialized.NameValueCollection
	Private _active As Boolean = False

	Private Const _name As String = "Piwik"
	Private Const _key As String = "PWK"
	Private Const _accountRegEx As String = "\d+(,\d+)*"

	Private Const _host As String = "//"

	Private Const _currScript As String = "/piwik.js"
	Private Const _debugScript As String = "/js/piwik.js"

	'Private Const _linkPluginScript As String = "linkid.js"

	Private Const _providerSript As String = "~/core/scripts/ow_piwik.js"
	Private Const _providerScriptObject As String = "OneWeb.Analytics.Piwik.Provider"

#End Region

#Region "Public Methods"

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Gets the name of the AnalyticsProvider
	''' </summary>
	''' <value></value>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2008-02-20	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Public ReadOnly Property Name() As String Implements OWSys.Analytics.IAnalyticsProvider.Name
		Get
			Return _name
		End Get
	End Property

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Gets the localized name of the AnalyticsProvider
	''' </summary>
	''' <value></value>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2008-02-20	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Public ReadOnly Property LocalizedName() As String Implements OWSys.Analytics.IAnalyticsProvider.LocalizedName
		Get
			Return Helpers.ResourceAccessor.GetText(_name, Me.GetType())
		End Get
	End Property

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Gets the key used for this provider's account string.
	''' </summary>
	''' <value></value>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2008-02-20	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Public ReadOnly Property Key() As String Implements OWSys.Analytics.IAnalyticsProvider.Key
		Get
			Return _key
		End Get
	End Property

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Gets the current account string of the analytics provider.
	''' </summary>
	''' <value></value>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2006-08-17	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Public Property AccountString() As String Implements OWSys.Analytics.IAnalyticsProvider.AccountString
		Get
			Return _account
		End Get
		Set(ByVal value As String)
			If IsValidAccountString(value) Then
				_account = value
			End If
		End Set
	End Property

	''' <summary>
	''' Gets a flag indicating if this provider needs OneWeb to generate the client-side analytics script
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	ReadOnly Property IsGeneratingClientScriptProvider() As Boolean Implements OWSys.Analytics.IAnalyticsProvider.IsGeneratingClientScriptProvider
		Get
			' return whether the provider is active
			Return _active
		End Get
	End Property

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' Initializes the settings of the AnalyticsProvider
	''' </summary>
	''' <param name="configSettings"></param>
	''' <remarks>
	''' </remarks>
	''' <history>
	''' 	[Travis Musika]	2008-02-20	Created
	''' </history>
	''' -----------------------------------------------------------------------------
	Public Sub Initialize(ByVal configSettings As System.Collections.Specialized.NameValueCollection) Implements OWSys.Analytics.IAnalyticsProvider.Initialize
		_settings = configSettings
	End Sub

	''' <summary>
	''' Allows the GA provider to attach to page lifecycle events.
	''' </summary>
	''' <param name="handler"></param>
	''' <remarks></remarks>
	Public Sub RegisterHttpHandler(ByVal handler As System.Web.IHttpHandler) Implements OWSys.Analytics.IAnalyticsProvider.RegisterHttpHandler
		' only attach if the current page is a sitepage, and the user is not logged on
		If TypeOf handler Is WebForms.SitePage Then
			If Not DirectCast(handler, WebForms.SitePage).IsLoggedOn AndAlso Me.AccountString.Length > 0 Then

				' attach to the page events to register the required analytics javascript code
				AddHandler DirectCast(handler, WebForms.SitePage).PreRender, AddressOf Page_PreRender

				' only attach to the page if the user is not logged on, and the account is valid
				_active = True
			End If
		End If
	End Sub

#End Region

#Region "Private Members"

	''' <summary>
	''' A method validating that the provider has a valid account string
	''' </summary>
	''' <returns></returns>
	''' <remarks></remarks>
	Private Function IsValidAccountString(ByVal text As String) As Boolean
		Return Not String.IsNullOrEmpty(text) AndAlso System.Text.RegularExpressions.Regex.IsMatch(text, _accountRegEx)
	End Function

	''' <summary>
	''' Attaches to the current page's PreRender method to add the necessary client script.
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	''' <remarks></remarks>
	Private Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs)

		Dim page As WebForms.SitePage = CType(sender, WebForms.SitePage)
		If page.Response.IsRequestBeingRedirected OrElse page.Response.SuppressContent OrElse page.Form Is Nothing Then Return ' make sure we're not redirecting, suppressing content, or not a form

		Dim s As New Specialized.NameValueCollection(_settings)
		Dim plugins As New Specialized.NameValueCollection()

		If Not String.IsNullOrEmpty(Me.AccountString) Then
			Dim pwkUrl As String

			' check the script version value
			If String.IsNullOrEmpty(s("version")) Then
				pwkUrl = If(page.Request.IsSecureConnection And Not String.IsNullOrEmpty(s("secureTrackerUrl")), s("secureTrackerUrl"), s("trackerUrl")) & _currScript
			Else
				Select Case s("version").ToLowerInvariant()
					Case "current" : pwkUrl = _currScript
					Case "debug" : pwkUrl = _debugScript
					Case Else : pwkUrl = _currScript
				End Select
				pwkUrl = If(page.Request.IsSecureConnection And Not String.IsNullOrEmpty(s("secureTrackerUrl")), s("secureTrackerUrl"), s("trackerUrl")) & pwkUrl
			End If

			' add the piwik script to the head script location and load asynchronously
			'scriptTag = String.Format("<script type=""text/javascript"" src=""{0}"" async=""async"" defer=""defer""></script>", gaUrl)
			'page.ClientScript.RegisterStartupScript(Me.GetType, "gainclude", scriptTag, False)
			If page.Header IsNot Nothing Then
				' insert the script into the header
				page.Header.Controls.Add(New ISL.OneWeb4.UI.WebControls.ScriptBlock() With
					{.DocumentSource = pwkUrl, .Async = True, .ID = "pwkinclude", .IncludeLocation = WebControls.ScriptIncludeLocation.Head})
			Else
				' no header element defined; insert the script at the top of the form control
				page.Form.Controls.AddAt(0, New ISL.OneWeb4.UI.WebControls.ScriptBlock() With
					{.DocumentSource = pwkUrl, .Async = True, .ID = "pwkinclude", .IncludeLocation = WebControls.ScriptIncludeLocation.Head})
			End If

			' add the custom google analytics code
			page.ClientScript.RegisterClientScriptInclude(Me.GetType, "ow_pwk", page.ResolveFileReference(_providerSript))

			Dim accounts As String() = Me.AccountString.Split(","c)
			Dim settings As String = Nothing
			If s.Count > 0 OrElse plugins.Count > 0 Then
				' create the json serializer for the analytics settings, using the custom Name-Value converter
				Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer
				serializer.RegisterConverters(New System.Web.Script.Serialization.JavaScriptConverter() {New Utility.NameValueJavaScriptConverter()})

				' close the plugins array if present
				If plugins.Count > 0 Then s("plugins") = serializer.Serialize(plugins)

				settings = serializer.Serialize(s)
			End If

			For Each account As String In accounts
				' add the initialization account code
				If settings IsNot Nothing Then
					page.ClientScript.RegisterStartupScript(Me.GetType, "pwkinit" & account, Analytics.AnalyticsManager.FormatAddProviderScript(_providerScriptObject, account, settings), True)
				Else
					page.ClientScript.RegisterStartupScript(Me.GetType, "pwkinit" & account, Analytics.AnalyticsManager.FormatAddProviderScript(_providerScriptObject, account), True)
				End If
			Next

		End If

	End Sub

#End Region

End Class

' Copyright 2014 Internet Solutions Ltd.