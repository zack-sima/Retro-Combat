#coding=utf-8
from fastapi import FastAPI, Request
import time
from starlette.websockets import WebSocket
import uvicorn
import sys

app = FastAPI()
from fastapi.middleware.cors import CORSMiddleware


server_password = ["retrocombat_admin"]
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

"www.retrocombat.com:8000/0":"Hong Kong 1",
"www.retrocombat.com:8000/1":"Hong Kong 2",
"www.retrocombat.com:8000/2":"Hong Kong 3",
"www.retrocombat.com:8000/3":"Hong Kong 4",
"www.retrocombat.com:8000/4":"Hong Kong 5",
"www.retrocombat.com:8000/5":"Hong Kong 6",
"www.retrocombat.com:8000/6":"Hong Kong 7",

}

import requests
import threading

def add_online_player(ip):
    requests.get("https://retrocombat.com:2022/add_online_player?ip=" + str(ip), timeout=5)


chinese_message = [""]
english_message = [""]
chinese_message_armchair = [""]
english_message_armchair = [""]
@app.get("/server_msg_english")
async def english_news(request:Request):
    threading.Thread(target=add_online_player, args=(request.client.host,)).start()
    return english_message[0]
@app.get("/server_msg_chinese")
async def chinese_news(request:Request):
    threading.Thread(target=add_online_player, args=(request.client.host,)).start()
    return chinese_message[0]

@app.get("/armchair_msg_english") #news for AC, not Retro Combat
async def english_news_armchair(request:Request):
    threading.Thread(target=add_online_player, args=(request.client.host,)).start()
    return english_message_armchair[0]
@app.get("/armchair_msg_chinese")
async def chinese_news_armchair(request:Request):
    threading.Thread(target=add_online_player, args=(request.client.host,)).start()
    return chinese_message_armchair[0]
@app.get("/update_armchair_english")
async def update_english_armchair(message: str, password: str):
    if password == server_password[0]:
        english_message_armchair[0] = message
        return "success"
    else:
        return "wrong password"
@app.get("/update_armchair_chinese")
async def update_chinese_arm(message: str, password: str):
    if password == server_password[0]:
        chinese_message_armchair[0] = message
        return "success"
    else:
        return "wrong password"

@app.get("/server_time") #only on the official server to prevent using different time zone
async def return_time():
    return time.time()
@app.get("/update_msg_english")
async def update_english(message: str, password: str):
    if password == server_password[0]:
        english_message[0] = message
        return "success"
    else:
        return "wrong password"
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
@app.get("/update_msg_chinese")
async def update_chinese(message: str, password: str):
    if password == server_password[0]:
        chinese_message[0] = message
        return "success"
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


