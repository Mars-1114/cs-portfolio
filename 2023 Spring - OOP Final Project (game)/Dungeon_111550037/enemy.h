#ifndef _ENENY
#define _ENEMY

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include "room.h"
#include "entity.h"
#include "character.h"

class Enemy :public Character {
private:
	int id;
public:
	Enemy() = default;
	Enemy(int _id, string name, int room_id, int hp, int atk, int def, int lvl) :Character(name, room_id, hp, atk, def, lvl) {
		id = _id;
	}
	virtual void loadCheck(int);
	virtual void showStats() override;
	virtual void attackMessage(int, string);
};

vector<Enemy> enemySummon();

#endif