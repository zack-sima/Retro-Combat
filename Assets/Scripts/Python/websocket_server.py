#coding=utf-8
from fastapi import FastAPI
import time
from fastapi.middleware.cors import CORSMiddleware
from starlette.websockets import WebSocket
import uvicorn
import sys
import asyncio, threading
import random
import json
server_password = ["retrocombat_admin"]

class Room:
	def remove_player(self, uid:int):
		self.scores[uid] = None
		self.players_shooting[uid] = None
		self.players_dying[uid] = None
		self.players_damaged[uid] = None
		self.players_damaged_sender[uid] = None
		self.players_moving[uid] = None
		self.players_running[uid] = None
		self.players_position[uid] = None
		self.players_rotation[uid] = None
		self.players_active[uid] = None
		self.players_weapon[uid] = None
		self.players_country[uid] = None
		self.players_spine_rotation[uid] = None
		self.players_crouch_rotation[uid] = None
		self.players_pickup_rotation[uid] = None
	def reset_server(self):
		print("server stats reset")
		self.recent_messages = []
		self.scores[-1] = 0
		self.scores[-2] = 0
		for key, value in self.scores.items():
			self.scores[key] = 0
		self.timer =[0, 0, 0]
	def check_empty(self):
		not_null = False
		for key, value in self.players_active.items():
			if value != None:
				not_null = True
				break
		return not not_null
	def __init__(self, map_num, gamemode_num):
		
		self.recent_messages = []
		self.gamemode = [0]
		self.gamemode[0] = gamemode_num
		self.scores = {-1: 0, -2: 0}

		#triggers - key is player id
		self.players_shooting = {}
		self.players_dying = {} #updaged by player; once read, all clients will put the player to death animation and respawn player once notified 

		self.players_damaged = {} #damage to player is added here (this value is reset once read by receiver)


		self.players_damaged_sender = {}

		#polling - constantly updated
		self.players_active = {}
		self.players_moving = {}
		self.players_running = {}
		self.players_position = {}
		self.players_rotation = {} #float list as value
		self.players_weapon = {}
		self.players_country = {}
		self.players_spine_rotation = {}
		self.players_crouch_rotation = {}
		self.players_pickup_rotation = {}
		self.players_vehicle = {} #0 is not vehicle, 1 is tank, 2 is airplane. Optional so if this value is not given it defaults to 0
		self.players_vehicle_1 = {} #vehicle property 1 (id of vehicle)
		self.players_vehicle_2 = {} #vehicle property 2 (airplane speed/tank head rotation)
		self.players_vehicle_3 = {} #vehicle property 3 (tank turret rotation)

		self.map_id = [0]
		self.map_id[0] = map_num
		self.timer = [0, 0, 0] #first element is actual timer, second is delta timer,  third is delta delta
		self.server_version = ["default"] #default means all versions are accepted

		self.reset_server() #server resets when no players are on?
	def server_countdown(self):
		if (self.timer[0] > 0):
			if not self.check_empty():
				self.timer[0] -= 1
		else:
			if self.timer[2] > 0:
				self.timer[0] = -25 #countdown before server starts again
				self.timer[2] = -25
			else:
				self.timer[0] += 1
				if (self.timer[0] > -1): #reset timer
					self.reset_server()
					self.timer[0] = 780 #match time (set custom!)
					self.timer[2] = 780
		self.timer[2] = self.timer[1]
		self.timer[1] = self.timer[0]
	def to_JSON(self):
		return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=0, separators=(',',':'))


#initial servers setup
if len(sys.argv) == 1:
	rooms = [Room(0, 0), Room(1, 0)]
