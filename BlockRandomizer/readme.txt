Block Randomizer  Include Type

This file contains the necessary code for an include that can randomize the display of content blocks on a OneWeb CMS site (7+) using the new Page Includes feature.  Note that this will prevent the page that the randomizer is on from being cacheable by the browser; otherwise the user will never get randomized content.

The code file can be copied directly into App_Code folder in the webroot, which will cause ASP.NET to automatically compile it.  Alternatively, it can be compiled into a standalone assembly and added to the bin folder.

In order to install the Include Type into the OneWeb CMS system, execute the sql script in the "Install BlockRandomizer.sql" file.

To use the include, include it on a page with the Page Includes feature, and configure the settings:

- Block positions: Specify a comma-separated list of block names (not case-sensitive) to randomly display.  You can use a wildcard (*) character to match multiple blocks.  

- Random Group: Optionally use a label if you need to coordinate the appearance of specific random items together.  This will ensure that multiple randomizers use the same value per page load.

