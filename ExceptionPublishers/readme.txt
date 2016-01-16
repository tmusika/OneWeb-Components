Exception Publishers

These two classes demonstrate how to add additional exception publishers to OneWeb.  Both code files can be copied directly into App_Code folder in the webroot, which will cause ASP.NET to automatically compile it.  Alternatively, it can be compiled into a standalone assembly and added to the bin folder.

The SyslogPublisher publishes exceptions to a syslog server.  Specify the syslogServer address and port number (separate by a colon), and optionally set the "facility" and "applicationName" values.  The SyslogPublisher does not have any external dependencies, and can run on ASP.NET 2+ (OneWeb 5+).

	<exceptionManagement mode="on">
			
		<!-- Syslog Provider 
			syslogServer	: the address and (optionally) port for the syslog server
			facility		: the facility with which to log the excetions under
			applicationName	: The name of the application, which is used as part of the message body
			-->
		<publisher mode="on" assembly="App_Code" type="SyslogPublisher" syslogServer="localhost" applicationName="onewebcms.com" facility="Local0" />


The RaygunPublisher publishes exceptions to raygun.io.  Set the "apiKey" to your key, and the "applicationName" as desired.  The class depends on version 5+ of Mindscape.Raygun4Net and Mindscape.Raygun4Net4, which can be downloaded through Nuget or https://github.com/MindscapeHQ/raygun4net

	<exceptionManagement mode="on">
		
		<!-- Raygun.io Provider
			apiKey			: the apikey provided by raygun.io for your application
			applicationName	: The name of the application.  This sets the UUID for the exception
			-->
		<publisher mode="on" assembly="App_Code" type="RaygunPublisher" apiKey="myRaygunApiKey" applicationName="onewebcms.com" />

	</exceptionManagement>
