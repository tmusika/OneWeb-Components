Imports System
Imports System.Web.UI
Imports ISL.OneWeb4.UI
Imports ISL.OneWeb4.UI.Components.Includes
Imports ISL.OneWeb4.UI.Components.Templates
Imports ISL.OneWeb4.UI.WebControls
Imports ISL.OneWeb4.UI.Components.Helpers.Extensions


<Include(Description:="Randomly shows one out of several content blocks", AllowedPositions:=AllowedPosition.Before)> _
Public Class BlockRandomizer
	Inherits Web.UI.Control
	Implements IInclude

	Private _blocks As Collections.Generic.IEnumerable(Of ContentBlock)

	''' <summary>
	''' Gets or sets the names of the content blocks.  This must be a comma-separate list.
	''' </summary>
	''' <returns>String name attribute.</returns>
	<Applications.ApplicationParameter("Specify the block positions by name, separated with a comma.  Wildcards are supported.", "Block positions", "(eg., ""Main_1,Main_2,Main_3""", "^[^\s()<>@,;:\\""/\[\]{}?=]+(\s*,[^\s()<>@,;:\\""/\[\]{}?=]+)*$")> _
 Public Property ContentBlocks() As String

	''' <summary>
	''' Gets or sets the random group the random content should be a part of.
	''' </summary>
	''' <returns></returns>
	<Applications.ApplicationParameter("Optionally enter a label to coordinate with other randomizers for the same request", "Random group")> _
	Public Property RandomGroup() As String
		Get
			If Not Me.ViewState("RandomGroup") Is Nothing Then
				Return CStr(Me.ViewState("RandomGroup"))
			Else
				Return String.Empty
			End If
		End Get
		Set(ByVal Value As String)
			Me.ViewState("RandomGroup") = Value
		End Set
	End Property

	Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Init
		' add the error handler to the page object
		'AddHandler Me.Page.Error, AddressOf Page_Error

		If TypeOf Me.Page Is WebForms.SitePage Then

			' if the page is a site page, then call the application's cache control setting
			AddHandler DirectCast(Me.Page, WebForms.SitePage).SetCacheControlValues, AddressOf SetCacheControlValues
		End If

	End Sub

	''' <summary>
	''' Handles the PreRender method to set the header.
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Protected Overridable Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load

		' don't randomize content on admin page
		If TypeOf Me.Page Is WebForms.AdminPage OrElse Me.Page.IsPostBack Then Return

		If Not String.IsNullOrEmpty(Me.ContentBlocks) Then

			_blocks = Me.Page.FindAll(Of ContentBlock)(AddressOf BlockPredicate)

			If TypeOf Me.Page Is WebForms.admin Then
				' label randomizing blocks
				For i As Integer = 0 To _blocks.Count - 1
					Dim ovr As UserControls.Overlay = TryCast(_blocks(i).Template.FindControl(_blocks(i).Id & "_o"), UserControls.Overlay)
					If ovr IsNot Nothing Then
						Dim nmlbl As Control = ovr.FindControl("ow_blockName")
						If nmlbl IsNot Nothing Then nmlbl.Controls.Add(New LiteralControl("&nbsp;<em>{Block Randomizer}</em>"))
					End If
				Next

			Else

				If _blocks.Count > 1 Then

					' generate the random number
					Dim rnd As New ManagedRandom(Me.RandomGroup)

					' determine type of random number: integer or float
					Dim randNumber As Integer = rnd.Next(0, _blocks.Count)

					' hide all other blocks not including the randomly chosen block
					For i As Integer = 0 To _blocks.Count - 1
						' set the block type to empty so content doesn't load
						If CBool(i <> randNumber) Then
							_blocks(i).Visible = False
							_blocks(i).BlockType = BlockType.Empty
						End If
					Next
				End If

			End If

		End If
	End Sub

	''' <summary>
	''' Overridable method to override the http cache-control values of the page.
	''' </summary>
	''' <remarks>
	''' Set the cache-control value of the page to 'no-cache' if randomization is active
	''' </remarks>
	Protected Overridable Sub SetCacheControlValues(ByVal source As Object, ByVal args As Components.Utility.CacheControlEventArgs)

		' by default, set the caching of the page to not cached when randomizing is enabled
		If _blocks IsNot Nothing AndAlso _blocks.Count > 1 Then args.Cacheability = HttpCacheability.NoCache

	End Sub

	''' <summary>
	''' Overrides the Unload method to clear the managed random number generator.
	''' </summary>
	''' <param name="e"></param>
	Protected Overrides Sub OnUnload(ByVal e As System.EventArgs)
		ManagedRandom.Clear()
	End Sub

	Private Function BlockPredicate(block As ContentBlock) As Boolean
		Static blocks As String() = Me.ContentBlocks.Split(New Char() {","c, " "c}, StringSplitOptions.RemoveEmptyEntries)

		For Each position As String In blocks
			' return true if the position name matches and the block is visible and not empty
			If block.Position Like position AndAlso block.Visible AndAlso Not CBool(block.BlockType And BlockType.Empty) Then Return True
		Next
		Return False

	End Function

End Class
