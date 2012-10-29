from appium import Appium
from bottle import Bottle, request, response, redirect
from bottle import run, static_file
import json
from time import time
from time import sleep

app = Bottle()

@app.get('/favicon.ico')
def get_favicon():
    return static_file('favicon.ico', root='.')

@app.route('/wd/hub/status', method='GET')
def status():
    status = {'sessionId': None, 
              'status': 0, 
              'value': {'build': {'version': 'Appium 1.0'}}}
    return status 

@app.route('/wd/hub/session', method='POST')
def create_session():
    redirect('/wd/hub/session/1')

@app.route('/wd/hub/session/<session_id>', method='GET')
def get_session(session_id=''):
    response = {'sessionId': '1', 
                'status': 0, 
                'value': {"version":"5.0",
                          "webStorageEnabled":False,
                          "locationContextEnabled":False,
                          "browserName":"iOS",
                          "platform":"MAC",
                          "javascriptEnabled":True,
                          "databaseEnabled":False,
                          "takesScreenshot":False}}
    return response

@app.route('/wd/hub/session/<session_id>', method='DELETE')
def delete_session(session_id=''):
    app.ios_client.stop() 
    response = {'sessionId': '1', 
                'status': 0, 
                'value': {}}
    return response 

@app.route('/wd/hub/session/<session_id>/frame', method='POST')
def switch_to_frame(session_id=''):
    status = 0
    request_data = request.body.read()
    try:
        frame = json.loads(request_data).get('id')
        if frame is None:
            app.ios_client.proxy('wd_frame = mainWindow')
        else:
            app.ios_client.proxy('wd_frame = %s' % frame)
    except:
        response.status = 400
        status = 13  # UnknownError

    app_response = {'sessionId': '1', 
                'status': status, 
                'value': {}}
    return app_response 

@app.route('/wd/hub/session/<session_id>/execute', method='POST')
def execute_script(session_id=''):
    status = 0
    ios_response = ''
    request_data = request.body.read()
    try:
        script = json.loads(request_data).get('script')
        ios_response = app.ios_client.proxy(script)[0][1]
    except:
        response.status = 400
        status = 13  # UnknownError

    response = {'sessionId': '1', 
		'status': status, 
		'value': ios_response}
    return response
    
@app.route('/wd/hub/session/<session_id>/element/<element_id>/text', method='GET')
def get_text(session_id='', element_id=''):
    status = 0
    ios_response = ''
    try:
        script = "elements['%s'].value()" % element_id
        ios_response = app.ios_client.proxy(script)[0][1]
    except:
        response.status = 400
        status = 13  # UnknownError

    response = {'sessionId': '1', 
		'status': status, 
		'value': ios_response}
    return response

@app.route('/wd/hub/session/<session_id>/element/<element_id>/attribute/<attribute>', method='GET')
def get_text(session_id='', element_id='', attribute=''):
    status = 0
    ios_response = ''
    try:
        script = "elements['%s'].name()" % element_id
        ios_response = app.ios_client.proxy(script)[0][1]
    except:
        response.status = 400
        status = 13  # UnknownError

    response = {'sessionId': '1', 
		'status': status, 
		'value': ios_response}
    return response
@app.route('/wd/hub/session/<session_id>/element/<element_id>/click', method='POST')
def get_text(session_id='', element_id=''):
    status = 0
    ios_response = ''
    try:
        script = "elements['%s'].tap()" % element_id
        ios_response = app.ios_client.proxy(script)[0][1]
    except:
        response.status = 400
        status = 13  # UnknownError

    response = {'sessionId': '1', 
		'status': status, 
		'value': ios_response}
    return response

@app.route('/wd/hub/session/<session_id>/element/<element_id>/value', method='POST')
def set_value(session_id='', element_id=''):
    status = 0
    ios_response = ''
    request_data = request.body.read()
    print request_data
    try:
        value_to_set = json.loads(request_data).get('value') 
        value_to_set = ''.join(value_to_set)

        script = "elements['%s'].setValue('%s')" % (element_id, value_to_set) 
        print script
        ios_response = app.ios_client.proxy(script)[0][1]
    except:
        response.status = 400
        status = 13  # UnknownError

    response = {'sessionId': '1', 
		'status': status, 
		'value': ''}
    return response

@app.route('/wd/hub/session/<session_id>/elements', method='POST')
def find_elements(session_id=''):
    status = 0
    request_data = request.body.read()
    print request_data
    try:
        locator_strategy = json.loads(request_data).get('using')
        element_type = json.loads(request_data).get('value') 
        elements = {'button': 'buttons()',
                    'textField': 'textFields()',
                    'secureTextField': 'secureTextFields()'} 

        ios_request = "wd_frame.%s.length" % elements[element_type]
        print ios_request

        number_of_items = int(app.ios_client.proxy(ios_request)[0][1])
        found_elements = []
        print number_of_items
        for i in range(number_of_items):
            var_name = 'wde' + str(int(time() * 1000000))
            print var_name

            ios_request = "elements['%s'] = wd_frame.%s[%s]" % (var_name, elements[element_type], i)
            print ios_request

            ios_response = app.ios_client.proxy(ios_request)
            print ios_response
            found_elements.append({'ELEMENT':var_name})
    except:
        response.status = 400
        status = 13  # UnknownError
    response = {'sessionId': '1', 
                'status': status, 
                'value': found_elements}
    return response 

if __name__ == '__main__':
    import sys
    if len(sys.argv) == 2:
        app = sys.argv[1]
        app.ios_client = Appium(app)
        app.ios_client.start() 
        run(app, host='0.0.0.0', port=4723)
    else:
      print """
  Appium - iOS App Automation 
       
  Usage: 
    When run as a script, include the absolute path to an app:
    $ python server.py ~/somethingawesome.app
  """

