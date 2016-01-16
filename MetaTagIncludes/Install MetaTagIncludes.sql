-- Installs the OpenGraph, Dublin Core, and Twitter Card Includes as new IncludeTypes
-- This assumes that the code file has been simply copied to the App_Code folder under the webroot. 
-- If the class has been compiled into an assembly, substitute the Assembly name instead of 'App_Code'.
INSERT INTO IncludeType (Name, Type, Assembly, Description)
VALUES (
	N'OpenGraph meta tags',
	N'OpenGraphInclude',
	N'App_Code',
	N'Includes the OpenGraph meta tags on a page'
	);
