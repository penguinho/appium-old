from applecart import Applecart
from bottle import Bottle, request, response
from bottle import run, static_file
import json
from time import time

app = Bottle()

@app.get('/favicon.ico')
def get_favicon():
    return static_file('favicon.ico', root='.')

@app.route('/status', method='GET')
def status():
    status = {'sessionId': None, 
              'status': 0, 
              'value': {'build': {'version': 'Applecart 1.0'}}}
    return status 

@app.route('/session', method='POST')
def create_session():
#TODO: Get app name from desired caps and start with applecart here
#    app.ios_client = Applecart('/path/to/your/awesome.app')
#    app.ios_client.start() 
    response = {'sessionId': '1', 
                'status': 0, 
                'value': None}
    return response 

@app.route('/session/<session_id>', method='DELETE')
def delete_session(session_id=''):
    app.ios_client.stop() 
    response = {'sessionId': '1', 
                'status': 0, 
                'value': None}
    return response 

@app.route('/session/<session_id>/frame', method='POST')
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

    response = {'sessionId': '1', 
                'status': status, 
                'value': {}}
    return response 

@app.route('/session/<session_id>/element', method='POST')
def find_element(session_id=''):
    status = 0
    request_data = request.body.read()
    try:
        locator_strategy = json.loads(request_data).get('using')
        element_id = json.loads(request_data).get('value') 
        element_var_name = 'wde' + str(int(time() * 1000000))
    except:
        response.status = 400
        status = 13  # UnknownError
    response = {'sessionId': '1', 
                'status': status, 
                'value': {'message':'Unknown Error'}}
    return response 
    

#run(app, server='paste', host='0.0.0.0', port=8080, reloader=True)
run(app, host='0.0.0.0', port=8080, reloader=True)
