#ifndef _ITEM
#define _ITEM

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include "entity.h"

class Item :public Entity {
private:
	int addHP;
	int addATK;
	int addDEF;
	int addCoin;
	int equippable;
	int specialized;
public:
	Item() = default;
	Item(string name, int id, int aHP, int aATK, int aDEF, int aCoin, int equip = 0, int special = 0) :Entity(name, id) {
		addHP = aHP;
		addATK = aATK;
		addDEF = aDEF;
		addCoin = aCoin;
		equippable = equip;
		specialized = special;
	}
	virtual void loadCheck(int);
	virtual void showStats();
	//set the addition health point
	void setHP(int _hp) {
		addHP = _hp;
	}
	//set the addition attack point
	void setATK(int _atk) {
		addATK = _atk;
	}
	//set the addition defence point
	void setDEF(int _def) {
		addDEF = _def;
	}
	//set the coin value
	void setCoin(int _c) {
		addCoin = _c;
	}
	void setSpecialized(int value) {
		specialized = value;
	}
	//get the addition health point
	int getHP() {
		return addHP;
	}
	//get the addition attack point
	int getATK() {
		return addATK;
	}
	//get the addition defence point
	int getDEF() {
		return addDEF;
	}
	//get the coin value
	int getCoin() {
		return addCoin;
	}
	//get the equippable value
	//0: false, 1: on weapon, 2: on armor, 3: consumable
	int getEquippable() {
		return equippable;
	}
	//get the specialized index
	//0: normal, 1: knight, 2: archer, 3: magus
	int getSpecialized() {
		return specialized;
	}
};

vector<Item> itemSummon();

#endif