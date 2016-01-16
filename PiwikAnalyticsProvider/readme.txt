Piwik AnalyticsProvider Type

This file contains the necessary code to integrate a Piwik web analytics tracking on a OneWeb CMS site (7+) using the Analytics provider-based integration feature.

The code file can be copied directly into App_Code folder in the webroot, which will cause ASP.NET to automatically compile it.  Alternatively, it can be compiled into a standalone assembly and added to the bin folder.  The script file should be placed in the /core/scripts folder, or another folder as long as the path to script file is updated correctly (line 44 of PiwikAnalyticsProvider.vb).

In order to install the Analytics provider into the OneWeb CMS system, copy the the configuration for the provider below inside the <analytics >...</analytics> element:

	<analytics ...>

		...
		<!-- PiwikAnalyticsProvider 
					version			: "current" or "debug" - determine the version of ga.js code to load from Google (default is current)
					trackerUrl		: "http://..."		- sets the url of the non-secure tracking script to your Piwik server
					secureTrackerUrl: "https://..."		- sets the url of the secure tracking script to your Piwik server
					-->
		<provider mode="on" assembly="App_Code" type="PiwikAnalyticsProvider" version="current" trackerUrl="http://piwik" secureTrackerUrl="https://piwik" />
	</analytics>

	
Afterwards, access the Web Analytics tab of your site properties to set the numeric Piwik site id for tracking your site.