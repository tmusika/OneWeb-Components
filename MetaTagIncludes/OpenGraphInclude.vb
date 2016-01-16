Imports System
Imports System.Web.UI
Imports ISL.OneWeb4.UI
Imports ISL.OneWeb4.UI.WebControls
Imports ISL.OneWeb4.UI.Components
Imports ISL.OneWeb4.UI.Components.Includes
Imports ISL.OneWeb4.UI.Components.Helpers.Extensions

<Include(Description:="Provides OpenGraph metatags; see <a href=""http://ogp.me/"" for details.", AllowedPositions:=AllowedPosition.Head)>
Public Class OpenGraphInclude
    Inherits Control
    Implements IInclude

    Private _tags As New Dictionary(Of String, HtmlMeta)

    'Private Const PathRegEx As String = "((https?://[\w\-.]+)|(~/)|/)?(\w|/(?!/)|-|_|\.|,|\+|\s)*[\w\-_\.,+]+(\?[^#\s]*)?(\#[\w\-\./%+]+)?"


    Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Init
        ' add the error handler to the page object
        'AddHandler Me.Page.Error, AddressOf Page_Error

        If TypeOf Me.Page Is WebForms.SitePage Then

            ' if the page is a site page, then call the application's cache control setting
            AddHandler DirectCast(Me.Page, WebForms.SitePage).SetCacheControlValues, AddressOf SetCacheControlValues

            ' create the metatags as necessary
            If Not String.IsNullOrEmpty(Me.SiteName) Then CreateMetaTag("SiteName")
            If Not String.IsNullOrEmpty(Me.PageUrl) Then CreateMetaTag("PageUrl")
            If Not String.IsNullOrEmpty(Me.ObjectType) Then CreateMetaTag("ObjectType")
            If Not String.IsNullOrEmpty(Me.Description) Then CreateMetaTag("Description")
            If Not String.IsNullOrEmpty(Me.ImageUrl) Then CreateMetaTag("ImageURL")
            If Not String.IsNullOrEmpty(Me.Title) Then CreateMetaTag("Title")

            ' create the xmlns for og
            If _tags.Count > 0 Then
                Me.Controls.Add(New DocXmlNS() With {.Name = "og", .Urn = "http://ogp.me/ns#"})
            End If

        End If

    End Sub

    ''' <summary>
    ''' Handles the PreRender method to set the metatag properties.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overridable Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.PreRender

        If Not String.IsNullOrEmpty(Me.SiteName) Then LoadMetaTag("SiteName", "og:site_name", Me.SiteName)
        If Not String.IsNullOrEmpty(Me.PageUrl) Then LoadMetaTag("PageUrl", "og:url", Me.PageUrl)
        If Not String.IsNullOrEmpty(Me.ObjectType) Then LoadMetaTag("ObjectType", "og:type", Me.ObjectType)
        If Not String.IsNullOrEmpty(Me.Description) Then LoadMetaTag("Description", "og:description", Me.Description)
        If Not String.IsNullOrEmpty(Me.ImageUrl) Then LoadMetaTag("ImageURL", "og:image", Me.ImageUrl)
        If Not String.IsNullOrEmpty(Me.Title) Then LoadMetaTag("Title", "og:title", Me.Title)

    End Sub

    ''' <summary>
    ''' Overridable method to override the http cache-control values of the page.
    ''' </summary>
    ''' <remarks>
    ''' Set the cache-control value of the page to 'public'
    ''' </remarks>
    Protected Overridable Sub SetCacheControlValues(ByVal source As Object, ByVal args As Utility.CacheControlEventArgs)

        ' set the cacheability to Public, as the content will be dependent on the page properties changing
        args.Cacheability = HttpCacheability.Public

    End Sub

    ''' <summary>
    ''' Creates the meta tag and adds it to the head element
    ''' </summary>
    ''' <param name="name"></param>
    Private Sub CreateMetaTag(name As String)
        Dim htmlMeta As New HtmlMeta()
        Me.Controls.Add(htmlMeta)
        _tags.Add(name, htmlMeta)
    End Sub

    ''' <summary>
    ''' Loads the properties of the metatags
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="[property]"></param>
    ''' <param name="value"></param>
    Private Sub LoadMetaTag(name As String, [property] As String, value As String)
        Dim htmlMeta As HtmlMeta = _tags(name)
        If htmlMeta Is Nothing Then
            htmlMeta = New HtmlMeta()
            Me.Page.Header.Controls.Add(htmlMeta)
        End If

        htmlMeta.Attributes("property") = [property]
        htmlMeta.Content = value
    End Sub


#Region "Public Properties"

    ''' <summary>
    ''' Gets or sets the og:site_name metatag.
    ''' control is on.
    ''' </summary>
    ''' <returns>String</returns>
    <Applications.CultureApplicationParameter("A human-readable name for the site (og:site_name)", "Site name", "(eg. OneWeb CMS)", ValueOptions:=Applications.ParameterValueOptions.AllowKeywords),
        Applications.DefaultValueAttribute("{Site.Name}")>
    Public Property SiteName() As String

    ''' <summary>
    ''' Gets or sets the og:title metatag.
    ''' control is on.
    ''' </summary>
    ''' <returns>String</returns>
    <Applications.CultureApplicationParameter("The title of your object as it should appear within the graph (og:title)", "Title", "(eg.  ""The Rock"")", ValueOptions:=Applications.ParameterValueOptions.AllowKeywords),
        Applications.DefaultValueAttribute("{Page.Heading}")>
    Public Property Title() As String

    ''' <summary>
    ''' Gets or sets the og:type metatag.
    ''' control is on.
    ''' </summary>
    ''' <returns>String</returns>
    <Applications.CultureApplicationParameter("The type of your object, e.g., ""video.movie"". Depending on the type you specify, other properties may also be required.", "Type", "(eg. Article)"),
        Applications.DefaultValueAttribute("article")>
    Public Property ObjectType() As String

    ''' <summary>
    ''' Gets or sets the og:url metatag.
    ''' </summary>
    ''' <returns>String</returns>
    <Applications.CultureApplicationParameter("The canonical URL of your object that will be used as its permanent ID in the graph", "Canonical URL", "(eg. ""http://www.imdb.com/title/tt0117500/"")", ValueOptions:=Applications.ParameterValueOptions.AllowKeywords),
        Applications.DefaultValueAttribute("http://localhost/{Site.Culture}{Page.Address}")>
    Public Property PageUrl() As String

    ''' <summary>
    ''' Gets or sets the site name metatag.
    ''' control is on.
    ''' </summary>
    ''' <returns>String</returns>
    <Applications.CultureApplicationParameter("An image URL which should represent your object within the graph.", "Image URL", "", ValueOptions:=Applications.ParameterValueOptions.AllowKeywords),
        Applications.DefaultValueAttribute("")>
    Public Property ImageUrl() As String

    ''' <summary>
    ''' Gets or sets the og:description metatag.
    ''' control is on.
    ''' </summary>
    ''' <returns>String</returns>
    <Applications.CultureApplicationParameter("A one to two sentence description of your object.", "Description", "", ValueOptions:=Applications.ParameterValueOptions.AllowKeywords),
        Applications.DefaultValueAttribute("{Page.Description}"),
        Applications.ApplicationParameterEditor(GetType(Applications.TextAreaEditor), "Rows=3&Columns=60&CssClass=isl_txt&Wrap=true&Height=5em")>
    Public Property Description() As String

#End Region


    <Serializable()>
    Public Enum ContentType
        Article
        Book
        Profile
        Website
    End Enum

End Class
