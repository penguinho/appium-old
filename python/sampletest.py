#! /usr/bin/python

from iosclient import *

client = iosclient(iosclient.iosclientconfig('iphone', 'username_here', 'password_here', 'TestApp.xcodeproj', 'TestApp.app', 'TestApp', 'iphonesimulator5.1',))
client.start()
num1 = 3
num2 = 6
client.setvalue('mainWindow.textFields()[0]', str(num1))
client.setvalue('mainWindow.textFields()[1]', str(num2))
client.tap('mainWindow.buttons()["Compute Sum"]')
result = client.getvalue('mainWindow.staticTexts()[0]')

if result == str(num1 + num2):
	print 'PASS: ' + str(num1+num2) + ' == ' + result
else:
	print 'FAIL: ' + str(num1+num2) + ' != ' + result

client.stop()
