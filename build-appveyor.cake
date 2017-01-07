Task("Hello")
	.Does(() => 
{
	Information("Running AppVeyor build.");
});

RunTarget("Hello");