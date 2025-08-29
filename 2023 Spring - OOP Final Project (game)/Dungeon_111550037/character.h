#ifndef _CHARACTER
#define _CHARACTER

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include "entity.h"
#include "room.h"

class Character :public Entity {
private:
	int maxHP;
	int currentHP;
	int ATK;
	int DEF;
	int level;
public:
	Character() = default;
	Character(string name, int id, int _HP, int _ATK, int _DEF, int lvl) :Entity(name, id) {
		maxHP = _HP;
		currentHP = maxHP;
		ATK = _ATK;
		DEF = _DEF;
		level = lvl;
	}
	//check the death status
	bool isDead() {
		if (currentHP <= 0) {
			currentHP = 0;
			return true;
		}
		else {
			return false;
		}
	}
	//compute the damage a character take
	void takeDamage(int);
	//print hurt message
	virtual void attackMessage(int, string) = 0;
	//set max health point
	void setMaxHP(int mHP) {
		maxHP = mHP;
	}
	//set current health point
	void setCurHP(int curHP) {
		currentHP = curHP;
		if (currentHP > maxHP) {
			currentHP = maxHP;
		}
	}
	//set attack point
	void setATK(int atk) {
		ATK = atk;
	}
	//set defend point
	void setDEF(int def) {
		DEF = def;
	}
	void setLvl(int n) {
		level = n;
	}
	//get max health point
	int getMaxHP() {
		return maxHP;
	}
	//get current health point
	int getCurHP() {
		return currentHP;
	}
	//get attack point
	int getATK() {
		return ATK;
	}
	//get defend point
	int getDEF() {
		return DEF;
	}
	//get the skill level
	int getLvl() {
		return level;
	}
};

#endif