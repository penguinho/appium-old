#! /usr/bin/python

from iosclient import *

client = iosclient(iosclient.iosclientconfig('iphone', 'username_here', 'password_here', 'TestApp.xcodeproj', 'TestApp.app', 'TestApp', 'iphonesimulator5.1',))
client.start()
print 'Enter UIAutomation Command: (exit will quit the program)'
while(True):
	cmd = raw_input()
	if cmd == 'exit':
		client.stop()
		exit(0)
	else:
		resp = client.issuecommand(cmd)
