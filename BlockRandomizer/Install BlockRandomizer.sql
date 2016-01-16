-- Installs the BlockRandomizer as a new IncludeType
-- This assumes that the code file has been simply copied to the App_Code folder under the webroot. 
-- If the class has been compiled into an assembly, substitute the Assembly name instead of 'App_Code'.
INSERT INTO IncludeType (Name, Type, Assembly, Description)
VALUES (
	N'Block Randomizer',
	N'BlockRandomizer',
	N'App_Code',
	N'Includes the Block Randomizer on a page to allow for randomizing the display of a set of content blocks'
	);
