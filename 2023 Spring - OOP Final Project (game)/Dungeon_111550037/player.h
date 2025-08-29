#ifndef _PLAYER
#define _PLAYER

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "entity.h"
#include "character.h"
#include "item.h"
#include "room.h"

class Player : public Character {
private:
	int prev_room;
	int occupation;
	int coin;
	int exp;
	Item* mainWeapon;
	Item* mainArmor;
	vector<Item> backpack;
public:
	Player() = default;
	Player(string name, int id, int occ, int _HP) :Character(name, id, _HP, 0, 0, 1) {
		prev_room = 0;
		occupation = occ;
		coin = 0;
		exp = 0;
		mainWeapon = NULL;
		mainArmor = NULL;
	}
	virtual void showStats();
	virtual void attackMessage(int, string);
	//update the player's health, attack, and defence status
	void updateStatus(int, int, int, int = 0);
	//add an item to the player's backpack
	void addItem(Item);
	//remove an item from the player's backpack
	void removeItem(int);
	//gain experience and level upgrade
	void gainExp(int);
	//check if the item specialized id matches the player's occupation
	void checkMatchedOcc(Item&);
	//get the occupation
	int getOcc() {
		return occupation;
	}
	//get the occupation name
	string getOccName() {
		if (occupation == 0) {
			return "Knight";
		}
		else if (occupation == 1) {
			return "Archer";
		}
		else if (occupation == 2) {
			return "Magus";
		}
		else {
			return "";
		}
	}
	//get the previous room
	int getPrevRoom() {
		return prev_room;
	}
	//set the previous room
	void setPrevRoom(int _id) {
		prev_room = _id;
	}
	//get the backpack
	vector<Item>* getBackpack() {
		return &backpack;
	}
	//get the experience
	int getExp() {
		return exp;
	}
	int getCoin() {
		return coin;
	}
	//set and change the main weapon
	void setWeapon(Item&);
	//set and change the main armor
	void setArmor(Item&);
	//get the main weapon
	Item* getWeapon() {
		return mainWeapon;
	}
	//get the main armor
	Item* getArmor() {
		return mainArmor;
	}
};

void occDesc(int);

#endif