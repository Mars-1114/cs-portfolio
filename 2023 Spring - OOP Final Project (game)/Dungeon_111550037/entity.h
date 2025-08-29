#ifndef _ENTITY
#define _ENTITY

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "room.h"
using namespace std;


class Entity {
private:
	string name;
	int inRoomID;
public:
	Entity() = default;
	Entity(string _name, int id) {
		name = _name;
		inRoomID = id;
	}
	//check if the current room matches the assigned room.
	//load the entity if returns true.
	virtual void loadCheck() {};
	//show the info of the entity
	virtual void showStats() {};
	//set the id of the assigned room
	void setRoomID(int id) {
		inRoomID = id;
	}
	//get the name of the entity
	string getName() {
		return name;
	}
	//get the id of the assigned room
	int getRoomID() {
		return inRoomID;
	}
};

#endif
