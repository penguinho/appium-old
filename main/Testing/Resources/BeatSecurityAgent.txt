repeat
	set procs to ""
	tell application "System Events"
		set procs to name of every process
	end tell
	if procs contains "SecurityAgent" then
		tell application "System Events"
			tell process "SecurityAgent" to set value of text field 1 of scroll area 1 of group 1 of window 1 to "$USERNAME"
			tell process "SecurityAgent" to set value of text field 2 of scroll area 1 of group 1 of window 1 to "$PASSWORD"
			click button 2 of group 2 of window 1 of application process "SecurityAgent"
			log ("Dismissed Security Dialog")
		end tell
	end if
	delay 1
end repeat