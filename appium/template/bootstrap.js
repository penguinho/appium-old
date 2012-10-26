/* ***** helper functions ***** */

// delay in seconds
function delay(secs)
{
	var date = new Date();
	var curDate = null;
	
	do { curDate = new Date(); }
	while(curDate-date < (secs * 1000.0));
} 

// find cell with name
function findCellWithName(cellName, window) {
	
	return window.visibleCells()[cellName];
	
	for (index = 0; index < window.tableViews().length; index = index + 1) {
		cell = window.tableViews()[index].visibleCells()[cellName];
		if ((cell != null) && (cell.toString() != "[object UIAElementNil]")) {
			return cell;
		}
	}
	return UIAElementNil;
}

/* ***** main loop ***** */

// automation globals
var iosAutoPath = "$PATH_ROOT"
var target      = UIATarget.localTarget();
var application = target.frontMostApp();
var host = target.host();
var mainWindow  = application.mainWindow();

// loop variables
var runLoop = true;
var instructionNumber = 0;

// main loop
while (runLoop)
{
    var instructionFile = iosAutoPath + instructionNumber.toString() + "-cmd.txt";
    var responseFile = iosAutoPath + instructionNumber.toString() + "-resp.txt";
    var instruction = host.performTaskWithPathArgumentsTimeout("/bin/cat", [instructionFile], 5);
    if (instruction.exitCode == 0)
    {
        var resp = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<collection>\n";
        var instructionText = instruction.stdout;
		try
		{
			var jsCommands = instructionText.split('\n');
			for (var jsCommandIndex = 0; jsCommandIndex < jsCommands.length; jsCommandIndex++)
			{
           		var jsCommand = jsCommands[jsCommandIndex];
				try
				{
					UIALogger.logDebug(instructionNumber.toString() + "." + jsCommandIndex.toString() + " - Command   - " + jsCommand);
           			var evalResult = eval(jsCommand);
           			if (evalResult == null)
           			{
			   			evalResult = "";
		       		}
					UIALogger.logDebug(instructionNumber.toString() + "." + jsCommandIndex.toString() + " - Response  - " + evalResult.toString());
	           		resp = resp + "<response>" + "0," + evalResult.toString() + "</response>\n";
				}
				catch (err)
				{
					UIALogger.logWarning("js command execution failed: " + err.description);
					resp = resp + "<response>" + "-1," + err.description + "</response>\n";
				}
			}
		}
		catch (err)
		{
			UIALogger.logWarning("could not parse intruction set: " + err.description);
			resp = resp + "<error>could not parse intruction set</error>\n";
		}
        resp = resp + "</collection>\n";
        UIALogger.logDebug("INSTRUCTION SET #" + instructionNumber.toString() + " XML RESPONSE:\n\n" + resp + "\n");
        host.performTaskWithPathArgumentsTimeout("/usr/bin/python", [ iosAutoPath + "writeResponse.py" ,responseFile, resp], 5);
		UIALogger.logDebug("END INSTRUCTION SET #" + instructionNumber.toString());
  	    instructionNumber++;
		UIALogger.logDebug("BEGIN INSTRUCTION SET #" + instructionNumber.toString());
    }
}
