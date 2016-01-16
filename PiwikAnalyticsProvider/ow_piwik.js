// --------------------------------------------------------------------------------
// ow_piwik.js
// Travis Musika
// ISL Digital Marketing
// Contains functionality to provide client-side Piwik services.
// --------------------------------------------------------------------------------
///<reference path="ow_util.js">
///<reference path="ow_analytics.js">

OneWeb.Analytics.Piwik = {
/*
<!-- Piwik -->
<script type="text/javascript">
  var _paq = _paq || [];
  _paq.push(["trackPageView"]);
  _paq.push(["enableLinkTracking"]);

  (function() {
    var u=(("https:" == document.location.protocol) ? "https" : "http") + "://piwik/";
    _paq.push(["setTrackerUrl", u+"piwik.php"]);
    _paq.push(["setSiteId", "1"]);
    var d=document, g=d.createElement("script"), s=d.getElementsByTagName("script")[0]; g.type="text/javascript";
    g.defer=true; g.async=true; g.src=u+"piwik.js"; s.parentNode.insertBefore(g,s);
  })();
</script>
<!-- End Piwik Code -->// -- Google Analytics Tracking Code -- // 
}
*/
};
_paq = (typeof _paq == "undefined") ? [] : _paq; // define the asynchronous _paq method buffer, if not defined yet

// PiwikProvider definition - derives from base Provider
OneWeb.Analytics.Piwik.Provider = function () {
	OneWeb.Analytics.Provider.apply(this, arguments);
	var pTracker = null;

	this.getTracker = function () { return pTracker; }
	this.setTracker = function (tracker) { pTracker = tracker; }

	// set up a unique name for the tracker
	OneWeb.Analytics.Piwik.Provider.count = (OneWeb.Analytics.Piwik.Provider.count || 0) + 1;
	this.name = (OneWeb.Analytics.Piwik.Provider.count > 1) ? "pwk" + OneWeb.Analytics.Piwik.Provider.count : "";
};
(function (PiwikProvider) {
	var OA = OneWeb.Analytics;

	/*OneWeb.Analytics.addPreInitializer(function () {
		//_paq.push(["trackPageView"]);
	});*/

	PiwikProvider.addPreInitializer = OA.Provider.addPreInitializer;
	PiwikProvider.addPostInitializer = OA.Provider.addPostInitializer;

	PiwikProvider.prototype = new OA.Provider();
	PiwikProvider.prototype.constructor = PiwikProvider;
	PiwikProvider.prototype.initialize = function () {
		var name = this.name,
			settings = this.getSettings();

		var linkTracking = settings["linkTracking"];

		// create or utilize the async tracker
		_paq.push(["setTrackerUrl", (("https:" == document.location.protocol && settings["secureTrackerUrl"]) ? "https:" + settings["secureTrackerUrl"] : "http:" + settings["trackerUrl"]) + "/piwik.php"]);
		_paq.push(["setSiteId", this.getAccount()]);

	    // set the allowLinker if the autoLink is set
		if (linkTracking && linkTracking.length > 0) {
		    _paq.push(['enableLinkTracking']);
		}

		_paq.push(['setDownloadClasses', OA.TRACK_DOWNLOAD_CLASS]);
		_paq.push(['setLinkClasses', OA.TRACK_EXTERNAL_CLASS]);

	}
	PiwikProvider.prototype.track = function (address) {
	    if (address) 
	        _paq.push(['setDocumentTitle', address]);
    	_paq.push(['trackPageView']);
	}
	PiwikProvider.prototype.trackEvent = function () {
	    if (arguments.length < 2) {
	        throw "Piwik trackEvent requires at least 2 parameters";
	    }
	    if ("trackEvent" in Piwik.getTracker()) {
	        var method = (this.name.length > 0 ? this.name + "." : "") + "trackEvent";
	        switch (arguments.length) {
	            case 2: _paq.push([method, arguments[0], arguments[1]]); break;
	            case 3: _paq.push([method, arguments[0], arguments[1], arguments[2]]); break;
	            default: _paq.push([method, arguments[0], arguments[1], arguments[2], arguments[3]]);
	        }
	    } else{
	        // event tracking not supported yet
	        var address = (document.domain + "/" + arguments[0] + "/" + arguments[1] + "/" + arguments[2]).toLowerCase();
	        this.track(address);
	    }
	}
	PiwikProvider.prototype.trackSocial = function () {
		if (arguments.length < 2) {
			throw "Piwik Analytics trackSocial requires at least 2 parameters";
		}
		this.trackEvent(arguments);
	}

})(OneWeb.Analytics.Piwik.Provider);
