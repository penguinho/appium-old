#! /usr/bin/python

from iosclient import *
from random import randrange

# start the ios automation client
client = iosclient(iosclient.iosclientconfig('iphone', 'username_here', 'password_here', 'TestApp.xcodeproj', 'TestApp.app', 'TestApp', 'iphonesimulator5.1',))
client.start()

# generate two random numbers
num1 = randrange(0,10)
num2 = randrange(0,10)

#####################
# UNBATCHED EXAMPLE #
#####################

#client.setvalue('mainWindow.textFields()[0]', str(num1))
#client.setvalue('mainWindow.textFields()[1]', str(num2))
#client.tap('mainWindow.buttons()["Compute Sum"]')
#result = client.getvalue('mainWindow.staticTexts()[0]')

###################
# BATCHED EXAMPLE #
###################

# add the random numbers inside the ios app
results = client.batchissuecommands([ \
	lambda: client.setvalue('mainWindow.textFields()[0]', str(num1)), \
	lambda: client.setvalue('mainWindow.textFields()[1]', str(num2)), \
	lambda: client.tap('mainWindow.buttons()["Compute Sum"]'), \
	lambda: client.delay(1), \
	lambda: client.getvalue('mainWindow.staticTexts()[0]')])
result = results[4][0][1]

# verify the values match
if result == str(num1 + num2):
	print 'PASS: ' + str(num1+num2) + ' == ' + result
else:
	print 'FAIL: ' + str(num1+num2) + ' != ' + result

# stop the ios automation client
client.stop()