else:
	rooms = []
	for index in range(len(sys.argv) // 2): #in sys.argv, 0 1 0 1 means two maps with id 0 and mode 1
		#print(int(sys.argv[index + 1]))
		rooms.append(Room(int(sys.argv[index * 2 + 1]), int(sys.argv[index * 2 + 2])))

def server_countdown_initialize():
	ticker = threading.Event()
	while not ticker.wait(1):
		for i in rooms:
			i.server_countdown()
threading.Thread(target=server_countdown_initialize).start()
app = FastAPI()

@app.get("/add_room") #did not implement remove room as there doesn't seem to be a need of it yet
#adds a room
async def add_rooms(map_id: int, gamemode: int, password:str):
	if (server_password[0] == password):
		rooms.append(Room(map_id, gamemode))
		print("room added")
	else:
		print("room cannot be added because password is wrong")
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

def add_message(room: int, msg: str):
	rooms[room].recent_messages.append(msg)
	if len(rooms[room].recent_messages) > 7:
		rooms[room].recent_messages.pop(0)
@app.get("/{rid}/set_serverroom_version")
async def update_serverroom_version(rid: int, version: str, password: str):
	if password == server_password[0]:
		rooms[rid].server_version[0] = version
		return "set to " + version
	else:
		return "wrong password"
@app.get("/{rid}/reset") #for debugging
async def reset_server(rid: int):
	rooms[rid].reset_server()
@app.get("/{rid}/lobby")
async def server_request(rid: int):
	if len(rooms) > rid:
		active_count = find_active_players(rid)
		return str(rooms[rid].map_id[0]) + "|" + str(active_count) + "|" + str(rooms[rid].gamemode[0])
	return "room does not exist"
def find_active_players(rid: int):
	active_count = 0
	for c, j in rooms[rid].players_active.items():
		#print(c, j)
		if (j != None):
			active_count += 1
	return active_count

max_players_allowed = [9]
@app.websocket("/{rid}/server/{player_name}/{team}/{version}")
async def server_websocket (websocket: WebSocket, rid: int, player_name: str, team: int, version: str):
	i = -1
	try: #'''
		return_outdated_version = False
		await websocket.accept()
		if rooms[rid].server_version[0] != "default" and version != rooms[rid].server_version[0]:
			return_outdated_version = True
			print("server version mismatch")
			# await websocket.close()
			# return #client will have error

		return_full_room = False #if this is true the player will never be actually registered as part of the game
		if (find_active_players(rid) > max_players_allowed[0]):
			return_full_room = True
			print("full room")

		i = 0
		if not return_full_room and not return_outdated_version:
			if team == -1: #join random team
				allies = 0
				axis = 0
				for index, j in rooms[rid].players_active.items():
					if j != None:
						if index % 2 == 0:
							allies += 1
						else:
							axis += 1
				if allies > axis:
					team = 1
				else:
					team = 0
				# while rooms[rid].players_active.get(i) != None:
				# 	i += 1

			if team == 0: #join team 1
				while rooms[rid].players_active.get(i) != None or i % 2 != 0:
					i += 1
			elif team == 1: #join team 2
				while rooms[rid].players_active.get(i) != None or i % 2 != 1:
					i += 1

			#print("added player to room " + str(rid) + "; player id " + str(i) + " and name " + player_name)
			add_message(rid, player_name + " joined the game")
			rooms[rid].players_active[i] = player_name
			rooms[rid].players_position[i] = [0, 0, 0]
			rooms[rid].players_rotation[i] = [0, 0, 0]
			rooms[rid].players_moving[i] = 0
			rooms[rid].players_running[i] = 0
			rooms[rid].players_shooting[i] = "0" #this is updated to add a random string behind it (when this update is detected locally one shot is fired)
			rooms[rid].players_weapon[i] = "0"
			rooms[rid].players_country[i] = -1
			rooms[rid].players_damaged[i] = 0
			rooms[rid].players_damaged_sender[i] = 0
			rooms[rid].players_dying[i] = 0
			rooms[rid].players_spine_rotation[i] = 0
			rooms[rid].players_crouch_rotation[i] = 0
			rooms[rid].players_pickup_rotation[i] = 0
			rooms[rid].players_vehicle[i] = 0
			rooms[rid].players_vehicle_1[i] = 0
			rooms[rid].players_vehicle_2[i] = 0
			rooms[rid].players_vehicle_3[i] = 0



			rooms[rid].scores[i] = 0


	
		while True:

			data = await asyncio.wait_for(websocket.receive_text(), 7.2)
			#data = await websocket.receive_text()

			#requests based on id of stream data received

			if (data == "requestId" or return_full_room or return_outdated_version): #returns id and map
				my_id = i
				if return_full_room: #server is full, drop connection
					my_id = -2
					#await asyncio.wait_for(websocket.close(), 7.2)
					await asyncio.wait_for(websocket.send_text("-2|0|0"), 7.2)
					raise Exception("server full")
				elif return_outdated_version:
					my_id = -3
					await asyncio.wait_for(websocket.send_text("-3|0|0"), 7.2)
					raise Exception("version mismatch")
				else:
					await asyncio.wait_for(websocket.send_text(str(my_id) + "|" + str(rooms[rid].map_id[0]) + "|" + str(rooms[rid].gamemode[0])), 7.2)

			elif (len(data.split("=")) > 2): #receive data and transfer back new information
				#print(data)
				datas = data.split("=")
				rooms[rid].players_position[i][0] = float(datas[0])
				rooms[rid].players_position[i][1] = float(datas[1])
				rooms[rid].players_position[i][2] = float(datas[2])

				rooms[rid].players_rotation[i][0] = float(datas[3])
				rooms[rid].players_rotation[i][1] = float(datas[4])
				rooms[rid].players_rotation[i][2] = float(datas[5])

				rooms[rid].players_moving[i] = int(datas[6])
				if (datas[7] != "0"):
					rooms[rid].players_shooting[i] = str(datas[7]) + "|" + str(time.time()).split(".")[0][7:] + str(time.time()).split(".")[1][:3]
				rooms[rid].players_country[i] = int(datas[10])
				rooms[rid].players_running[i] = int(datas[8])
				rooms[rid].players_weapon[i] = str(datas[9])

				if (datas[11] != "0"):
					damaged_player_id = 0
					for index, j in enumerate(datas[11].split("|")): #should only return 2 if player damaged one person (will returned multiples of that if more)
						if index % 2 == 0: #corresponding id
							damaged_player_id = int(j)
						elif rooms[rid].players_damaged.get(damaged_player_id) != None:
							rooms[rid].players_damaged[damaged_player_id] += float(j)
							rooms[rid].players_damaged_sender[damaged_player_id] = i #give uid of damaging player

				rooms[rid].players_dying[i] = int(datas[12].split("|")[0])
				if (datas[12].split("|")[1] != "" and int(datas[12].split("|")[1]) != -1):
					killer_id = int(datas[12].split("|")[1])
					if (killer_id % 2 != i % 2):
						rooms[rid].scores[killer_id] += 1
						if (killer_id % 2 == 0):
							rooms[rid].scores[-1] += 1
						else:
							rooms[rid].scores[-2] += 1
				if datas[13] != "": #add message
					add_message(rid, datas[13])

				rooms[rid].players_spine_rotation[i] = float(datas[14].split("|")[0])
				rooms[rid].players_crouch_rotation[i] = float(datas[15].split("|")[0])
				rooms[rid].players_pickup_rotation[i] = float(datas[16].split("|")[0])

				try:
					rooms[rid].players_vehicle[i] = int(datas[17])
					if (rooms[rid].players_vehicle[i] == 1): #tank properties
						rooms[rid].players_vehicle_1[i] = int(datas[18])
						rooms[rid].players_vehicle_2[i] = float(datas[19])
						rooms[rid].players_vehicle_3[i] = float(datas[20])
					elif (rooms[rid].players_vehicle[i] == 2): #airplane properties
						rooms[rid].players_vehicle_1[i] = int(datas[18])
						rooms[rid].players_vehicle_2[i] = float(datas[19])
						rooms[rid].players_vehicle_3[i] = 0

				except: #no vehicle information could be found
					print("old version without vehicle properties")
					rooms[rid].players_vehicle[i] = 0


				# return_text = ""
				# #print(rooms[rid].players_shooting.items())
				# for key, value in rooms[rid].players_position.items():
				# 	if (rooms[rid].players_position[key] != None):
				# 		temp_text = hex(key)[2] + "=" + str(value[0]) + "=" + str(value[1]) + "=" + str(value[2]) + "="
				# 		temp_text += str(rooms[rid].players_rotation[key][0]) + "=" + str(rooms[rid].players_rotation[key][1]) + "=" + str(rooms[rid].players_rotation[key][2]) + "="
				# 		temp_text += str(rooms[rid].players_moving[key]) + "=" + str(rooms[rid].players_shooting[key]) + "=" + str(rooms[rid].players_running[key]) + "="
						
				# 		temp_text += str(rooms[rid].players_weapon[key]) + "=" + str(rooms[rid].players_country[key]) + "=" + str(rooms[rid].players_damaged[key]) + "|"
				# 		try:
				# 			temp_text += str(rooms[rid].players_active[rooms[rid].players_damaged_sender[key]]) + "|"
				# 		except:
				# 			temp_text += "nul|"
				# 		temp_text += str(rooms[rid].players_damaged_sender[key]) + "="
				# 		temp_text += str(rooms[rid].players_dying[key]) + "=" + str(rooms[rid].players_spine_rotation[key]) + "=" + str(rooms[rid].players_crouch_rotation[key])+ "="
				# 		temp_text += str(rooms[rid].players_active[key]) + "=" + str(rooms[rid].players_pickup_rotation[key]) + "=" + str(rooms[rid].players_vehicle[key]) + "=" + str(rooms[rid].players_vehicle_1[key]) + "=" + str(rooms[rid].players_vehicle_2[key]) +"=" + str(rooms[rid].players_vehicle_3[key])+ "+"
				# 		return_text += temp_text
				
				# messages = ""
				# for ind, s in enumerate(rooms[rid].recent_messages):
				# 	messages += s
				# 	if ind < len(rooms[rid].recent_messages) - 1:
				# 		messages += "|"
				# return_text += messages + "+"
				# leaderboards = ""
				# index = 0
				# for ind, val in rooms[rid].scores.items():
				# 	if (val != None):
				# 		if (ind >= 0): #player kills
				# 			leaderboards += str(ind) + "=" + str(val)
				# 		elif (ind == -1): #team kills
				# 			leaderboards += "-1" + "=" + str(val)
				# 		elif (ind == -2):
				# 			leaderboards += "-2" + "=" + str(val)
				# 		if index < len(rooms[rid].scores) - 1:
				# 			leaderboards += "|"
				# 	index += 1
				# return_text += leaderboards + "+"
				# return_text += str(rooms[rid].timer[0])

				rooms[rid].players_damaged[i] = 0 #reset damage to player after player received damage

				txt = rooms[rid].to_JSON().replace("\n", "")
				await asyncio.wait_for(websocket.send_text(txt), 10)
				#await asyncio.wait_for(websocket.send_text(return_text), 7.1)
			else:
				print("format error on id " + str(i) + "in room " + str(rid))
				await asyncio.wait_for(websocket.send_text("invalid format"), 10)

	except Exception as E:

		print("Exception " + str(E) + " at line " + str(sys.exc_info()[-1].tb_lineno))
		print("connection dropped " + str(i) + " in room " + str(rid))
		if i != -1 and str(E) != "server full":
			#socket broken
			add_message(rid, player_name + " left the game")
			rooms[rid].remove_player(i)#"""
			if (rooms[rid].check_empty()):
				rooms[rid].reset_server()
	
uvicorn.run(app, port=8000, host="0.0.0.0")
#run as https
#uvicorn.run(app, port=8000, host="0.0.0.0", ssl_certfile="/root/ssl_certificates/fullchain.pem", ssl_keyfile="/root/ssl_certificates/privkey.pem")