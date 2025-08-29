#ifndef _ROOM
#define _ROOM

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <iostream>

class Room {
private:
	int north;
	int south;
	int east;
	int west;
	int id;
	int lockedDir;
	int triggerDir;

public:
	Room() {
		north = NULL;
		south = NULL;
		east = NULL;
		west = NULL;
		id = -1;
		lockedDir = 0;
		triggerDir = 0;
	}
	Room(int n, int s, int e, int w, int _id, int lock = 0, int trigger = 0) {
		north = n;
		south = s;
		east = e;
		west = w;
		id = _id;
		lockedDir = lock;
		triggerDir = trigger;
	}
	//get the room and the surrouned room
	//0: itself, 1: north, 2: south, 3: east, 4: west
	int getRoom(int n) {
		if (n == 0) {
			return id;
		}
		else if (n == 1) {
			return north;
		}
		else if (n == 2) {
			return south;
		}
		else if (n == 3) {
			return east;
		}
		else if (n == 4) {
			return west;
		}
		else {
			return -1;
		}
	}
	//get the locked direction
	int getLockDir() {
		return lockedDir;
	}
	int getTriggerDir() {
		return triggerDir;
	}
	void unlock(int type) {
		if (type == 0) {
			lockedDir = 0;
		}
		else if (type == 1){
			triggerDir = 0;
		}
	}
};

//create the dungeon map
Room* Generate();

#endif
