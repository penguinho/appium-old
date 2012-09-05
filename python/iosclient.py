#! /usr/bin/python

from os import environ as env, path, chdir, mkdir, remove, listdir
from os.path import split as pathsplit
from os.path import exists
from shutil import rmtree, copy, move
from sys import argv
from commands import getstatusoutput as runcmd
from urllib import urlretrieve as download
from tempfile import mkdtemp
from time import time
from subprocess import Popen as spawncmd
from shlex import split as splitargs

# ios client class
class iosclient(object):
	
	# true if the client is batching commands
	isbatchingcommands = None
	# index of the current command
	commandindex = None
	# path to the compile ios application
	compiledapppath = None
	# queue for batched commands
	batchcommandqueue = None
	# configuration for the iosclient
	config = None
	
	# prebuild steps
	onprebuild = lambda self, xcodeprojectpath: None
	# postbuild steps
	onpostbuild = lambda self, xcodeprojectpath: None
	# shutdown steps
	shutdown = lambda self, xcodeprojectpath: None
	
	# running instruments process
	instrumentsprocess = None
	# security agent detector process
	securityagentdetectorprocess = None
	# directory where the ios automation client is running
	iosautoworkingdirectory = None
	# base path
	basepath = pathsplit(path.realpath(__file__))[0]
	
	# stores configuration information about an ios client
	class iosclientconfig(object):
		# type of device
		device = None
		# user name used to authenticate any escalation dialogs that may appear when running the application
		adminusername = None
		# password used to authenticate any escalation dialogs that may appear when running the application
		adminpassword = None
		# name of the xcode project for the supplied application
		xcodeprojectname = None
		# path to the xcode project
		xcodeprojectpath = None
		# name of the compiled app
		compiledappname = None
		# target for the build
		buildtarget = None
		# sdk for the build
		buildsdk = None
		# scheme for the build
		buildscheme = None

		# contructor for iosclientconfig
		def __init__(self, device, adminusername, adminpassword, xcodeprojectname, compiledappname, buildtarget, buildsdk='iphonesimulator5.1', buildscheme=None, xcodeprojectpath=None):
			self.device = device;
			self.adminusername = adminusername
			self.adminpassword = adminpassword
			self.xcodeprojectname = xcodeprojectname
			self.compiledappname = compiledappname
			self.buildtarget = buildtarget
			self.buildsdk = buildsdk
			self.buildscheme = buildscheme
			self.xcodeprojectpath = xcodeprojectpath

	# constructor for iosclient
	def __init__(self, config):
		self.config = config
		self.batchcommandqueue = []
		self.isbatchingcommands = False
		self.commandindex = -1
		compiledapppath = ''
	
	# checks if the ios automation client is running
	def isrunning(self):
		return self.instrumentsprocess != None and self.instrumentsprocess.poll() == None

	# starts the ios automation client
	def start(self):
		# do not start the client again if it's already running
		if self.isrunning():
			return True;
		
		# create a temporary working directory
		print ''
		print '-Installing Automation Client-'
		self.iosautoworkingdirectory = mkdtemp('', 'iosauto-')

		# install the instruments template
		instrumentstemplatepath = path.join(self.iosautoworkingdirectory, 'Automation.tracetemplate')
		copy(path.join(self.basepath, 'Automation.tracetemplate'), instrumentstemplatepath)
				
		# install the python script used to write response back
		copy(path.join(self.basepath, 'writeResponse.py'), path.join(self.iosautoworkingdirectory, 'writeResponse.py'))
		
		# install the bootstrap javascript
		bootstrappath = path.join(self.iosautoworkingdirectory, 'bootstrap.js')
		bootstrapsourcefile = open(path.join(self.basepath, 'bootstrap.js'), 'r')
		bootstrapsource = bootstrapsourcefile.read()
		bootstrapsourcefile.close()
		bootstrapdestfile = open (bootstrappath, 'w')
		bootstrapdestfile.write(bootstrapsource.replace('$PATH_ROOT', self.iosautoworkingdirectory + '/'))
		bootstrapdestfile.close()
					
		# install beat security agent applescript
		bsapath = path.join(self.iosautoworkingdirectory, 'BeatSecurityAgent.applescript')
		bsasourcefile = open(path.join(self.basepath, 'BeatSecurityAgent.applescript'), 'r')
		bsasource = bsasourcefile.read()
		bsasourcefile.close()
		bsadestfile = open (bsapath, 'w')
		bsadestfile.write(bsasource.replace('$USERNAME', self.config.adminusername).replace('$PASSWORD', self.config.adminpassword))
		bsadestfile.close()

		# clean the ios simulator by deleting old builds
		print ''
		print '-Cleaning the iOS Simulator-'
		builddirectory = path.join(env.get('HOME'), 'Library/Application Support/iPhone Simulator')
		if exists(builddirectory):
			for subdirectory in listdir(builddirectory):
				print 'Deleting: ' + path.join(builddirectory, subdirectory)
				try:
					rmtree(path.join(builddirectory, subdirectory))
				except:
					pass
		
		# delete copies of the app under test
		(status, output) = runcmd('mdfind -name "' + self.config.compiledappname + '"')
		if status == 0:
			oldbuilds = output.split('\n')
			if len(oldbuilds[0]) > 0:
				for oldbuildpath in oldbuilds:
					try:
						print 'Deleting: ' + oldbuildpath
						rmtree(oldbuildpath)
					except:
						pass
		
		# find the xcode project if it was not supplied
		if self.config.xcodeprojectpath == None or not '/' in self.config.xcodeprojectname:
			(status, output) = runcmd('mdfind -name "' + self.config.xcodeprojectname + '"')
			if status == 0:
				xcodeprojects = output.split('\n')
				if len(xcodeprojects[0]) > 0:
					self.config.xcodeprojectpath = xcodeprojects[0]
		
	    # perform prebuild steps
		self.onprebuild(self.config.xcodeprojectpath)
										   
	    # generate the xcodebuild command line
		buildcmd = '/usr/bin/xcodebuild -sdk ' + self.config.buildsdk + ' -target ' + self.config.buildtarget
		if self.config.buildscheme != None:
			buildcmd = buildcmd + ' -scheme ' + self.config.buildscheme
		if 'pad' in self.config.device:
			buildcmd = buildcmd + ' TARGETED_DEVICE_FAMILY=2'
		else:
			buildcmd = buildcmd + ' TARGETED_DEVICE_FAMILY=1'

		# build the project
		print ''
		print '-Building the Xcode Project-'
		print 'Xcode Project Path: ' + self.config.xcodeprojectpath
		xcodeprojectdirectory = pathsplit(self.config.xcodeprojectpath)[0]
		chdir(xcodeprojectdirectory)
		runcmd(buildcmd)
		chdir(self.basepath)

		# perform post build steps
		self.onpostbuild(self.config.xcodeprojectpath)

		# find the compiled application
		starttime = time()
		while self.compiledapppath == None and time() - starttime < 600:
			(status, output) = runcmd('mdfind -name "' + self.config.compiledappname + '"')
			if status == 0:
				compiledapps = output.split('\n')
				if len(compiledapps[0]) > 0 and compiledapps[0].endswith('.app'):
					self.compiledapppath = compiledapps[0]
		print 'Compiled App Path: ' + self.compiledapppath
		
		# launch beat security agent process
		self.securityagentdetectorprocess = spawncmd(['/usr/bin/osascript' , bsapath])
		
		# launch the automation in instruments
		instrumentscmd = '/usr/bin/instruments -t ' + instrumentstemplatepath + ' ' + self.compiledapppath + ' -e UIASCRIPT ' + bootstrappath + ' -e UIARESULTSPATH ' + self.iosautoworkingdirectory
		self.instrumentsprocess = spawncmd(splitargs(instrumentscmd), stdout=None, stdin=None, stderr=open('/dev/null','w'))
		print ''
		print '-Automation Started-'
		return self.instrumentsprocess.poll() == None

	# issues a command to the simulator
	def issuecommand(self, cmdtext):
		# add the command to the batch command queue if the client is batching
		if self.isbatchingcommands:
			self.batchcommandqueue.append(cmdtext)
			return [[0, 'command batched successfully']]
		
		# increment the command index because this is not a batch command
		self.commandindex = self.commandindex + 1
		
		# write the command to the command file
		try:
			cmdfile = open(path.join(self.iosautoworkingdirectory, str(self.commandindex) + '-cmd.txt'), 'w')
			cmdfile.write(cmdtext)
			cmdfile.close()
		except:
			print 'ERROR WRITING COMMAND'
			self.commandindex = self.commandindex - 1
		
		# wait up to 10 minutes for a response
		issuetime = time()
		while time() - issuetime < 600:
			responsefile = path.join(self.iosautoworkingdirectory, str(self.commandindex) + '-resp.txt')
			#try:
			# parse results from the response file if it exists
			if exists(responsefile):
				results = []
				resultfile = open(responsefile, 'r')
				resultxml = resultfile.read()
				resultfile.close()
				for item in resultxml.split('<response>')[1:]:
					results.append(item.split('</response>')[0].split(',',1))
				return results
			#except:
			#	print 'COULD NOT PARSE RESPONSE'
	
	# runs commands in a batch job
	def batchissuecommands(self, commands):
		
		# enqueue all of the commands
		self.isbatchingcommands = True
		for command in commands:
			command()

		# generate on large command string
		aggregatecommandstring = ''
		for i in range(0, len(self.batchcommandqueue)):
			aggregatecommandstring = aggregatecommandstring + self.batchcommandqueue[i] + '\n'
			aggregatecommandstring = aggregatecommandstring + '"end batched automation command ' + str(i) + '";'
			if i < (len(self.batchcommandqueue)-1):
				aggregatecommandstring = aggregatecommandstring + '\n'
		self.batchcommandqueue = []
					
		# run all of the commands at once
		self.isbatchingcommands = False
		result = self.issuecommand(aggregatecommandstring)

		# separate out the results
		allresults = {}
		commandresults = []
		lastcommandindex = 0
		for i in range(0,len(result)):
			if result[i][1] == 'end batched automation command ' + str(lastcommandindex):
				allresults[lastcommandindex] = commandresults
				lastcommandindex = lastcommandindex + 1
				commandresults = []
			else:
				commandresults.append(result[i])
	
		return allresults
	
	
	# stops running automation
	def stop(self):
		if not self.isrunning():
			return

		# issue command to stop the automation
		print ''
		print '-Stopping Automation-'
		self.issuecommand('runLoop=false;')
				
		# kill the instruments process if it's still running
		startime = time()
		self.securityagentdetectorprocess.terminate()
		starttime = time()
		while (time() - starttime < 15 and self.instrumentsprocess.poll() == None):
			pass
		if self.instrumentsprocess.poll() == None:
			self.instrumentsprocess.terminate()
		runcmd('/usr/bin/osascript -e "tell app \\"iPhone Simulator\\" to quit"')
		return

	# delays for the supplied number of seconds
	def delay(self, seconds):
		return self.issuecommand('delay(' + str(seconds) + ');')[0]

	# gets the element
	def get(self, hook):
		return self.issuecommand(hook + ';')[0][1]

	# gets the value of the element
	def getvalue(self, hook):
		return self.issuecommand(hook + '.value();')[0][1]
	
	# taps the element
	def tap(self, hook):
		return self.issuecommand(hook + '.tap();')[0]

	# scrolls until the element is visible
	def scrollto(self, hook):
		return self.issuecommand(hook + '.scrollToVisible();')[0]

	# sets the value of an element
	def setvalue(self, hook, value):
		value = value.replace('"','\"')
		return self.issuecommand(hook + '.setValue("' + value + '");')[0]
