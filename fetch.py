import base64
import requests
import urllib.parse
import webbrowser
from http.server import HTTPServer, BaseHTTPRequestHandler

# === CONFIGURATION ===
CLIENT_ID = 'dadcc3e1920f4fb78f62e6704e233a0f'
CLIENT_SECRET = '95932a3835fa4aa3824e7407c3e7badd'
REDIRECT_URI = 'http://127.0.0.1:8888/callback'
SCOPES = 'user-read-private user-read-email playlist-modify-private'  # Add scopes as needed

# === STEP 1: Build and open authorization URL ===
auth_params = {
    'client_id': CLIENT_ID,
    'response_type': 'code',
    'redirect_uri': REDIRECT_URI,
    'scope': SCOPES,
}
auth_url = 'https://accounts.spotify.com/authorize?' + urllib.parse.urlencode(auth_params)

print(f"Opening browser for Spotify login...")
webbrowser.open(auth_url)

# === STEP 2: Set up local server to catch redirect with code ===
class AuthHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        print(f"Got request: {self.path}")  # Add this line to debug
        query = urllib.parse.urlparse(self.path).query
        params = urllib.parse.parse_qs(query)
        if 'code' in params:
            code = params['code'][0]
            self.send_response(200)
            self.end_headers()
            self.wfile.write(b"You can close this window now.")
            self.server.code = code

httpd = HTTPServer(('localhost', 8888), AuthHandler)
httpd.handle_request()  # Handle a single request (the redirect from Spotify)

auth_code = httpd.code
print(f"Authorization code received: {auth_code}")

# === STEP 3: Exchange code for access + refresh tokens ===
token_url = 'https://accounts.spotify.com/api/token'
auth_header = base64.b64encode(f"{CLIENT_ID}:{CLIENT_SECRET}".encode()).decode()
headers = {
    'Authorization': f'Basic {auth_header}',
    'Content-Type': 'application/x-www-form-urlencoded',
}
data = {
    'grant_type': 'authorization_code',
    'code': auth_code,
    'redirect_uri': REDIRECT_URI,
}

response = requests.post(token_url, headers=headers, data=data)
response_data = response.json()
print(response_data)
if 'refresh_token' in response_data:
    print("\n✅ Refresh Token:")
    print(response_data['refresh_token'])
else:
    print("\n❌ Error fetching token:")
    print(response_data)