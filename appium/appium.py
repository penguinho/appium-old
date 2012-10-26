import ConfigParser
import glob
import os
from os.path import exists
from shutil import copy
from subprocess import call, check_output, Popen, PIPE
from tempfile import mkdtemp
from time import time, sleep

class Appium:
    def __init__(self, app='', ):
        self.app       = app
        self.username  = None
        self.password  = None
        self.instruments_process = None
        self.command_index = -1

    def start(self):
        ## Do not start again if Instruments is already running
        if self.is_running():
            return True
        self.command_index = -1
        self.get_config()
        self.create_temp_dir()
        self.copy_files()
        self.modify_bootstrap_script()
        self.kill_security_popup()
        self.launch_instruments()
        self.wait_for_simulator()
        self.wait_for_app()

    # Check if Instruments is running
    def is_running(self):
        return self.instruments_process is not None and self.instruments_process.poll() is  None

    def get_config(self):
        # Check to see if the username and password have been set already::
        if self.username and self.password:
            return
        else:
            # Try to get username and password from file:
            config = ConfigParser.ConfigParser()
            result = config.read(os.path.expanduser('~/.appium'))
            if result:
                self.username = config.get('appium','username')
                self.password = config.get('appium','password')
            else:
                raise Exception("ERROR: You need to specify OS X name and password in ~/.appium")

    # Create temp dir
    def create_temp_dir(self):
        self.temp_dir = mkdtemp('', 'appium-')
        #print self.temp_dir

    # Copy files
    def copy_files(self):
        self.base_path = os.path.split(os.path.realpath(__file__))[0]
        source = os.path.join(self.base_path, 'template', '*.*')
        for filename in glob.glob(source):
            copy(filename, self.temp_dir)

    # Modify bootstrap script
    def modify_bootstrap_script(self):
        self.bootstrap = os.path.join(self.temp_dir,'bootstrap.js')
        with open(self.bootstrap,'r') as file:
            contents = file.read()
        new_contents = contents.replace("$PATH_ROOT", self.temp_dir + '/')
        with open(self.bootstrap,'w') as file:
            file.write(new_contents)

    # Kill security popup
    def kill_security_popup(self):
        applescript = os.path.join(self.temp_dir,'BeatSecurityAgent.applescript')
        self.security_process = Popen(['/usr/bin/osascript', applescript, self.username, self.password])
        return self.security_process.poll() is None  # Should be True

    # Launch Instruments app
    def launch_instruments(self):
        command = ['/usr/bin/instruments', '-t', 
                   os.path.join(self.temp_dir,'Automation.tracetemplate'), 
                   self.app,
                   '-e', 'UIASCRIPT', self.bootstrap,
                   '-e', 'UIARESULTSPATH', self.temp_dir]
        self.instruments_process = Popen(command, stdout=PIPE, stdin=None, stderr=PIPE)
        return self.instruments_process.poll() is None  # Should be True

    def simulator_state(self):
        process_states = {'true': True,
                          'false': False}

        output = check_output(["/usr/bin/osascript", "-e",
            "tell application \"System Events\" to (name of processes) contains \"iPhone Simulator\""])
        
        is_running = False
        if output:
            output = output.strip()
            is_running = process_states.get(output)
        return is_running

    def wait_for_simulator(self, timeout=30):
        starttime = time()
        while time() - starttime < timeout:
            state = self.simulator_state()
            if state == True:
                self.simulator_is_running = True
                return True
            else:
                sleep(.5)
        self.simulator_is_running = False
        return False

    def wait_for_app(self):
        # When we get a response we know the app is alive.
        self.proxy('') 

    # Proxy a command to the simulator
    # using a file-based inter-process communication
    # between Python and Instruments.
    def proxy(self, command):
        self.write_command(command) 
        response = self.read_response()        
        return response

    # Write the command to a file
    def write_command(self, command):
        # Increment the command index 
        self.command_index = self.command_index + 1
        try:
            filename = str(self.command_index) + '-cmd.txt'
            filepath = os.path.join(self.temp_dir, filename)
            with open(filepath,'w') as file:
                file.write(command)
        except:
            print 'ERROR WRITING COMMAND'
            self.command_index = self.command_index - 1

    def read_response(self):
        # Wait up to 10 minutes for a response
        start_time = time()
        while time() - start_time < 600:
            filename = str(self.command_index) + '-resp.txt'
            filepath = os.path.join(self.temp_dir, filename)
            if exists(filepath):
                results = []
                with open(filepath,'r') as file:
                    xml = file.read()
                for item in xml.split('<response>')[1:]:
                    results.append(item.split('</response>')[0].split(',',1))
                return results

    def stop(self):
        if not self.is_running():
            return
        # Kill the security popup killer
        self.security_process.terminate()

        # Tell Instruments to shut down (nicely)
        self.proxy('runLoop=false;')

        # Kill Instruments if it's not being nice
        start_time = time()
        while (time() - start_time < 15 and self.instruments_process.poll() == None):
            pass
        if self.instruments_process.poll() is None:
            self.instruments_process.terminate()

        # Kill iOS Simulator
        call("""/usr/bin/osascript -e 'tell app "iPhone Simulator" to quit'""", shell=True)
        self.simulator_is_running = False

if __name__ == '__main__':
    from interpreter import launch
    import sys
    if len(sys.argv) == 2:
        app = sys.argv[1]
        launch(app)
    else:
      print """
  Appium - iOS App Automation 
       
  Usage: 
    When run as a script, include the absolute path to an app:
    $ python appium.py ~/somethingawesome.app
  """

