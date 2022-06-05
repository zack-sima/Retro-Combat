#coding=utf-8
from fastapi import FastAPI
import time
from starlette.websockets import WebSocket
import uvicorn
import sys
server_password = ["retrocombat_admin"]

app = FastAPI()
from fastapi.middleware.cors import CORSMiddleware

origins = [
    "*"
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)



servers = {

"47.88.27.128:8000/0":"US West 1",
"47.88.27.128:8000/1":"US West 2",
"47.88.27.128:8000/2":"US West 3",
"47.88.27.128:8000/3":"US West 4",
"47.88.27.128:8000/4":"US West 5",
"47.88.27.128:8000/5":"US West 6",
"47.88.27.128:8000/6":"US West 7",
"47.88.27.128:8000/7":"US West 8",
"47.88.27.128:8000/8":"US West 9",
"47.88.27.128:8000/9":"US West 10",


}
@app.get("/remove_server")
async def remove_server(address: str, password: str):
    if password == server_password[0]:
        if (servers.get(address)):
            servers.pop(address)
            return "updated servers list to remove " + address
        else:
            return "address does not exist"
    else:
        return "wrong password"
@app.get("/add_server")
async def add_server(address: str, server_name: str, password: str):
    if password == server_password[0]:
        if not (servers.get(address)):
            servers[address] = server_name
            return "updated servers list to add " + address
        else:
            return "address already exists"
    else:
        return "wrong password"

@app.get("/servers")
async def return_servers():
	return_string = ""
	for key, value in servers.items():
		return_string += key + "|" + value + "+"
	return return_string

uvicorn.run(app, port=8001, host="0.0.0.0")
#https
#uvicorn.run(app, port=8001, host="0.0.0.0", ssl_keyfile="/root/ssl_certificates/privkey.pem", ssl_certfile="/root/ssl_certificates/fullchain.pem")



