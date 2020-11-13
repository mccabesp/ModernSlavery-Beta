param 
(
    $tool = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\MSTest.exe",
    $path = ".",
    $include = "*.webtest",
    $results = "./webtestresults.trx", 		# ./webtestresults.trx
    $testsettings = "runner.testsettings", 	# runner.testsettings
	$webserver = "https://localhost:44371" 	# https://localhost:44371
)

#$web_tests = get-ChildItem -Path $paths -Recurse -Include $include
#foreach ($item in $web_tests) {
#    $args += "/TestContainer:$item "
#}

del .\webtestresults.trx

# set any context parameters
Set-Item Env:Test.WebServer "$webserver"

& $tool /TestContainer:$path/Scoping/EnterCodes.webtest /resultsfile:$results